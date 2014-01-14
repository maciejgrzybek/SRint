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
                public ReceivingThread(BlockingCollection<byte[]> messagesCollection, ZMQ.Socket socket)
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
                        if (received != null && received.Length > 0)
                            messagesCollection.Add(received);
                    }
                }

                public void StopReading()
                {
                    workIndicator = false;
                }

                private ZMQ.Socket socket;
                private BlockingCollection<byte[]> messagesCollection;
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

                // FIXME remove everything below

                protobuf.Message.State state = new protobuf.Message.State { state_id = 15 };
                state.nodes.Add(new protobuf.Message.NodeDescription { node_id = 31, ip = "127.0.0.1", port = 5555 });

                protobuf.Message.Variable v = new protobuf.Message.Variable { name = "dupa", value = 72 };
                v.owners.Add(new protobuf.Message.NodeDescription { node_id = 31, ip = "127.0.0.1", port = 5555 });
                state.variables.Add(v);

                protobuf.Message m = new protobuf.Message { type = protobuf.Message.MessageType.STATE, state_content = state };
                var arr = Serialization.MessageSerializer.Serialize(m);

                protobuf.Message ms = Serialization.MessageSerializer.Deserialize(arr);

                //SendMessage(arr);
            }

            public void SendMessage(byte[] message)
            {
                sendSocket.Send(message);
            }
            //public void SendMessage(string message)
            //{
             //   sendSocket.Send(message, Encoding.ASCII);
                //var recv = sendSocket.Recv(2000);
                //if (recv != null)
                //{
                //    Logger.Instance.LogNotice("Received in return: " + Encoding.ASCII.GetString(recv));
                //    protobuf.Message m = Serialization.MessageSerializer.Deserialize(recv);
                //    Logger.Instance.LogNotice("Message = " + m.type.ToString() + " " + m.state_content.state_id + " " + m.state_content.variables[0].name + " = " + m.state_content.variables[0].value);
                //}
                //else
                //    Logger.Instance.LogError("recv = null");
            //}

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
                byte[] listenMessage = ReadNextListenMessage();
                byte[] recvMessage = ReadNextRecvMessage();

                if (listenMessage != null)
                    NotifyMessageObservers(listenMessage, MessageType.Listen);

                if (recvMessage != null)
                    NotifyMessageObservers(recvMessage, MessageType.Recv);
            }

            public byte[] ReadNextRecvMessage()
            {
                byte[] message;
                bool isAnyMessageInQueue = recvMessagesCollection.TryTake(out message, 1000); // TODO parametrize timeout
                if (!isAnyMessageInQueue)
                    return null;

                return message;
            }

            public byte[] ReadNextListenMessage()
            {
                byte[] message;
                bool isAnyMessageInQueue = listenMessagesCollection.TryTake(out message, 200); // TODO parametrize timeout
                if (!isAnyMessageInQueue)
                    return null;

                return message;
            }

            public void RegisterIncommingMessageObserver(IncommingMessageObserver observer)
            {
                incommingMessageObserverList.Add(observer);
            }

            private void NotifyMessageObservers(byte[] message, MessageType type)
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

            private BlockingCollection<byte[]> listenMessagesCollection = new BlockingCollection<byte[]>();
            private BlockingCollection<byte[]> recvMessagesCollection = new BlockingCollection<byte[]>();

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
