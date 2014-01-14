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
            commandsReactions = new Dictionary<SRintAPI.Command, Action<SRintAPI.Command>>
            {
                { new SRintAPI.CreateVariableCommand(), CreateVariable },
                { new SRintAPI.DeleteVariableCommand(), DeleteVariable },
                { new SRintAPI.SetValueCommand(), SetValue }
            };
            this.sender = sender;
        }

        public void OnIncommingMessage(string message, Communication.MessageType type) // in server thread (not socket's, nor UI!)
        {
            Logger.Instance.LogNotice("Received message of type: " + type.ToString() + ". Message body: " + message);
            DispatchMessage(message, type);

            SRintAPI.Command command = null;
            bool isCommandInQueue = commandsQueue.TryTake(out command, 0);
            if (isCommandInQueue)
                commandsReactions[command](command);

            PropagateToNetwork();
        }

        private interface MessageHandler
        {
            void Execute(ServerController controller);
        }

        private class RecvMessageHandler : MessageHandler
        {
            public RecvMessageHandler(string message, Communication.MessageType type)
            {
                this.message = message;
                this.type = type;
            }

            public void Execute(ServerController controller)
            {
                protobuf.Message m = Serialization.MessageSerializer.Deserialize(Encoding.ASCII.GetBytes(message));
                controller.snapshot = new StateSnapshot(m);
                controller.OnSnapshotChange();
            }

            private string message;
            private Communication.MessageType type;
        }

        private class ListenMessageHandler : MessageHandler
        {
            public ListenMessageHandler(string message, Communication.MessageType type)
            {
                this.message = message;
                this.type = type;
            }
            public void Execute(ServerController controller)
            {
                // TODO implement here full logic of Listen packet appear
            }

            private string message;
            private Communication.MessageType type;
        }
        private void DispatchMessage(string message, Communication.MessageType type)
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
            sender.SendMessage(Encoding.ASCII.GetString(toSend));
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
            // TODO implement this
        }

        private SettingsForm.Settings settings;
        private BlockingCollection<SRintAPI.Command> commandsQueue;
        private readonly Dictionary<SRintAPI.Command, Action<SRintAPI.Command>> commandsReactions;
        private Communication.MessageSender sender;
        private StateSnapshot snapshot = null;
        private MessageHandler messageHandler;        
    }
}
