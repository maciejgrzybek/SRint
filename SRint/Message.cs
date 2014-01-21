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
        {}

        public class CommunicationMetaMessage : Message
        {
            public ZeroMQ.ZmqSocket socket;
        }

        public class ConnectedCommunicationMetaMessage : CommunicationMetaMessage
        {}

        public class ConnectDelayedCommunicationMetaMessage : CommunicationMetaMessage
        {}

        public class ConnectRetiredCommunicationMetaMessage : CommunicationMetaMessage
        {}

        public class ListeningCommunicationMetaMessage : CommunicationMetaMessage
        {}

        public class BindFailedCommunicationMetaMessage : CommunicationMetaMessage
        {}

        public class AcceptedCommunicationMetaMessage : CommunicationMetaMessage
        {}

        public class AcceptFailedCommunicationMetaMessage : CommunicationMetaMessage
        {}

        public class ClosedCommunicationMetaMessage : CommunicationMetaMessage
        {}

        public class CloseFailedCommunicationMetaMessage : CommunicationMetaMessage
        {}

        public class DisconnectedCommunicationMetaMessage : CommunicationMetaMessage
        {}
    }
}
