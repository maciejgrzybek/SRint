using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRint
{
    namespace Communication
    {
        public interface MessageSender
        {
            void Connect(string address, int port);
            void SendMessage(byte[] message);
        }
    }
}
