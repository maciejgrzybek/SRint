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
                { typeof(SRintAPI.SetValueCommand), SetValue },
                { typeof(SRintAPI.AppendNodeToNetworkCommand), AppendNodeToNetwork }
            };
            this.sender = sender;
            if (settings.isNetworkFounder)
            {
                snapshot = new StateSnapshot();
                snapshot.message.state_content.nodes.Add(new protobuf.Message.NodeDescription { node_id = 1, ip = settings.address, port = settings.port }); // node_id = 1 => network founder
            }
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
            bool isActionPerformedAlready = DispatchMessage(message); // TODO real dispatching - entry vs normal vs election

            if (isActionPerformedAlready)
                return; // no further action in this turn

            SRintAPI.Command command = null;
            bool isCommandInQueue = commandsQueue.TryTake(out command, 0);
            if (isCommandInQueue)
                commandsReactions[command.GetType()](command);

            EnsureConnectionToAppropriateNextNode();
            PropagateToNetwork();
        }

        private bool DispatchMessage(byte[] message)
        {
            protobuf.Message m = Serialization.MessageSerializer.Deserialize(message);
            if (m.type == protobuf.Message.MessageType.ENTRY_REQUEST)
            {
                System.Diagnostics.Debug.Assert(m.state_content.nodes.Count == 1);

                protobuf.Message.NodeDescription node = m.state_content.nodes[0];
                commandsQueue.Add(new SRintAPI.AppendNodeToNetworkCommand { nodeAddress = node.ip, nodePort = node.port, isFirstAppendedNode = settings.isNetworkFounder }); // entry request will be handler AFTER all other requests enqueued!
                return !IsTokenCreationNeeded();
            }
            snapshot = new StateSnapshot(m);
            OnSnapshotChange();
            return false;
        }

        private bool IsTokenCreationNeeded()
        {
            return (snapshot.message.state_content.nodes.Count == 1 && settings.isNetworkFounder);
        }

        private void EnsureConnectionToAppropriateNextNode()
        {
            var info = sender.ConnectedToNodeInfo;
            int index = GetMyIndexInNodeDescriptionList();
            if (index+1 == snapshot.message.state_content.nodes.Count) // is last element
            {
                protobuf.Message.NodeDescription firstNode = snapshot.message.state_content.nodes[0];
                if (!info.HasValue || (((Communication.ConnectionInfo)(info)).address != firstNode.ip || ((Communication.ConnectionInfo)(info)).port != firstNode.port))
                {
                    sender.Disconnect();
                    sender.Connect(firstNode.ip, firstNode.port);
                }
            }
            else
            {
                protobuf.Message.NodeDescription nextNode = snapshot.message.state_content.nodes[index + 1];
                if (!info.HasValue || (((Communication.ConnectionInfo)(info)).address != nextNode.ip || ((Communication.ConnectionInfo)(info)).port != nextNode.port))
                {
                    sender.Disconnect();
                    sender.Connect(nextNode.ip, nextNode.port);
                }
            }
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
            SRintAPI.VariableManipulatingCommand cmd = command as SRintAPI.VariableManipulatingCommand;
            protobuf.Message.Variable variable = new protobuf.Message.Variable { name = cmd.name, value = 0 };
            snapshot.variables.Add(variable);
            SetMeAsOwner(variable);
        }

        private void DeleteVariable(SRintAPI.Command command)
        {
            SRintAPI.VariableManipulatingCommand cmd = command as SRintAPI.VariableManipulatingCommand;
            var v = snapshot.variables.Find((variable) => variable.name == cmd.name);
            v.owners.RemoveAll((owner) => { return (owner.ip == settings.address && owner.port == settings.port); }); // TODO take under consideration node_id
            if (v.owners.Count == 0)
                snapshot.variables.Remove(v);
        }

        private void SetValue(SRintAPI.Command command)
        {
            SRintAPI.VariableManipulatingCommand cmd = command as SRintAPI.VariableManipulatingCommand;
            var v = snapshot.variables.Find((variable) => variable.name == cmd.name);
            var c = command as SRintAPI.SetValueCommand;
            v.value = c.value;
            SetMeAsOwner(v);
        }

        private void AppendNodeToNetwork(SRintAPI.Command command)
        {
            SRintAPI.AppendNodeToNetworkCommand cmd = command as SRintAPI.AppendNodeToNetworkCommand;
            // TODO check whether node already exists
            var nodes = snapshot.message.state_content.nodes;
            int myIndex = GetMyIndexInNodeDescriptionList();
            nodes.Insert(myIndex + 1, new protobuf.Message.NodeDescription { node_id = GetHighestNodeID() + 1, ip = cmd.nodeAddress, port = cmd.nodePort }); // FIXME set appropriate node_id!
        }

        private int GetHighestNodeID()
        {
            int highestNodeID = 1;
            var nodes = snapshot.message.state_content.nodes;
            nodes.ForEach((node) =>
            {
                if (node.node_id > highestNodeID)
                    highestNodeID = node.node_id;
            });
            return highestNodeID;
        }

        private int GetMyIndexInNodeDescriptionList()
        {
            return snapshot.message.state_content.nodes.FindIndex((node) =>
            {
                return (node.ip == settings.address && node.port == settings.port);
            });
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
