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
            Logger.Instance.OnErrorOccured += OnErrorOccured;
            Logger.Instance.OnWarningOccured += OnWarningOccured;
            Logger.Instance.OnNoticeOccured += OnNoticeOccured;
        }

        private void OnErrorOccured(object sender, LoggerEventArgs args)
        {
            this.Dispatcher.Invoke(() =>
              {
                    loggerTextBlock.Text += "\n";
                    loggerTextBlock.Text += "!!! ";
                    loggerTextBlock.Text += args.Message;
                    scrollViewer.ScrollToBottom();
              });
        }

        private void OnWarningOccured(object sender, LoggerEventArgs args)
        {
            this.Dispatcher.Invoke(() =>
              {
                    loggerTextBlock.Text += "\n";
                    loggerTextBlock.Text += "! ";
                    loggerTextBlock.Text += args.Message;
                    scrollViewer.ScrollToBottom();
             });
        }

        private void OnNoticeOccured(object sender, LoggerEventArgs args)
        {
            this.Dispatcher.Invoke(() =>
                {
                    loggerTextBlock.Text += "\n";
                    loggerTextBlock.Text += args.Message;
                    scrollViewer.ScrollToBottom();
                });
        }
    }
}
