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

            public event IncommingMessageHandler OnIncommingMessage;

            public Server(string address = "tcp://*", int recvPort = 5555, int poolSize = 1)
            {
                context = new ZMQ.Context(poolSize);

                recvSocket = context.Socket(ZMQ.SocketType.PULL);
                BindToRecvSocket(address, recvPort);
                receiver = new ReceivingThread(recvMessagesCollection, recvSocket);
                receivingThread = new Thread(receiver.StartReading);

                sendSocket = CreateSendingSocket();
            }

            public void Connect(string address, int port)
            {
                string addr = "tcp://" + address + ":" + port.ToString();
                sendSocket.Connect(addr);
            }

            public void Disconnect()
            {
                sendSocket.Dispose();
                sendSocket = CreateSendingSocket();
            }

            public void SendMessage(byte[] message)
            {
                sendSocket.Send(message);
            }

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
                byte[] recvMessage = ReadNextRecvMessage();

                if (recvMessage != null)
                    NotifyMessageObservers(recvMessage);
            }

            public byte[] ReadNextRecvMessage()
            {
                byte[] message;
                bool isAnyMessageInQueue = recvMessagesCollection.TryTake(out message, 1000); // TODO parametrize timeout
                if (!isAnyMessageInQueue)
                    return null;

                return message;
            }

            private void NotifyMessageObservers(byte[] message)
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

            private ZMQ.Socket CreateSendingSocket()
            {
                return context.Socket(ZMQ.SocketType.PUSH);
            }

            private ZMQ.Context context;
            private ZMQ.Socket recvSocket;
            private ZMQ.Socket sendSocket;

            private BlockingCollection<byte[]> recvMessagesCollection = new BlockingCollection<byte[]>();

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
