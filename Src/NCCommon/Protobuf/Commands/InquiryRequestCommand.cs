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

// Generated from: InquiryRequestCommand.proto
namespace Alachisoft.NCache.Common.Protobuf
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"InquiryRequestCommand")]
  public partial class InquiryRequestCommand : global::ProtoBuf.IExtensible
  {
    public InquiryRequestCommand() {}
    

    private long _inquiryRequestId = default(long);
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"inquiryRequestId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(long))]
    public long inquiryRequestId
    {
      get { return _inquiryRequestId; }
      set { _inquiryRequestId = value; }
    }

    private long _inquiryCommandId = default(long);
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"inquiryCommandId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(long))]
    public long inquiryCommandId
    {
      get { return _inquiryCommandId; }
      set { _inquiryCommandId = value; }
    }

    private string _serverIP = "";
    [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name=@"serverIP", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string serverIP
    {
      get { return _serverIP; }
      set { _serverIP = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}