using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient client;

        public MainWindow()
        {
            InitializeComponent();
            client = new TcpClient(this);
        }

        private void connectButtonClicked(object sender, RoutedEventArgs e)
        {
            client.Start();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            ResponseTextMessage msg = new ResponseTextMessage();
            msg.Text = msgTextBox.Text;
            client.SendMessageAsync(msg);
        }

        private void disconnectButton_Click(object sender, RoutedEventArgs e)
        {
            client.Close();
        }

        /*
        * Appends text to main window and scrolls down the screen automatically.
        */
        public void PostMessage(string msg)
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Dispatcher.Invoke((Action) (() => chatTextBox.Text += msg + "\n"));
                Dispatcher.Invoke((Action) (() => chatTextBox.Focus()));
                Dispatcher.Invoke((Action)(() => chatTextBox.CaretIndex = chatTextBox.Text.Length));
                Dispatcher.Invoke((Action)(() => chatTextBox.ScrollToEnd()));
            });
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }
    }
}
