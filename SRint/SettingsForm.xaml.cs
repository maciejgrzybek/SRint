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

namespace SRint
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsForm : Window
    {
        public event EventHandler<Settings> OnSettingsChanged;
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AcceptForm();
        }

        public Settings settings;

        public struct Settings
        {
            public string address { get; set; }
            public int port { get; set; }
            public string nodeAddress { get; set; }
            public int? nodePort { get; set; }
        }

        private void AcceptForm()
        {
            settings = new Settings { address = ipAddress.Text, port = Convert.ToInt32(port.Text) };
            if (isEnteringNetworkCheckbox.IsChecked == true)
            {
                settings.nodeAddress = nodeAddress.Text;
                settings.nodePort = Convert.ToInt32(nodePort.Text);
            }
            if (OnSettingsChanged != null)
            {
                OnSettingsChanged(this, settings);
            }
            DialogResult = true;
            Close();
        }

        private void DiscardForm()
        {
            DialogResult = false;
            Close();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            nodeAddress.IsEnabled = true;
            nodePort.IsEnabled = true;
        }

        private void isEnteringNetworkCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            nodeAddress.IsEnabled = false;
            nodePort.IsEnabled = false;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DiscardForm();
        }
    }
}
