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

// Generated from: SyncEventsCommand.proto
// Note: requires additional types generated from: EventIdCommand.proto
namespace Alachisoft.NCache.Common.Protobuf
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"SyncEventsCommand")]
  public partial class SyncEventsCommand : global::ProtoBuf.IExtensible
  {
    public SyncEventsCommand() {}
    

    private long _requestId = default(long);
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"requestId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(long))]
    public long requestId
    {
      get { return _requestId; }
      set { _requestId = value; }
    }
    private readonly global::System.Collections.Generic.List<Alachisoft.NCache.Common.Protobuf.EventIdCommand> _eventIds = new global::System.Collections.Generic.List<Alachisoft.NCache.Common.Protobuf.EventIdCommand>();
    [global::ProtoBuf.ProtoMember(2, Name=@"eventIds", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<Alachisoft.NCache.Common.Protobuf.EventIdCommand> eventIds
    {
      get { return _eventIds; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}
