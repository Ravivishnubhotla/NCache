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

// Generated from: RegisterBulkKeyNotifCommand.proto
namespace Alachisoft.NCache.Common.Protobuf
{
    [global::System.Serializable, global::ProtoBuf.ProtoContract(Name = @"RegisterBulkKeyNotifCommand")]
    public partial class RegisterBulkKeyNotifCommand : global::ProtoBuf.IExtensible
    {
        public RegisterBulkKeyNotifCommand() { }


        private int _removeCallbackId = default(int);
        [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name = @"removeCallbackId", DataFormat = global::ProtoBuf.DataFormat.ZigZag)]
        [global::System.ComponentModel.DefaultValue(default(int))]
        public int removeCallbackId
        {
            get { return _removeCallbackId; }
            set { _removeCallbackId = value; }
        }

        private int _updateCallbackId = default(int);
        [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name = @"updateCallbackId", DataFormat = global::ProtoBuf.DataFormat.ZigZag)]
        [global::System.ComponentModel.DefaultValue(default(int))]
        public int updateCallbackId
        {
            get { return _updateCallbackId; }
            set { _updateCallbackId = value; }
        }
        private readonly global::System.Collections.Generic.List<string> _keys = new global::System.Collections.Generic.List<string>();
        [global::ProtoBuf.ProtoMember(3, Name = @"keys", DataFormat = global::ProtoBuf.DataFormat.Default)]
        public global::System.Collections.Generic.List<string> keys
        {
            get { return _keys; }
        }


        private long _requestId = default(long);
        [global::ProtoBuf.ProtoMember(4, IsRequired = false, Name = @"requestId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(long))]
        public long requestId
        {
            get { return _requestId; }
            set { _requestId = value; }
        }

        private bool _notifyOnExpiration = (bool)true;
        [global::ProtoBuf.ProtoMember(5, IsRequired = false, Name = @"notifyOnExpiration", DataFormat = global::ProtoBuf.DataFormat.Default)]
        [global::System.ComponentModel.DefaultValue((bool)true)]
        public bool notifyOnExpiration
        {
            get { return _notifyOnExpiration; }
            set { _notifyOnExpiration = value; }
        }

        private int _datafilter = (int)-1;
        [global::ProtoBuf.ProtoMember(6, IsRequired = false, Name = @"datafilter", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue((int)-1)]
        public int datafilter
        {
            get { return _datafilter; }
            set { _datafilter = value; }
        }

        private int _callbackType = (int)2;
        [global::ProtoBuf.ProtoMember(7, IsRequired = false, Name = @"callbackType", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue((int)2)]
        public int callbackType
        {
            get { return _callbackType; }
            set { _callbackType = value; }
        }

        private string _surrogateClientID = "";
        [global::ProtoBuf.ProtoMember(8, IsRequired = false, Name = @"surrogateClientID", DataFormat = global::ProtoBuf.DataFormat.Default)]
        [global::System.ComponentModel.DefaultValue("")]
        public string surrogateClientID
        {
            get { return _surrogateClientID; }
            set { _surrogateClientID = value; }
        }
        private global::ProtoBuf.IExtension extensionObject;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
    }

}
