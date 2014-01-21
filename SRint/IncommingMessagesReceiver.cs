using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRint
{
    namespace Communication
    {
        public delegate void IncommingMessageHandler(Message message);
        public interface IncommingMessagesReceiver
        {
            event IncommingMessageHandler OnIncommingMessage;
            void StartSocketPolling();
            void StopSocketPolling();
            //string ReadNextMessage();
        }
    }
}
