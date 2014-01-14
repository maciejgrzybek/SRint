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
using System.Windows.Shapes;
using System.Collections.Concurrent;

namespace SRint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            commandsQueue = new BlockingCollection<SRintAPI.Command>();
            api = new SRintAPI(commandsQueue);
            pages = new Dictionary<string,Page>()
            {
                { "ShowLogPage_Button", new LogPage(this) },
                { "ShowVariablesView_Button", new VariablesView(api) }
            };
            frame.NavigationService.Navigate(pages["ShowLogPage_Button"]);
        }

        private void CreateServerButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsForm form = new SettingsForm();
            form.ShowDialog();

            Logger.Instance.LogNotice("Spawning server. Address = " + form.settings.address + ":" + form.settings.port);

            CreateServer_Button.IsEnabled = false;

            server = new Communication.Server(form.settings.address, form.settings.port);
            poller = new Communication.ServerPoller(server);
            poller.PollingStarted += OnPollingStart;
            poller.PollingFinished += OnPollingFinish;

            StartServer_Button.IsEnabled = true;
            StopServer_Button.IsEnabled = false;

            ServerController ctrl = new ServerController(form.settings, commandsQueue, server);
            server.RegisterIncommingMessageObserver(ctrl);

            ctrl.OnSnapshotChanged += api.controller_OnSnapshotChanged;
        }

        private void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Instance.LogNotice("Starting server thread");
            poller.Start();
            StartServer_Button.IsEnabled = false;
            StopServer_Button.IsEnabled = true;
        }

        private void StopServerButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Instance.LogNotice("Stopping server");
            StartServer_Button.IsEnabled = false;
            StopServer_Button.IsEnabled = false;
            poller.RequestStop();
        }

        private void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            frame.NavigationService.Navigate(pages[button.Name]);
        }

        void OnPollingStart(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                StartServer_Button.IsEnabled = false;
                StopServer_Button.IsEnabled = true;
                Logger.Instance.LogNotice("Polling started");
            });
        }

        void OnPollingFinish(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                StartServer_Button.IsEnabled = true;
                StopServer_Button.IsEnabled = false;
                Logger.Instance.LogNotice("Polling stopped");
            });
        }

        private SRintAPI api;
        private Communication.Server server;
        private Communication.ServerPoller poller;
        private BlockingCollection<SRintAPI.Command> commandsQueue;
        private readonly Dictionary<string, Page> pages;
    }
}
