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
// limitations under the License

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: GetNextRecordResponse.proto
namespace Alachisoft.NCache.Common.Protobuf
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"GetNextRecordResponse")]
  public partial class GetNextRecordResponse : global::ProtoBuf.IExtensible
  {
    public GetNextRecordResponse() {}
    

    private byte[] _Key = null;
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"Key", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(null)]
    public byte[] Key
    {
      get { return _Key; }
      set { _Key = value; }
    }

    private byte[] _Value = null;
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"Value", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(null)]
    public byte[] Value
    {
      get { return _Value; }
      set { _Value = value; }
    }

    private string _ClientId = "";
    [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name=@"ClientId", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string ClientId
    {
      get { return _ClientId; }
      set { _ClientId = value; }
    }

    private string _ClientIp = "";
    [global::ProtoBuf.ProtoMember(4, IsRequired = false, Name=@"ClientIp", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string ClientIp
    {
      get { return _ClientIp; }
      set { _ClientIp = value; }
    }

    private int _ClientPort = default(int);
    [global::ProtoBuf.ProtoMember(5, IsRequired = false, Name=@"ClientPort", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int ClientPort
    {
      get { return _ClientPort; }
      set { _ClientPort = value; }
    }

    private string _ClusterIp = "";
    [global::ProtoBuf.ProtoMember(6, IsRequired = false, Name=@"ClusterIp", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string ClusterIp
    {
      get { return _ClusterIp; }
      set { _ClusterIp = value; }
    }

    private int _ClusterPort = default(int);
    [global::ProtoBuf.ProtoMember(7, IsRequired = false, Name=@"ClusterPort", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int ClusterPort
    {
      get { return _ClusterPort; }
      set { _ClusterPort = value; }
    }

    private int _CallbackId = default(int);
    [global::ProtoBuf.ProtoMember(8, IsRequired = false, Name=@"CallbackId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int CallbackId
    {
      get { return _CallbackId; }
      set { _CallbackId = value; }
    }

    private string _TaskId = "";
    [global::ProtoBuf.ProtoMember(9, IsRequired = false, Name=@"TaskId", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string TaskId
    {
      get { return _TaskId; }
      set { _TaskId = value; }
    }

    private bool _IsLastResult = default(bool);
    [global::ProtoBuf.ProtoMember(10, IsRequired = false, Name=@"IsLastResult", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(default(bool))]
    public bool IsLastResult
    {
      get { return _IsLastResult; }
      set { _IsLastResult = value; }
    }

    private string _NodeAddress = "";
    [global::ProtoBuf.ProtoMember(11, IsRequired = false, Name=@"NodeAddress", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string NodeAddress
    {
      get { return _NodeAddress; }
      set { _NodeAddress = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}
