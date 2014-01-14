using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace SRint
{
    class ServerController : Communication.IncommingMessageObserver
    {
        public event EventHandler<StateSnapshot> OnSnapshotChanged;
        public ServerController(SettingsForm.Settings settings, BlockingCollection<SRintAPI.Command> commandsQueue, Communication.MessageSender sender)
        {
            this.settings = settings;
            this.commandsQueue = commandsQueue;
            commandsReactions = new Dictionary<Type, Action<SRintAPI.Command>>
            {
                { typeof(SRintAPI.CreateVariableCommand), CreateVariable },
                { typeof(SRintAPI.DeleteVariableCommand), DeleteVariable },
                { typeof(SRintAPI.SetValueCommand), SetValue }
            };
            this.sender = sender;
        }

        public void OnIncommingMessage(byte[] message, Communication.MessageType type) // in server thread (not socket's, nor UI!)
        {
            Logger.Instance.LogNotice("Received message of type: " + type.ToString() + ". Message body: " + message);
            DispatchMessage(message, type);

            SRintAPI.Command command = null;
            bool isCommandInQueue = commandsQueue.TryTake(out command, 0);
            if (isCommandInQueue)
                commandsReactions[command.GetType()](command);

            PropagateToNetwork();
        }

        private interface MessageHandler
        {
            void Execute(ServerController controller);
        }

        private class RecvMessageHandler : MessageHandler
        {
            public RecvMessageHandler(byte[] message, Communication.MessageType type)
            {
                this.message = message;
                this.type = type;
            }

            public void Execute(ServerController controller)
            {
                protobuf.Message m = Serialization.MessageSerializer.Deserialize(message);
                controller.snapshot = new StateSnapshot(m);
                controller.OnSnapshotChange();
            }

            private byte[] message;
            private Communication.MessageType type;
        }

        private class ListenMessageHandler : MessageHandler
        {
            public ListenMessageHandler(byte[] message, Communication.MessageType type)
            {
                this.message = message;
                this.type = type;
            }
            public void Execute(ServerController controller)
            {
                // TODO implement here full logic of Listen packet appear
            }

            private byte[] message;
            private Communication.MessageType type;
        }
        private void DispatchMessage(byte[] message, Communication.MessageType type)
        {
            switch (type)
            {
                case Communication.MessageType.Listen:
                    messageHandler = new ListenMessageHandler(message, type);
                    break;
                case Communication.MessageType.Recv:
                    messageHandler = new RecvMessageHandler(message, type);
                    break;
            }
            messageHandler.Execute(this);
        }

        private void PropagateToNetwork()
        {
            var toSend = Serialization.MessageSerializer.Serialize(snapshot.message);
            sender.SendMessage(toSend);
        }

        private void OnSnapshotChange()
        {
            if (OnSnapshotChanged != null)
            {
                OnSnapshotChanged(this, snapshot);
            }
        }

        private void CreateVariable(SRintAPI.Command command)
        {
            protobuf.Message.Variable variable = new protobuf.Message.Variable { name = command.name, value = 0 };
            variable.owners.Add(new protobuf.Message.NodeDescription {ip = settings.address, port = settings.port });
            snapshot.variables.Add(variable);
        }

        private void DeleteVariable(SRintAPI.Command command)
        {
            // TODO implement this
        }
        private void SetValue(SRintAPI.Command command)
        {
            var v = snapshot.variables.Find((variable) => variable.name == command.name);
            var c = command as SRintAPI.SetValueCommand;
            v.value = c.value;
        }

        private SettingsForm.Settings settings;
        private BlockingCollection<SRintAPI.Command> commandsQueue;
        private readonly Dictionary<Type, Action<SRintAPI.Command>> commandsReactions;
        private Communication.MessageSender sender;
        private StateSnapshot snapshot = null;
        private MessageHandler messageHandler;        
    }
}
