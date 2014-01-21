using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRint
{
    namespace Communication
    {
        abstract public class Message
        {}
        
        public class NetworkMessage : Message
        {
            public byte[] payload;
        }

        public class MetaMessage : Message
        {
            ZeroMQ.Monitoring.MonitorEvents ev;
        }
    }
}
