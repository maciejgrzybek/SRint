using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using ZeroMQ;

namespace SRint
{
    namespace Communication
    {
        public class Server : IncommingMessagesReceiver,
                              MessageSender
        {
            class ReceivingThread
            {
                public ReceivingThread(BlockingCollection<Message> messagesCollection, ZmqSocket socket)
                {
                    this.socket = socket;
                    this.messagesCollection = messagesCollection;
                }

                public void StartReading()
                {
                    workIndicator = true;

                    while (workIndicator)
                    {
                        byte[] received = new byte[1024]; // TODO change this fixed size buffer
                        int result = socket.Receive(received, TimeSpan.FromMilliseconds(2000)); // TODO parametrize timeout
                        if (result > 0 && received != null && received.Length > 0)
                        {
                            Array.Resize(ref received, result);
                            messagesCollection.Add(new NetworkMessage { payload = received });
                        }
                    }
                }

                public void StopReading()
                {
                    workIndicator = false;
                }

                private ZmqSocket socket;
                private BlockingCollection<Message> messagesCollection;
                private volatile bool workIndicator = false;
            }

            public event IncommingMessageHandler OnIncommingMessage;

            public Server(string address = "tcp://*", int recvPort = 5555, int poolSize = 1)
            {
                context = ZmqContext.Create();

                recvSocket = context.CreateSocket(SocketType.PULL);
                BindToRecvSocket(address, recvPort);
                receiver = new ReceivingThread(recvMessagesCollection, recvSocket);
                receivingThread = new Thread(receiver.StartReading);

                sendSocket = CreateSendingSocket();
                SocketMonitor monitor = new SocketMonitor(context, sendSocket);
                monitor.OnConnected += (ZmqSocket socket) => { Logger.Instance.LogNotice("Connected."); };
                monitor.OnDisconnected += (ZmqSocket socket) => { Logger.Instance.LogNotice("Disconnected."); };
                monitor.Start();
            }

            public void Connect(string address, int port)
            {
                string addr = "tcp://" + address + ":" + port.ToString();
                sendSocket.Connect(addr);
                connectedToNodeInfo = new ConnectionInfo { address = address, port = port };
            }

            public void Disconnect()
            {
                if (connectedToNodeInfo.HasValue)
                {
                    string address = "tcp://" + connectedToNodeInfo.Value.address + ":" + connectedToNodeInfo.Value.port;
                    sendSocket.Disconnect(address);
                }
                sendSocket.Dispose();
                connectedToNodeInfo = null;
                sendSocket = CreateSendingSocket();
            }

            public void SendMessage(byte[] message)
            {
                sendSocket.Send(message, TimeSpan.FromMilliseconds(3000)); // TODO parametrize timeout
            }

            public ConnectionInfo? ConnectedToNodeInfo { get { return connectedToNodeInfo; } }

            public void StartSocketPolling()
            {
                receivingThread.Start();
            }

            public void StopSocketPolling()
            {
                receiver.StopReading();
                receivingThread.Join();
                receivingThread = new Thread(receiver.StartReading);
            }

            public void OperateOnce()
            {            
                Message recvMessage = ReadNextRecvMessage();

                if (recvMessage != null)
                    NotifyMessageObservers(recvMessage);
            }

            public Message ReadNextRecvMessage()
            {
                Message message;
                bool isAnyMessageInQueue = recvMessagesCollection.TryTake(out message, 1000); // TODO parametrize timeout
                if (!isAnyMessageInQueue)
                    return null;

                return message;
            }

            private void NotifyMessageObservers(Message message)
            {
                if (OnIncommingMessage != null)
                {
                    OnIncommingMessage(message);
                }
            }

            private void BindToRecvSocket(string addr, int port)
            {
                string address = "tcp://" +addr + ":" + port.ToString();
                recvSocket.Bind(address);
            }

            private ZeroMQ.ZmqSocket CreateSendingSocket()
            {
                return context.CreateSocket(ZeroMQ.SocketType.PUSH);
            }

            private ZeroMQ.ZmqContext context;
            private ZeroMQ.ZmqSocket recvSocket;
            private ZeroMQ.ZmqSocket sendSocket;
            private ZeroMQ.Monitoring.ZmqMonitor sendingMonitor;

            private readonly BlockingCollection<Message> recvMessagesCollection = new BlockingCollection<Message>();

            private ReceivingThread receiver;
            private Thread receivingThread;
            private ConnectionInfo? connectedToNodeInfo;
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
