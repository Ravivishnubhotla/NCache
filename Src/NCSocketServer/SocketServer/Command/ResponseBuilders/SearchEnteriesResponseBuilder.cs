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
using Alachisoft.NCache.Caching.Queries;
using Alachisoft.NCache.Serialization.Formatters;
using System.Collections;
using Alachisoft.NCache.Common.DataStructures.Clustered;

namespace Alachisoft.NCache.SocketServer.Command.ResponseBuilders
{
    // Dated: July 20, 2011
    /// <summary>
    /// This class is responsible for providing the responses based on the command Version specified.
    /// Main role of this class is to provide the backward compatibility. As different version of command can
    /// be processed by the same server. In that case the response should be in the form understandable by the
    /// client who sent the command.
    /// 
    /// This class only processes the different versions of SearchEnteries command
    /// </summary>

    class SearchEnteriesResponseBuilder : ResponseBuilderBase
    {
        public static void BuildResponse(QueryResultSet resultSet, int commandVersion, string RequestId, IList _serializedResponse, int commandID, Caching.Cache cache, out int resultCount)
        {
            Alachisoft.NCache.SocketServer.Util.KeyPackageBuilder.Cache = cache;
            long requestId = Convert.ToInt64(RequestId);
            resultCount = 0;
            try
            {
                switch (commandVersion)
                {
                    case 0: // Version from NCache 3.8 to NCache 3.8 SP3
                        {
                            Alachisoft.NCache.Common.Protobuf.Response response = new Alachisoft.NCache.Common.Protobuf.Response();
                            Alachisoft.NCache.Common.Protobuf.SearchEntriesResponse searchEntriesResponse = new Alachisoft.NCache.Common.Protobuf.SearchEntriesResponse();
                            response.requestId = requestId;
                            response.commandID = commandID;
                            searchEntriesResponse.keyValuePackage = Alachisoft.NCache.SocketServer.Util.KeyPackageBuilder.PackageKeysValues(resultSet.SearchEntriesResult, searchEntriesResponse.keyValuePackage);
                            response.responseType = Alachisoft.NCache.Common.Protobuf.Response.Type.SEARCH_ENTRIES;
                            response.searchEntries = searchEntriesResponse;
                            _serializedResponse.Add(Alachisoft.NCache.Common.Util.ResponseHelper.SerializeResponse(response));
                        }
                        break;
                    case 1: // From Version 3.8 SP4 onwards // Offically announcing support in 4.1 // So not supporting 3.8 SP4 clients in case of aggregate functions with 4.1 Cache Server
                    case 2: // NCache 4.1 SP1
                        {
                            switch (resultSet.Type)
                            {
                                case QueryType.AggregateFunction:
                                    {
                                        Alachisoft.NCache.Common.Protobuf.Response response = new Alachisoft.NCache.Common.Protobuf.Response();
                                        Alachisoft.NCache.Common.Protobuf.SearchEntriesResponse searchEntriesResponse = new Alachisoft.NCache.Common.Protobuf.SearchEntriesResponse();
                                        searchEntriesResponse.queryResultSet = new Alachisoft.NCache.Common.Protobuf.QueryResultSet();
                                        response.requestId = requestId;
                                        response.commandID = commandID;
                                        searchEntriesResponse.queryResultSet.queryType = Alachisoft.NCache.Common.Protobuf.QueryType.AGGREGATE_FUNCTIONS;
                                        searchEntriesResponse.queryResultSet.aggregateFunctionType = (Alachisoft.NCache.Common.Protobuf.AggregateFunctionType)(int)resultSet.AggregateFunctionType;
                                        searchEntriesResponse.queryResultSet.aggregateFunctionResult = new Alachisoft.NCache.Common.Protobuf.DictionaryItem();
                                        searchEntriesResponse.queryResultSet.aggregateFunctionResult.key = resultSet.AggregateFunctionResult.Key.ToString();
                                        searchEntriesResponse.queryResultSet.aggregateFunctionResult.value = resultSet.AggregateFunctionResult.Value != null ? CompactBinaryFormatter.ToByteBuffer(resultSet.AggregateFunctionResult.Value, null) : null;
                                        response.responseType = Alachisoft.NCache.Common.Protobuf.Response.Type.SEARCH_ENTRIES;
                                        response.searchEntries = searchEntriesResponse;
                                        resultCount = 1;
                                        _serializedResponse.Add(Alachisoft.NCache.Common.Util.ResponseHelper.SerializeResponse(response));
                                    }
                                    break;

                                case QueryType.SearchEntries:
                                    {
                                        int sequenceId = 1;
                                        IList keyValuesPackageChuncks = (ClusteredArrayList)Alachisoft.NCache.SocketServer.Util.KeyPackageBuilder.PackageKeysValues(resultSet.SearchEntriesResult);
                                        Alachisoft.NCache.Common.Protobuf.Response response = new Alachisoft.NCache.Common.Protobuf.Response();
                                        Alachisoft.NCache.Common.Protobuf.SearchEntriesResponse searchEntriesResponse = new Alachisoft.NCache.Common.Protobuf.SearchEntriesResponse();
                                        searchEntriesResponse.queryResultSet = new Alachisoft.NCache.Common.Protobuf.QueryResultSet();
                                        response.requestId = requestId;
                                        response.commandID = commandID;
                                        searchEntriesResponse.queryResultSet.queryType = Alachisoft.NCache.Common.Protobuf.QueryType.SEARCH_ENTRIES;
                                        response.numberOfChuncks = keyValuesPackageChuncks.Count;
                                        response.responseType = Alachisoft.NCache.Common.Protobuf.Response.Type.SEARCH_ENTRIES;

                                        foreach (Alachisoft.NCache.Common.Protobuf.KeyValuePackageResponse package in keyValuesPackageChuncks)
                                        {
                                            response.sequenceId = sequenceId++;
                                            searchEntriesResponse.queryResultSet.searchKeyEnteriesResult = package;
                                            response.searchEntries = searchEntriesResponse;
                                            _serializedResponse.Add(Alachisoft.NCache.Common.Util.ResponseHelper.SerializeResponse(response));
                                        }
                                        if (resultSet != null && resultSet.SearchEntriesResult != null)
                                            resultCount = resultSet.SearchEntriesResult.Count;
                                    }
                                    break;
                                case QueryType.GroupByAggregateFunction:
                                    {
                                        Alachisoft.NCache.Common.Protobuf.Response response = new Alachisoft.NCache.Common.Protobuf.Response();
                                        Alachisoft.NCache.Common.Protobuf.SearchEntriesResponse searchEntriesResponse = new Alachisoft.NCache.Common.Protobuf.SearchEntriesResponse();
                                        searchEntriesResponse.queryResultSet = new Alachisoft.NCache.Common.Protobuf.QueryResultSet();

                                        searchEntriesResponse.queryResultSet.queryType = Alachisoft.NCache.Common.Protobuf.QueryType.GROUPBY_AGGREGATE_FUNCTIONS;

                                        Common.Protobuf.RecordSet groupByResult = new Common.Protobuf.RecordSet();

                                        searchEntriesResponse.queryResultSet.groupByAggregateFunctionResult = groupByResult;


                                        response.requestId = requestId;
                                        response.commandID = commandID;
                                        response.responseType = Alachisoft.NCache.Common.Protobuf.Response.Type.SEARCH_ENTRIES;
                                        response.searchEntries = searchEntriesResponse;
                                        _serializedResponse.Add(Alachisoft.NCache.Common.Util.ResponseHelper.SerializeResponse(response));
                                        resultCount = 1;
                                    }
                                    break;
                            }
                        }
                        break;
                    default:
                        {
                            throw new Exception("Unsupported Command Version " + commandVersion);
                        }
                }
            }
            catch (Exception ex)
            {
                if (SocketServer.Logger.IsErrorLogsEnabled)
                {
                    SocketServer.Logger.NCacheLog.Error(ex.ToString());
                    if (resultSet == null)
                    {
                        SocketServer.Logger.NCacheLog.Error("QueryResultSet is null");
                    }
                    else if (resultSet.AggregateFunctionResult.Key == null)
                    {
                        SocketServer.Logger.NCacheLog.Error("QueryResultSet.AggregateFunctionResult.Key is null");
                    }
                    else if (resultSet.AggregateFunctionResult.Value == null)
                    {
                        SocketServer.Logger.NCacheLog.Error("QueryResultSet.AggregateFunctionResult.Value is null");
                    }
                }
                throw;
            }
        }
    }
}
