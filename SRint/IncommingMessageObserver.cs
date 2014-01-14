using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRint
{
    namespace Communication
    {
        public enum MessageType
        {
            Listen,
            Recv
        }

        public interface IncommingMessageObserver
        {
            void OnIncommingMessage(string message, MessageType type);
        }
    }
}
