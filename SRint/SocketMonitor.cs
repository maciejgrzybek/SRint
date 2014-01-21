using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroMQ;

namespace SRint
{
    static class EventsMethodExtensions
    {
        public static void SafeInvoke(this MonitorEventHandler handler, ZmqSocket socket)
        {
            if (handler != null)
                handler(socket);
        }
    }

    public delegate void MonitorEventHandler(ZmqSocket socket);
    public class SocketMonitor
    {
        public event MonitorEventHandler OnConnected;
        public event MonitorEventHandler OnConnectDelayed;
        public event MonitorEventHandler OnConnectRetried;
        public event MonitorEventHandler OnListening;
        public event MonitorEventHandler OnBindFailed;
        public event MonitorEventHandler OnAccepted;
        public event MonitorEventHandler OnAcceptFailed;
        public event MonitorEventHandler OnClosed;
        public event MonitorEventHandler OnCloseFailed;
        public event MonitorEventHandler OnDisconnected;
        public SocketMonitor(ZmqContext context, ZmqSocket observedSocket)
        {
            this.context = context;
            this.observedSocket = observedSocket;
            ZeroMQ.Monitoring.MonitorSocketExtensions.Monitor(observedSocket, "inproc://monitor");
            socket = context.CreateSocket(SocketType.PAIR);

            events = new Dictionary<ZeroMQ.Monitoring.MonitorEvents, Action>()
            {
                { ZeroMQ.Monitoring.MonitorEvents.Connected, () => OnConnected.SafeInvoke(observedSocket) },
                { ZeroMQ.Monitoring.MonitorEvents.ConnectDelayed, () => OnConnectDelayed.SafeInvoke(observedSocket) },
                { ZeroMQ.Monitoring.MonitorEvents.ConnectRetried, () => OnConnectRetried.SafeInvoke(observedSocket) },
                { ZeroMQ.Monitoring.MonitorEvents.Listening, () => OnListening.SafeInvoke(observedSocket) },
                { ZeroMQ.Monitoring.MonitorEvents.BindFailed, () => OnBindFailed.SafeInvoke(observedSocket) },
                { ZeroMQ.Monitoring.MonitorEvents.Accepted, () => OnAccepted.SafeInvoke(observedSocket) },
                { ZeroMQ.Monitoring.MonitorEvents.AcceptFailed, () => OnAcceptFailed.SafeInvoke(observedSocket) },
                { ZeroMQ.Monitoring.MonitorEvents.Closed, () => OnClosed.SafeInvoke(observedSocket) },
                { ZeroMQ.Monitoring.MonitorEvents.CloseFailed, () => OnCloseFailed.SafeInvoke(observedSocket) },
                { ZeroMQ.Monitoring.MonitorEvents.Disconnected, () => OnDisconnected.SafeInvoke(observedSocket) },
            };
        }

        public void Start()
        {
            poll = true;
            var thread = new System.Threading.Thread(Poll);
            thread.Start();
        }

        public void Stop()
        {
            poll = false;
        }

        private void Poll()
        {
            socket.Connect("inproc://monitor");
            while (poll)
            {
                byte[] recv = new byte[1024];
                int size;
                socket.Receive(recv, out size);
                if (size > 6)
                    continue; // skip event details (like IP address etc.)
                short rcv = BitConverter.ToInt16(recv, 0);
                var result = (ZeroMQ.Monitoring.MonitorEvents)rcv;
                events[result]();
            }
        }

        private ZmqContext context;
        private ZmqSocket socket;
        private ZmqSocket observedSocket;

        private volatile bool poll = false;

        private readonly Dictionary<ZeroMQ.Monitoring.MonitorEvents, Action> events;
    }
}
