using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SRint
{
    /// <summary>
    /// Interaction logic for VariablesView.xaml
    /// </summary>
    public partial class VariablesView : Page,
                                         MessagesPrinter
    {
        public VariablesView(SRintAPI api)
        {
            InitializeComponent();
            System.Windows.Input.FocusManager.SetFocusedElement(this, inputText);
            commands = new Dictionary<string, Command>() {
                { "help", new ShowHelpCommand() },
                { "create", new CreateVariableCommand(api) },
                { "free", new DeleteVariableCommand(api) },
                { "list", new ShowVariablesCommand(api) },
                { "set", new SetVariableCommand(api) },
                { "get", new GetVariableCommand(api) }
             };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string command = inputText.Text;
            inputText.Clear();
            consoleTextBlock.Text += "\n> " + command;
            try
            {
                ExecuteCommand(command);
            }
            catch (CommandNotFoundException ex)
            {
                PrintMessage("Unknown command: " + command);
            }
        }

        private void inputText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                Button_Click(null, new RoutedEventArgs());
        }

        private void ExecuteCommand(string command)
        {
            int firstSpacePosition = command.IndexOf(" ");
            string firstWord = command;
            string[] arguments = null;
            if (firstSpacePosition > 0)
            {
                firstWord = command.Substring(0, firstSpacePosition);
                arguments = command.Substring(firstSpacePosition+1).Split(' ');
            }
            try
            {
                string commandMessage = commands[firstWord].Execute(arguments);
                PrintMessage(commandMessage);
            }
            catch (KeyNotFoundException ex)
            {
                throw new CommandNotFoundException();
            }
        }

        public void PrintMessage(string message)
        {
            this.Dispatcher.Invoke(() =>
                {
                    consoleTextBlock.Text += "\n";
                    consoleTextBlock.Text += message;
                    scrollViewer.ScrollToBottom();
                });
        }

        private Dictionary<string, Command> commands;
    }
}
