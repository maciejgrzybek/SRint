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
            settings = new Settings { address = ipAddress.Text, port = Convert.ToInt32(port.Text) };
            if (OnSettingsChanged != null)
            {
                OnSettingsChanged(this, settings);
            }
            this.Close();
        }

        public Settings settings;

        public struct Settings
        {
            public string address;
            public int port;
        }
    }
}
