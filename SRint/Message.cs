using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProtoBuf;
using System.IO;

namespace SRint
{
    namespace Serialization
    {
        [ProtoContract]
        public sealed class Message
        {
            public enum MessageType
            {
                State = 0, Election = 1, Entry_request = 2
            }

            [ProtoMember(1, IsRequired = true)]
            public MessageType type { get; set; }

            [ProtoMember(2, IsRequired = false)]
            public Election? election_content { get; set; }

            [ProtoMember(3, IsRequired = false)]
            public State? state_content { get; set; }

            [ProtoContract]
            public struct Election
            {
                [ProtoMember(1, IsRequired = true)]
                public Int64 timestamp { get; set; }

                [ProtoMember(2, IsRequired = true)]
                public Int32 state_id { get; set; }

                [ProtoMember(3, IsRequired = true)]
                public Int32 node_id { get; set; }
            }

            [ProtoContract]
            public struct State
            {
                [ProtoMember(1, IsRequired = true)]
                public Int64 state_id { get; set; }

                [ProtoMember(2, IsRequired = false)]
                public List<NodeDescription> nodes { get; set; }

                [ProtoMember(3, IsRequired = false)]
                public List<Variable> variables { get; set; }
            }

            [ProtoContract]
            public sealed class NodeDescription
            {
                [ProtoMember(1, IsRequired = true)]
                public string ip { get; set; }

                [ProtoMember(2, IsRequired = true)]
                public Int32 port { get; set; }

                [ProtoMember(3, IsRequired = true)]
                public Int32 node_id;
            }

            [ProtoContract]
            public sealed class Variable
            {
                [ProtoMember(1, IsRequired = true)]
                public string name { get; set; }

                [ProtoMember(2, IsRequired = true)]
                public Int64 value { get; set; }

                [ProtoMember(3, IsRequired = false)]
                public List<NodeDescription> owners { get; set; }
            }
        }
    }
}
