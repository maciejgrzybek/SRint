using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRint
{
    interface Command
    {
        string Execute(string[] arguments);
    }

    public class CommandNotFoundException : System.Exception
    {}

    public class InvalidCommandException : System.Exception
    {}

    interface MessagesPrinter
    {
        void PrintMessage(string msg);
    }

    class ShowHelpCommand : Command
    {
        public string Execute(string[] arguments)
        {
            string msg = null;
            if (arguments == null)
            {
                msg = "help - prints this message"
                    + "\nlist"
                    + "\ncreate name"
                    + "\nfree name"
                    + "\nset name value"
                    + "\nget name";
            }
            return msg;
        }
    }

    class CreateVariableCommand : Command
    {
        public CreateVariableCommand(ISRint api)
        {
            this.api = api;
        }

        public string Execute(string[] arguments)
        {
            if (arguments.Length < 1)
                throw new InvalidCommandException();

            api.dhCreate(arguments[0]);
            return "Variable '" + arguments[0] + "' will be created when next token arrives.";
        }

        private ISRint api;
    }

    class DeleteVariableCommand : Command
    {
        public DeleteVariableCommand(ISRint api)
        {
            this.api = api;
        }

        public string Execute(string[] arguments)
        {
            if (arguments.Length < 1)
                throw new InvalidCommandException();

            api.dhFree(arguments[0]);
            return "Variable '" + arguments[0] + "' marked for removal.";
        }

        private ISRint api;
    }

    class SetVariableCommand : Command
    {
        public SetVariableCommand(ISRint api)
        {
            this.api = api;
        }

        public string Execute(string[] arguments)
        {
            if (arguments == null || arguments.Length < 2)
                throw new InvalidCommandException();

            string name = arguments[0];
            long value = Convert.ToInt64(arguments[1]);

            api.dhSet(name, value);

            return name + " = " + value;
        }

        private ISRint api;
    }

    class GetVariableCommand : Command
    {
        public GetVariableCommand(ISRint api)
        {
            this.api = api;
        }

        public string Execute(string[] arguments)
        {
            if (arguments == null || arguments.Length < 1)
                throw new InvalidCommandException();

            string name = arguments[0];

            long? value = api.dhGet(name);

            if (value == null)
                return "Variable '" + name + "' does not exist.";

            return name + " = " + value;
        }

        private ISRint api;
    }

    class ShowVariablesCommand : Command
    {
        public ShowVariablesCommand(ISRint api)
        {
            this.api = api;
        }

        public string Execute(string[] arguments)
        {
            if (arguments != null && arguments.Length > 0)
                throw new InvalidCommandException();

            var variables = api.getVariables();
            string result = "Variables:\n";
            variables.ForEach((variable) => { result += variable.name + " = " + variable.value + "\n"; });
            return result;
        }

        private ISRint api;
    }
}
