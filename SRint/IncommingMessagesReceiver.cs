using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRint
{
    namespace Communication
    {
        public interface IncommingMessagesReceiver
        {
            void StartSocketPolling();
            void StopSocketPolling();
            //string ReadNextMessage();
            void RegisterIncommingMessageObserver(IncommingMessageObserver observer);
        }
    }
}
