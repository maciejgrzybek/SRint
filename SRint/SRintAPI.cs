using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace SRint
{
    public class SRintAPI : ISRint
    {
        public event EventHandler<VariableChangeDescription> OnVariableValueChanged;
        abstract public class Command
        {
            public string name { get; set; }
        }
        public class CreateVariableCommand : Command
        {}
        public class SetValueCommand : Command
        {
            public long value { get; set; }
        }

        public class DeleteVariableCommand : Command
        {}

        public SRintAPI()
        {
            lastSnapshot = new StateSnapshot();
        }

        public SRintAPI(BlockingCollection<Command> commandsQueue)
            : this()
        {
            this.commandsQueue = commandsQueue;
        }

        public void controller_OnSnapshotChanged(object sender, StateSnapshot e) // in back-end thread
        {
            e.variables.ForEach(variable =>
            {
                protobuf.Message.Variable foundVariable = getVariable(variable.name);
                if (foundVariable.value != variable.value)
                {
                    OnVariableValueChange(new VariableChangeDescription { OldValue = foundVariable.value, NewValue = variable.value, VariableName = variable.name });
                }
            });
            lastSnapshot = e;
        }

        public void dhCreate(string name)
        {
            commandsQueue.TryAdd(new CreateVariableCommand  { name = name }, 0);
            lastSnapshot.variables.Add(new protobuf.Message.Variable { name = name, value = 0 });
        }
        public void dhFree(string name)
        {
            commandsQueue.TryAdd(new DeleteVariableCommand { name = name }, 0);
        }
        public long dhGet(string name)
        {
            protobuf.Message.Variable variable = getVariable(name);
            if (variable == null)
                return -1;

            return variable.value;
        }
        public void dhSet(string name, long value)
        {
            commandsQueue.TryAdd(new SetValueCommand { name = name, value = value }, 0);
            var v = lastSnapshot.variables.Find((variable) => variable.name == name);
            v.value = value;
        }
        public void dhSetCallback(string name, OnConcreteVariableValueChanged callback)
        {
            callbacks[name].Add(callback);
        }
        public List<protobuf.Message.Variable> getVariables()
        {
            return lastSnapshot.variables;
        }

        private void OnVariableValueChange(VariableChangeDescription change)
        {
            if (OnVariableValueChanged != null)
            {
                OnVariableValueChanged(this, change);
                callbacks[change.VariableName].ForEach(callback => callback(change.VariableName, change.OldValue));
            }
        }

        private protobuf.Message.Variable getVariable(string name)
        {
            protobuf.Message.Variable foundVariable = lastSnapshot.variables.Find(v => v.name == name);
            if (foundVariable.name == null)
                return null;
            return foundVariable;
        }

        private BlockingCollection<Command> commandsQueue;
        private volatile StateSnapshot lastSnapshot;
        private Dictionary<string, List<OnConcreteVariableValueChanged>> callbacks = new Dictionary<string, List<OnConcreteVariableValueChanged>>();
    }
}
