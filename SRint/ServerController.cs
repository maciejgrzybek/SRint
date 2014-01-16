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
        public class InvalidStateException : SystemException
        {}
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

        public void EnterNetwork()
        {
            if (settings.nodeAddress == null || settings.nodePort == null)
                throw new InvalidStateException();

            sender.Connect(settings.nodeAddress, (int)settings.nodePort);

            SendEntryRequest();
        }

        private void SendEntryRequest()
        {
            protobuf.Message.State state = new protobuf.Message.State { state_id = -1 };
            state.nodes.Add(new protobuf.Message.NodeDescription { node_id = -1, ip = settings.address, port = settings.port });
            protobuf.Message message = new protobuf.Message { state_content = state, type = protobuf.Message.MessageType.ENTRY_REQUEST };

            byte[] serializedMessage = Serialization.MessageSerializer.Serialize(message);
            sender.SendMessage(serializedMessage);
        }

        public void OnIncommingMessage(byte[] message) // in server thread (not socket's, nor UI!)
        {
            Logger.Instance.LogNotice("Received message: " + message);
            bool isActionPerformedAlready = DispatchMessage(message); // TODO real dispatching - entry vs normal vs election

            if (isActionPerformedAlready)
                return; // no further action in this turn

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

        private bool DispatchMessage(byte[] message)
        {
            protobuf.Message m = Serialization.MessageSerializer.Deserialize(message);
            if (m.type == protobuf.Message.MessageType.ENTRY_REQUEST)
            {
                // TODO handle entry request
                return true;
            }
            snapshot = new StateSnapshot(m);
            OnSnapshotChange();
            return false;
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
            var v = snapshot.variables.Find((variable) => variable.name == command.name);
            v.owners.RemoveAll((owner) => { return (owner.ip == settings.address && owner.port == settings.port); }); // TODO take under consideration node_id
            if (v.owners.Count == 0)
                snapshot.variables.Remove(v);
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
