using System;
using System.Configuration;
using System.Threading;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for ConnectWindow.xaml
    /// </summary>
    public partial class ConnectWindow : Window
    {
        public ConnectWindow()
        {
            InitializeComponent();
            portTextBox.Text = Properties.Settings.Default.Port;
            nicknameTextBox.Text = Properties.Settings.Default.nickname;
            ipTextBox.Text = Properties.Settings.Default.IpAddress;
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Port = portTextBox.Text;
            Properties.Settings.Default.nickname = nicknameTextBox.Text;
            Properties.Settings.Default.IpAddress = ipTextBox.Text;
            Properties.Settings.Default.Save();
            DialogResult = true;
        }

    }
}
