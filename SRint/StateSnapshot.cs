using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRint
{
    public class StateSnapshot
    {
        public StateSnapshot()
        {
            variables = new List<protobuf.Message.Variable>();
            message = new protobuf.Message();
            message.state_content = new protobuf.Message.State();
        }
        public StateSnapshot(protobuf.Message message)
        {
            this.message = message;
            variables = message.state_content.variables;
        }
        public List<protobuf.Message.Variable> variables { get; set; }
        public protobuf.Message message { get; set; }
    }
}