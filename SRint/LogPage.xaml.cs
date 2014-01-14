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
    /// Interaction logic for LogPage.xaml
    /// </summary>
    public partial class LogPage : Page
    {
        public LogPage(MainWindow parent)
        {
            InitializeComponent();
            this.parent = parent;
            Logger.Instance.OnErrorOccured += OnErrorOccured;
            Logger.Instance.OnWarningOccured += OnWarningOccured;
            Logger.Instance.OnNoticeOccured += OnNoticeOccured;
        }

        private void OnErrorOccured(object sender, LoggerEventArgs args)
        {
            loggerTextBlock.Text += "\n";
            loggerTextBlock.Text += "!!! ";
            loggerTextBlock.Text += args.Message;
        }

        private void OnWarningOccured(object sender, LoggerEventArgs args)
        {
            loggerTextBlock.Text += "\n";
            loggerTextBlock.Text += "! ";
            loggerTextBlock.Text += args.Message;
        }

        private void OnNoticeOccured(object sender, LoggerEventArgs args)
        {
            this.Dispatcher.Invoke(() =>
                {
                    loggerTextBlock.Text += "\n";
                    loggerTextBlock.Text += args.Message;
                });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DateTime dt = System.DateTime.Now;
            protobuf.Message.State state = new protobuf.Message.State { state_id = 12 };
            protobuf.Message.Variable variable = new protobuf.Message.Variable { name = "Test", value = 17 };
            state.variables.Add(variable);
            protobuf.Message msg = new protobuf.Message
            {
                type = protobuf.Message.MessageType.STATE,
                state_content = state
            };
            string toSend = Encoding.ASCII.GetString(Serialization.MessageSerializer.Serialize(msg));
            Logger.Instance.LogNotice("Sending: \"" + toSend + "\" of length: " + toSend.Length);
            //parent.server.SendMessage(toSend);

            //loggerTextBlock.Text += "\ncontext spawning";
            //var context = new ZMQ.Context(1);
            //loggerTextBlock.Text += "\nusing requester";
            //using (ZMQ.Socket requester = context.Socket(SocketType.REQ))
            //{
            //    loggerTextBlock.Text += "\nconnecting to host";
            //    requester.Connect("tcp://10.25.67.29:5555");
            //    //requester.Connect("tcp://127.0.0.1:5555");

            //    const string requestMessage = "Hello";
            //    loggerTextBlock.Text += "Sending request...";
            //    requester.Send(requestMessage, Encoding.ASCII);
            //    string reply = requester.Recv(Encoding.ASCII, 2000);
            //    loggerTextBlock.Text += "Received reply: {0}" + reply;
            //}
        }

        MainWindow parent;
    }
}
