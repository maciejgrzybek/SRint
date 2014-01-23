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
            message = new protobuf.Message();
            message.state_content = new protobuf.Message.State();
        }
        public StateSnapshot(protobuf.Message message)
        {
            this.message = message;
        }
        public protobuf.Message message { get; set; }
    }
}