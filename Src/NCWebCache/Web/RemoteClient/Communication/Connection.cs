// Copyright (c) 2018 Alachisoft
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections;
#if NET40
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Alachisoft.NCache.Common;
using Alachisoft.NCache.Common.DataStructures.Clustered;
using Alachisoft.NCache.Common.Enum;
using Alachisoft.NCache.Common.Protobuf;
using Alachisoft.NCache.Common.Sockets;
using Alachisoft.NCache.Common.Threading;
using Alachisoft.NCache.Common.DataStructures;
using Alachisoft.NCache.Web.Caching.Util;
using Alachisoft.NCache.Web.Statistics;
using System.IO;
using Alachisoft.NCache.Runtime.Exceptions;
using Alachisoft.NCache.Web.Util;
using Alachisoft.NCache.Web.Command;
using Alachisoft.NCache.Common.Net;
using Exception = System.Exception;

namespace Alachisoft.NCache.Web.Communication
{
    internal sealed class Connection
    {
        #region --------Constants--------

        internal const int CmdSizeHolderBytesCount = 10;
        internal const int ValSizeHolderBytesCount = 10;
        internal const int TotSizeHolderBytesCount = ValSizeHolderBytesCount + CmdSizeHolderBytesCount;

        //10 bytes for the message size...
        private const int MessageHeader = 10;

        //Threshold for maximum number of commands in a request.
        private const int MaxCmdsThreshold = 100;

        #endregion

        #region private members

        private OnCommandRecieved _commandRecieved = null;
        private OnServerLost _serverLost = null;
        private bool _isConnected = true;
        private Socket _primaryClient = null;
        private Socket _secondaryClient = null;
        private IPAddress _address;
        private string _ipAddress = string.Empty;
        private string _intendedRecipientIPAddress = string.Empty;
        private int _port = 0;
        private object _connectionMutex = new object();
        private Latch _connectionStatusLatch = new Latch(ConnectionStatus.Disconnected);
        public static long s_receiveBufferSize = 2048000;
        private int _processID = System.Diagnostics.Process.GetCurrentProcess().Id;
        private string _cacheId;
        private Thread _primaryReceiveThread = null;
        private Thread _secondaryReceiveThread = null;
        private bool _notificationsRegistered = false;
        private bool _isReconnecting = false;
        private bool _forcedDisconnect = false;
        private bool _nagglingEnabled = false;
        private long _nagglingSize = 5 * 100 * 1024;
        private Alachisoft.NCache.Common.DataStructures.Queue _msgQueue;
        private bool _supportDualSocket = false;
        private object _syncLock = new object();
        private static IPEndPoint _bindIP;
        private Logs _logger;
        private ResponseIntegrator _responseIntegrator;
        private Address _serverAddress;
        private Broker _container;
        private PerfStatsCollector _perfStatsColl = null;
        private object _socketSelectionMutex = new object();
        private bool _usePrimary = true;
        private bool _requestLoggingEnabled;
        private bool _optimized = false;
        private int _hostPort;
        private bool _isIdle = false;

        #endregion

        internal Connection(Broker container, OnCommandRecieved commandRecieved, OnServerLost serverLost, Logs logs,
            PerfStatsCollector perfStatsCollector, ResponseIntegrator rspIntegraotr, string bindIP, string cacheName)
        {
            Initialize(container, commandRecieved, serverLost, logs, perfStatsCollector, rspIntegraotr, bindIP,
                cacheName);
        }

        #region Properties

        public bool Optimized
        {
            get { return _optimized; }
            set { _optimized = value; }
        }


        public bool SupportDualSocket
        {
            get { return _supportDualSocket; }
        }

        public Socket PrimaryClientSocket
        {
            get { return _primaryClient; }
        }

        public Socket SecondaryClientSocket
        {
            get { return _secondaryClient; }
        }

        internal Latch StatusLatch
        {
            get { return _connectionStatusLatch; }
        }

        /// <summary>
        /// Checks if request logging is enabled on this server or not.
        /// </summary>
        internal bool RequestInquiryEnabled
        {
            get { return _requestLoggingEnabled; }
            set { _requestLoggingEnabled = value; }
        }

        internal bool IsConnected
        {
            get { return _connectionStatusLatch.IsAnyBitsSet(ConnectionStatus.Connected); }
        }

        /// <summary>
        /// Get ip address of machine to which connection is made
        /// </summary>
        internal string IpAddress
        {
            get { return this._ipAddress; }
        }

        internal IPAddress Address
        {
            get { return this._address; }
        }

        internal Address ServerAddress
        {
            get { return this._serverAddress; }
            set { _serverAddress = value; }
        }

        internal string IntendedRecipientIPAddress
        {
            set { this._intendedRecipientIPAddress = value; }
            get { return this._intendedRecipientIPAddress; }
        }

        /// <summary>
        /// Get port on which connection is made
        /// </summary>
        internal int Port
        {
            get { return this._port; }
            private set { _port = value; }
        }

        internal bool NotifRegistered
        {
            get { return this._notificationsRegistered; }
            set { this._notificationsRegistered = value; }
        }

        internal bool IsReconnecting
        {
            get { return this._isReconnecting; }
            set { this._isReconnecting = value; }
        }

        /// <summary>
        /// Gets the prefered communication socket.
        /// </summary>
        private Socket CommunicationSocket
        {
            get
            {
                Socket selectedSocket = _primaryClient;
                if (SupportDualSocket)
                {
                    selectedSocket = _primaryClient;
                    lock (_socketSelectionMutex)
                    {
                        if (!_usePrimary) selectedSocket = _secondaryClient;
                        _usePrimary = !_usePrimary;
                    }
                }

                return selectedSocket;
            }
        }

        //Marking it where it needs to be...
        internal bool IsIdle
        {
            get
            {
                lock (_connectionMutex) return _isIdle;
            }
            set
            {
                lock (_connectionMutex) _isIdle = value;
            }
        }

        #endregion

        public string GetClientLocalIP()
        {
            string ip = string.Empty;
            if (PrimaryClientSocket != null)
            {
                if (this.IsConnected)
                {
                    IPEndPoint add = (IPEndPoint)PrimaryClientSocket.LocalEndPoint;
                    ip = add.Address.ToString();
                }
            }

            return ip;
        }

        //function that sets string provided to bindIP
        internal void SetBindIP(string value)
        {
            if (value != null && value != string.Empty)
            {
                try
                {
                    _bindIP = new IPEndPoint(IPAddress.Parse(value), 0);
                }
                catch (Exception)
                {
                }
            }
        }

        public override bool Equals(object obj)
        {
            Connection connection = obj as Connection;
            return (connection != null && this.IpAddress == connection.IpAddress);
        }

        public void Dispose()
        {
            if (_msgQueue != null && !_msgQueue.Closed)
            {
                _msgQueue.close(true);
                _msgQueue = null;
            }

          
        }

        internal bool Connect(IPAddress ipAddress, int port)
        {
            int retry = 0;
            this.Optimized = false;
            this._ipAddress = ipAddress.ToString();
            this._address = ipAddress;
            this._port = port;
            this._serverAddress = new Address(ipAddress, port);
            lock (_connectionMutex)
            {
                _primaryClient = PrepareToConnect(_primaryClient);

                IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
                while (retry < 3)
                {
                    try
                    {
                        _primaryClient.Connect(endPoint);
                        return true;
                    }
                    catch (Exception e)
                    {
                        if (_logger.IsErrorLogsEnabled)
                            _logger.NCacheLog.Error("Connection.Connect",
                                " can not connect to " + ipAddress + ":" + port + ". error: " + e.ToString());
                        if (e.Message.Contains(
                            "A connection attempt failed because the connected party did not properly respond after a period of time")
                        )
                        {
                            retry++;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            return false;
        }

        internal bool Connect(string hostName, int port)
        {
            return Connect(((IPAddress[])Dns.GetHostByName(hostName).AddressList)[0], port);
        }

        public void Init()
        {
            StartThread();
        }

        private Socket PrepareToConnect(Socket client)
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 1024 * 1024);
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 131072);
            client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);

            if (_bindIP != null)
            {
                try
                {
                    client.Bind(_bindIP);
                }
                catch (Exception)
                {
                    throw new Exception("Invalid bind-ip-address specified in client configuration");
                }
            }

            _forcedDisconnect = false;
            return client;
        }

        internal void ConnectSecondarySocket(IPAddress address, int port)
        {
            _secondaryClient = PrepareToConnect(_secondaryClient);
            IPEndPoint endPoint = new IPEndPoint(address, port);

            try
            {
                _secondaryClient.Connect(endPoint);
            }
            catch (Exception e)
            {
                if (_logger.IsErrorLogsEnabled)
                    _logger.NCacheLog.Error("Connection.Connect",
                        " can not connect to " + address + ":" + port + ". error: " + e.ToString());
            }
        }

        /// <summary>
        /// it transfers the existing connection to a new connection without changing the object container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="commandRecieved"></param>
        /// <param name="serverLost"></param>
        /// <param name="logs"></param>
        /// <param name="perfStatsCollector"></param>
        /// <param name="rspIntegraotr"></param>
        /// <param name="bindIP"></param>
        /// <param name="cacheName"></param>
        /// <param name="ipAddress"></param>
        /// <param name="cachePort"></param>
        /// <returns></returns>
        public bool SwitchTo(Broker container, OnCommandRecieved commandRecieved, OnServerLost serverLost, Logs logs,
            PerfStatsCollector perfStatsCollector, ResponseIntegrator rspIntegraotr, string bindIP, string cacheName,
            IPAddress ipAddress, int cachePort)
        {
            int oldPort = Port;
            Initialize(container, commandRecieved, serverLost, logs, perfStatsCollector, rspIntegraotr, bindIP,
                cacheName);
            if (this.Connect(Address, cachePort))
            {
                _hostPort = cachePort;
                this.Port = oldPort;
                return true;
            }

            return false;
        }

        internal void Disconnect()
        {
            Disconnect(true);
        }

        internal void Disconnect(bool changeStatus)
        {
            _forcedDisconnect = true;
            if (_primaryReceiveThread != null && _primaryReceiveThread.ThreadState != ThreadState.Aborted &&
                _primaryReceiveThread.ThreadState != ThreadState.AbortRequested)
            {
                try
                {
                    if (_logger.NCacheLog != null) _logger.NCacheLog.Flush();
#if !NETCORE
                    _primaryReceiveThread.Abort();
#else
                    _primaryReceiveThread.Interrupt();
#endif
                }
                catch (System.Threading.ThreadAbortException)
                {
                }
                catch (System.Threading.ThreadInterruptedException)
                {
                }

                _primaryReceiveThread = null;
            }

            if (_primaryClient != null && _primaryClient.Connected)
            {
                try
                {
                    _primaryClient.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException)
                {
                }
                catch (ObjectDisposedException)
                {
                }

                _primaryClient.Close();
            }

            //dispose the secondary socket
            if (_secondaryReceiveThread != null && _secondaryReceiveThread.ThreadState != ThreadState.Aborted &&
                _secondaryReceiveThread.ThreadState != ThreadState.AbortRequested)
            {
                try
                {
                    if (_logger.NCacheLog != null) _logger.NCacheLog.Flush();
#if !NETCORE
                    _secondaryReceiveThread.Abort();
#else
                    _secondaryReceiveThread.Interrupt();
#endif
                }
                catch (System.Threading.ThreadAbortException)
                {
                }
                catch (System.Threading.ThreadInterruptedException)
                {
                }

                _secondaryReceiveThread = null;
            }


            if (_secondaryClient != null && _secondaryClient.Connected)
            {
                try
                {
                    _secondaryClient.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException)
                {
                }
                catch (ObjectDisposedException)
                {
                }

                _secondaryClient.Close();
                _secondaryClient = null;
            }

            if (changeStatus)
                this._connectionStatusLatch.SetStatusBit(ConnectionStatus.Disconnected, ConnectionStatus.Connected);
        }

        public CommandResponse RecieveCommandResponse()
        {
            return RecieveCommandResponse(_primaryClient);
        }

        public CommandResponse RecieveCommandResponse(Socket client)
        {
            byte[] value = null;
            CommandResponse cmdRespose = null;
            try
            {
                value = AssureRecieve(client, Optimized);

                ///Deserialize the response
                Alachisoft.NCache.Common.Protobuf.Response response = null;
                using (MemoryStream stream = new MemoryStream(value))
                {
                    response = ProtoBuf.Serializer.Deserialize<Alachisoft.NCache.Common.Protobuf.Response>(stream);
                    stream.Close();
                }

                if (response != null && response.responseType ==
                    Alachisoft.NCache.Common.Protobuf.Response.Type.RESPONSE_FRAGMENT)
                {
                    response = _responseIntegrator.AddResponseFragment(this._serverAddress,
                        response.getResponseFragment);
                }

                if (response != null)
                {
                    cmdRespose = new CommandResponse(false, new Address());
                    cmdRespose.CacheId = this._cacheId;
                    cmdRespose.Result = response;
                }
            }
            catch (SocketException e)
            {
                throw new ConnectionException(e.Message, this._serverAddress.IpAddress, this._serverAddress.Port);
            }

            return cmdRespose;
        }


        internal void AssureSend(byte[] buffer, Socket client, bool checkConnected)
        {
            int dataSent = 0, dataLeft = buffer.Length;
            lock (_connectionMutex)
            {
                if (checkConnected &&
                    _connectionStatusLatch.IsAnyBitsSet(ConnectionStatus.Disconnected | ConnectionStatus.Connecting))
                {
                    throw new ConnectionException(this._serverAddress.IpAddress, this._serverAddress.Port);
                }

                while (dataSent < buffer.Length)
                {
                    try
                    {
                        dataLeft = buffer.Length - dataSent;
                        dataSent += client.Send(buffer, dataSent, dataLeft, SocketFlags.None);
                    }
                    catch (SocketException se)
                    {
                        if (se.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                        {
                        }
                        else
                        {
                            if (_logger.IsErrorLogsEnabled)
                                _logger.NCacheLog.Error("Connection.AssureSend().SocketException ", se.ToString());
                            _connectionStatusLatch.SetStatusBit(ConnectionStatus.Disconnected,
                                ConnectionStatus.Connected);
                            throw new ConnectionException(this._serverAddress.IpAddress, this._serverAddress.Port);
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        if (_logger.IsErrorLogsEnabled)
                            _logger.NCacheLog.Error("Connection.AssureSend", "socket is already closed");
                        if (_connectionStatusLatch.IsAnyBitsSet(ConnectionStatus.Connected))
                            _connectionStatusLatch.SetStatusBit(ConnectionStatus.Disconnected,
                                ConnectionStatus.Connected);
                        throw new ConnectionException(this._serverAddress.IpAddress, this._serverAddress.Port);
                    }
                }
            }
        }

        /// <summary>
        /// This method is used by the naggling manager to send the naggled data.
        /// We pass a fixed sized buffer to this method that contains the naggled data.
        /// One extra argument 'bytesToSent' tells how many bytes we need to send from this buffer.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="buffer"></param>
        /// <param name="bytesToSent"></param>
        /// <param name="checkConnected"></param>
        internal void AssureSend(Socket client, byte[] buffer, int bytesToSent, bool checkConnected)
        {
            int dataSent = 0, dataLeft = bytesToSent;
            lock (_connectionMutex)
            {
                if (checkConnected &&
                    _connectionStatusLatch.IsAnyBitsSet(ConnectionStatus.Disconnected | ConnectionStatus.Connecting))
                {
                    throw new ConnectionException(this._serverAddress.IpAddress, this._serverAddress.Port);
                }

                while (dataSent < bytesToSent)
                {
                    try
                    {
                        dataLeft = bytesToSent - dataSent;
                        dataSent += client.Send(buffer, dataSent, dataLeft, SocketFlags.None);
                    }
                    catch (SocketException se)
                    {
                        if (se.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                        {
                        }
                        else
                        {
                            if (_logger.IsErrorLogsEnabled)
                                _logger.NCacheLog.Error("Connection.AssureSend", se.ToString());
                            _connectionStatusLatch.SetStatusBit(ConnectionStatus.Disconnected,
                                ConnectionStatus.Connected);
                            throw new ConnectionException(this._serverAddress.IpAddress, this._serverAddress.Port);
                        }
                    }
                }

                IsIdle = false;
            }
        }

        private void Initialize(Broker container, OnCommandRecieved commandRecieved, OnServerLost serverLost, Logs logs,
            PerfStatsCollector perfStatsCollector, ResponseIntegrator rspIntegraotr, string bindIP, string cacheName)
        {
            _commandRecieved = null;
            _serverLost = null;
            _isConnected = true;
            _primaryClient = null;
            _secondaryClient = null;
            _ipAddress = string.Empty;
            _intendedRecipientIPAddress = string.Empty;
            _port = 0;
            _connectionMutex = new object();
            _connectionStatusLatch = new Latch(ConnectionStatus.Disconnected);
            s_receiveBufferSize = 2048000;
            _processID = System.Diagnostics.Process.GetCurrentProcess().Id;
            _primaryReceiveThread = null;
            _secondaryReceiveThread = null;
            _notificationsRegistered = false;
            _isReconnecting = false;
            _forcedDisconnect = false;
            _nagglingEnabled = false;
            _nagglingSize = 5 * 100 * 1024; //500k
            _supportDualSocket = false;
            _syncLock = new object();
            _perfStatsColl = null;
            _socketSelectionMutex = new object();
            _usePrimary = true;
            _optimized = false;
            _isIdle = false;
            _container = container;
            _commandRecieved = commandRecieved;
            _serverLost = serverLost;
            _logger = logs;
            _responseIntegrator = rspIntegraotr;
            _cacheId = cacheName;
            _perfStatsColl = perfStatsCollector;

            SetBindIP(bindIP);
            if (System.Configuration.ConfigurationSettings.AppSettings["EnableNaggling"] != null)
                _nagglingEnabled =
                    Convert.ToBoolean(System.Configuration.ConfigurationSettings.AppSettings["EnableNaggling"]);

            //read the naggling size from app.config and covert it to bytes.
            if (System.Configuration.ConfigurationSettings.AppSettings["NagglingSize"] != null)
                _nagglingSize =
                    1024 * Convert.ToInt64(System.Configuration.ConfigurationSettings.AppSettings["NagglingSize"]);

            if (System.Configuration.ConfigurationSettings.AppSettings["EnableDualSockets"] != null)
                _supportDualSocket =
                    Convert.ToBoolean(System.Configuration.ConfigurationSettings.AppSettings["EnableDualSockets"]);
        }

        private byte[] AssureRecieve(Socket client, bool optimized)
        {
            byte[] buffer = new byte[CmdSizeHolderBytesCount + (optimized ? CmdSizeHolderBytesCount : 0)];

            AssureRecieve(ref buffer, client);

            int commandSize;
            try
            {
                commandSize = HelperFxn.ToInt32(buffer, 0 + (optimized ? CmdSizeHolderBytesCount : 0),
                    CmdSizeHolderBytesCount);
            }
            catch (InvalidCastException)
            {
                string str = System.Text.UTF8Encoding.UTF8.GetString(buffer);
                if (_logger.IsErrorLogsEnabled) _logger.NCacheLog.Error("AssureReceive", str);
                throw;
            }

            if (commandSize == 0)
            {
                return new byte[0];
            }

            buffer = new byte[commandSize];

            AssureRecieve(ref buffer, client);
            return buffer;
        }

        private void AssureRecieve(ref byte[] buffer, Socket client)
        {
            int totalBytesRecieved = 0;
            int bytesRecieved = 0;
            do
            {
                try
                {
                    bytesRecieved = client.Receive(buffer, totalBytesRecieved, buffer.Length - totalBytesRecieved,
                        SocketFlags.None);
                    if (bytesRecieved == 0) throw new SocketException((int)SocketError.ConnectionReset);
                    totalBytesRecieved += bytesRecieved;
                }
                catch (SocketException se)
                {
                    if (se.SocketErrorCode != SocketError.NoBufferSpaceAvailable) throw;
                }
            } while (totalBytesRecieved < buffer.Length);

            IsIdle = false;
        }


        internal void SendCommand(byte[] commandBytes, bool checkConnected)
        {
            if (_perfStatsColl.IsEnabled)
                _perfStatsColl.IncrementClientRequestsPerSecStats(1);


            byte[] dataWithSize = new byte[commandBytes.Length + MessageHeader];
            byte[] lengthBytes = HelperFxn.ToBytes(commandBytes.Length.ToString());

            Array.Copy(lengthBytes, 0, dataWithSize, 0, lengthBytes.Length);
            Array.Copy(commandBytes, 0, dataWithSize, MessageHeader, commandBytes.Length);

 
            if (SupportDualSocket)
            {
                Socket selectedSocket = _primaryClient;
                lock (_socketSelectionMutex)
                {
                    if (!_usePrimary) selectedSocket = _secondaryClient;
                    _usePrimary = !_usePrimary;
                }

                AssureSend(dataWithSize, selectedSocket, checkConnected);
            }
            else
            {
                AssureSend(dataWithSize, _primaryClient, checkConnected);
            }
            
        }

        private void OnServerLost()
        {
            if (_forcedDisconnect)
            {
                if (_logger.IsErrorLogsEnabled)
                    _logger.NCacheLog.Error("Connection.ReceivedThread", "Connection with server lost gracefully");
            }
            else if (_logger.IsErrorLogsEnabled)
                _logger.NCacheLog.Error("Connection.ReceivedThread",
                    "An established connection with the server " + _serverAddress + " is lost.");

            if (!_forcedDisconnect)
                _connectionStatusLatch.SetStatusBit(ConnectionStatus.Disconnected, ConnectionStatus.Connected);
            _serverLost(_serverAddress, _forcedDisconnect);
        }

        internal void StartThread()
        {
            _primaryReceiveThread = new Thread(new ParameterizedThreadStart(RecieveThread));
            _primaryReceiveThread.Priority = ThreadPriority.AboveNormal;
            _primaryReceiveThread.IsBackground = true;
            _primaryReceiveThread.Start(_primaryClient);

            if (SupportDualSocket)
            {
                _secondaryReceiveThread = new Thread(new ParameterizedThreadStart(RecieveThread));
                _secondaryReceiveThread.Priority = ThreadPriority.AboveNormal;
                _secondaryReceiveThread.IsBackground = true;
                _secondaryReceiveThread.Start(_secondaryClient);
            }
        }

        private void RecieveThread(object clientSocket)
        {
            while (true)
            {
                try
                {
                    byte[] cmdBytes;
                    Socket client = null;

                    if (clientSocket != null && clientSocket is Socket)
                    {
                        client = clientSocket as Socket;
                    }

                    cmdBytes = AssureRecieve(client, false);

                    using (Stream tempStream = new ClusteredMemoryStream(cmdBytes))
                    {
                        tempStream.Position = 0;
                        while (tempStream.Position < tempStream.Length)
                        {
                            ProcessResponse(tempStream);
                        }
                    }
                }
                catch (SocketException se)
                {
                    OnConnectionBroken(se, ExType.Socket);
                    break;
                }
                catch (IOException ie)
                {
                    //System.IOException is going to thrown in the case SslStream when the connection gets forcibly closed.
                    //Thus we are going to handle it as a SocketException....
                    OnConnectionBroken(ie, ExType.Socket);
                    break;
                }
                catch (ConnectionException ce)
                {
                    OnConnectionBroken(ce, ExType.Connection);
                    break;
                }
                catch (ThreadAbortException te)
                {
                    OnConnectionBroken(te, ExType.Abort);
                    break;
                }
                catch (ThreadInterruptedException tae)
                {
                    OnConnectionBroken(tae, ExType.Interrupt);
                    break;
                }
                catch (Exception e)
                {
                    OnConnectionBroken(e, ExType.General);
                }
            }
        }

        private void OnConnectionBroken(Exception e, ExType exType)
        {
            switch (exType)
            {
                case ExType.Socket:
                case ExType.Connection:
                    if (_forcedDisconnect)
                    {
                        if (_logger.IsErrorLogsEnabled)
                            _logger.NCacheLog.Error("Connection.ReceivedThread",
                                "Connection with server lost gracefully");
                    }
                    else if (_logger.IsErrorLogsEnabled)
                        _logger.NCacheLog.Error("Connection.ReceivedThread",
                            "An established connection with the server " + _serverAddress + " is lost. Error:" +
                            e.ToString());

                    if (!_forcedDisconnect)
                        _connectionStatusLatch.SetStatusBit(ConnectionStatus.Disconnected, ConnectionStatus.Connected);
                    _primaryReceiveThread = null;
                    _serverLost(_serverAddress, _forcedDisconnect);
                    break;
                case ExType.Interrupt:
                case ExType.Abort:
                    if (AppDomain.CurrentDomain.IsFinalizingForUnload()) return;
                    if (_forcedDisconnect)
                    {
                        if (_logger.IsErrorLogsEnabled)
                        {
                            _logger.NCacheLog.Error("Connection.ReceivedThread",
                                "Connection with server lost gracefully");
                            _logger.NCacheLog.Flush();
                        }
                    }

                    if (!_forcedDisconnect)
                        _connectionStatusLatch.SetStatusBit(ConnectionStatus.Disconnected, ConnectionStatus.Connected);
                    _serverLost(_serverAddress, _forcedDisconnect);
                    break;
                case ExType.General:
                    if (_logger.IsErrorLogsEnabled)
                    {
                        _logger.NCacheLog.Error("Connection.ReceivedThread", e.ToString());
                        _logger.NCacheLog.Flush();
                    }

                    if (!_forcedDisconnect)
                        _connectionStatusLatch.SetStatusBit(ConnectionStatus.Disconnected, ConnectionStatus.Connected);
                    _serverLost(_serverAddress, _forcedDisconnect);
                    break;
            }
        }

        /// <summary>
        /// Reads a single single response from the stream and processes it.
        /// </summary>
        /// <param name="stream"></param>
        private void ProcessResponse(Stream stream)
        {
            Response response;

            //Reading  a response's header...
            byte[] cmdSzBytes = new byte[CmdSizeHolderBytesCount];
            stream.Read(cmdSzBytes, 0, CmdSizeHolderBytesCount);
            int commandSize = HelperFxn.ToInt32(cmdSzBytes, 0, CmdSizeHolderBytesCount);

            byte[] cmdBytes = new byte[commandSize];
            stream.Read(cmdBytes, 0, commandSize);

            using (Stream tempStream = new ClusteredMemoryStream(cmdBytes))
            {
                response = ProtoBuf.Serializer.Deserialize<Response>(tempStream);
            }

            CommandResponse cmdRespose = null;
            if (response != null)
            {
                cmdRespose = new CommandResponse(false, new Address());
                cmdRespose.CacheId = _cacheId;
                cmdRespose.Result = response;
            }

            if (_perfStatsColl.IsEnabled)
            {
                _perfStatsColl.IncrementClientResponsesPerSecStats(1);
            }

            if (cmdRespose != null)
            {
                _container.ProcessResponse(cmdRespose, _serverAddress);
            }
        }
    }
}