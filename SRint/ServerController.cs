using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace SRint
{
    class ServerController
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

        public void OnIncommingMessage(byte[] message) // in server thread (not socket's, nor UI!)
        {
            Logger.Instance.LogNotice("Received message: " + message);
            DispatchMessage(message);

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

        private void DispatchMessage(byte[] message)
        {
            protobuf.Message m = Serialization.MessageSerializer.Deserialize(message);
            snapshot = new StateSnapshot(m);
            OnSnapshotChange();
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
            snapshot.variables.Add(variable);
            SetMeAsOwner(variable);
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
            SetMeAsOwner(v);
        }

        private void SetMeAsOwner(protobuf.Message.Variable variable)
        {
            // TODO take under consideration node_id!
            bool existInOwners = variable.owners.Exists((node) => { return (node.ip == settings.address && node.port == settings.port); });
            if (!existInOwners)
                variable.owners.Add(new protobuf.Message.NodeDescription { ip = settings.address, port = settings.port });
        }

        private SettingsForm.Settings settings;
        private BlockingCollection<SRintAPI.Command> commandsQueue;
        private readonly Dictionary<Type, Action<SRintAPI.Command>> commandsReactions;
        private Communication.MessageSender sender;
        private StateSnapshot snapshot = null;
    }
}
