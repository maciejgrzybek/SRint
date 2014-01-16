using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRint
{
    namespace Communication
    {
        public struct ConnectionInfo
        {
            public string address { get; set; }
            public int port { get; set; }
        }
        public interface MessageSender
        {
            void Connect(string address, int port);
            void Disconnect();
            void SendMessage(byte[] message);
            ConnectionInfo? ConnectedToNodeInfo { get; }
        }
    }
}
