using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace SRint
{
    namespace Communication
    {
        public class Server : IncommingMessagesReceiver,
                              MessageSender
        {
            class ReceivingThread
            {
                public ReceivingThread(BlockingCollection<string> messagesCollection, ZMQ.Socket socket)
                {
                    this.socket = socket;
                    this.messagesCollection = messagesCollection;
                }

                public void StartReading()
                {
                    workIndicator = true;

                    while (workIndicator)
                    {
                        var received = socket.Recv(2000); // TODO parametrize timeout
                        if (received != null)
                            messagesCollection.Add(Encoding.ASCII.GetString(received));
                    }
                }

                public void StopReading()
                {
                    workIndicator = false;
                }

                private ZMQ.Socket socket;
                private BlockingCollection<string> messagesCollection;
                private volatile bool workIndicator = false;
            }

            public Server(string address = "tcp://*", int recvPort = 5555, int poolSize = 1)
            {
                context = new ZMQ.Context(poolSize);

                listenSocket = context.Socket(ZMQ.SocketType.PULL);
                BindToListenSocket(address, recvPort + 1);
                listener = new ReceivingThread(listenMessagesCollection, listenSocket);
                listeningThread = new Thread(listener.StartReading);

                recvSocket = context.Socket(ZMQ.SocketType.PULL);
                BindToRecvSocket(address, recvPort);
                receiver = new ReceivingThread(recvMessagesCollection, recvSocket);
                receivingThread = new Thread(receiver.StartReading);

                sendSocket = context.Socket(ZMQ.SocketType.PUSH);
                //sendSocket.Connect("tcp://192.168.43.72:5555");
                sendSocket.Connect("tcp://169.254.26.129:5555");
            }

            public void SendMessage(string message)
            {
                sendSocket.Send(message, Encoding.ASCII);
                //var recv = sendSocket.Recv(2000);
                //if (recv != null)
                //{
                //    Logger.Instance.LogNotice("Received in return: " + Encoding.ASCII.GetString(recv));
                //    protobuf.Message m = Serialization.MessageSerializer.Deserialize(recv);
                //    Logger.Instance.LogNotice("Message = " + m.type.ToString() + " " + m.state_content.state_id + " " + m.state_content.variables[0].name + " = " + m.state_content.variables[0].value);
                //}
                //else
                //    Logger.Instance.LogError("recv = null");
            }

            public void StartSocketPolling()
            {
                listeningThread.Start();
                receivingThread.Start();
            }

            public void StopSocketPolling()
            {
                listener.StopReading();
                receiver.StopReading();
                listeningThread.Join();
                receivingThread.Join();
                listeningThread = new Thread(listener.StartReading);
                receivingThread = new Thread(receiver.StartReading);
            }

            public void OperateOnce()
            {
                string listenMessage = ReadNextListenMessage();
                string recvMessage = ReadNextRecvMessage();

                if (listenMessage != null)
                    NotifyMessageObservers(listenMessage, MessageType.Listen);

                if (recvMessage != null)
                    NotifyMessageObservers(recvMessage, MessageType.Recv);
            }

            public string ReadNextRecvMessage()
            {
                string message;
                bool isAnyMessageInQueue = recvMessagesCollection.TryTake(out message, 1000); // TODO parametrize timeout
                if (!isAnyMessageInQueue)
                    return null;

                return message;
            }

            public string ReadNextListenMessage()
            {
                string message;
                bool isAnyMessageInQueue = listenMessagesCollection.TryTake(out message, 200); // TODO parametrize timeout
                if (!isAnyMessageInQueue)
                    return null;

                return message;
            }

            public void RegisterIncommingMessageObserver(IncommingMessageObserver observer)
            {
                incommingMessageObserverList.Add(observer);
            }

            private void NotifyMessageObservers(string message, MessageType type)
            {
                //foreach(var observer in incommingMessageObserverList)
                //{
                //    observer.OnIncommingMessage(message, type); 
                //}
                incommingMessageObserverList.ForEach(observer => observer.OnIncommingMessage(message, type));
            }

            private void BindToListenSocket(string addr, int port)
            {
                string address = "tcp://" + addr + ":" + port.ToString();
                listenSocket.Bind(address);
            }

            private void BindToRecvSocket(string addr, int port)
            {
                string address = "tcp://" +addr + ":" + port.ToString();
                recvSocket.Bind(address);
            }

            private List<IncommingMessageObserver> incommingMessageObserverList = new List<IncommingMessageObserver>();
            private ZMQ.Context context;
            private ZMQ.Socket listenSocket;
            private ZMQ.Socket recvSocket;
            private ZMQ.Socket sendSocket;

            private BlockingCollection<string> listenMessagesCollection = new BlockingCollection<string>();
            private BlockingCollection<string> recvMessagesCollection = new BlockingCollection<string>();

            private ReceivingThread listener;
            private Thread listeningThread;
            private ReceivingThread receiver;
            private Thread receivingThread;
        }

        class ServerPoller
        {
            public event EventHandler PollingStarted;
            public event EventHandler PollingFinished;

            public ServerPoller(Server server)
            {
                this.server = server;
            }

            public void Start()
            {
                Thread thread = new Thread(Operate);
                working = true;
                thread.Start();
            }

            public void RequestStop()
            {
                working = false;
            }

            private void Operate()
            {
                if (!working)
                    return;

                OnPollingStarted();
                server.StartSocketPolling();

                while (working)
                {
                    server.OperateOnce();
                }

                server.StopSocketPolling();

                OnPollingFinished();
            }

            private void OnPollingStarted()
            {
                OnPollingStarted(EventArgs.Empty);
            }

            private void OnPollingStarted(EventArgs e)
            {
                EventHandler handler = PollingStarted;
                if (handler != null)
                    handler(this, e);
            }

            private void OnPollingFinished()
            {
                OnPollingFinished(EventArgs.Empty);
            }

            private void OnPollingFinished(EventArgs e)
            {
                EventHandler handler = PollingFinished;
                if (handler != null)
                    handler(this, e);
            }

            private volatile bool working = false;
            private Server server;
        }

    }
}
