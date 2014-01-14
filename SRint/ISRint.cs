using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRint
{
    public delegate void OnConcreteVariableValueChanged(string name, long oldValue);
    interface ISRint
    {
        event EventHandler<VariableChangeDescription> OnVariableValueChanged;

        void dhCreate(string name);
        void dhFree(string name);
        long dhGet(string name);
        void dhSet(string name, long value);
        void dhSetCallback(string name, OnConcreteVariableValueChanged callback);
        List<protobuf.Message.Variable> getVariables();
    }

    public class VariableChangeDescription
    {
        public long OldValue { get; set;  }
        public long NewValue { get; set; }
        public string VariableName { get; set; }
    }
}
