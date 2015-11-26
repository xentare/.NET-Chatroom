using System;
using System.Configuration;
using System.Threading;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private ConnectWindow connectWindow;

        public MainWindow()
        {
            //this.connectWindow = connectWindow;
            InitializeComponent();
            client = new TcpClient(this);
            msgTextBox.MaxLines = 1;
            msgTextBox.MaxLength = 512;
        }

        private void connectButtonClicked(object sender, RoutedEventArgs e)
        {

        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            client.SendMessageAsync(msgTextBox.Text);
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

        public void PostLog(string msg)
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Dispatcher.Invoke((Action) (() => logTextBox.Text += msg + "\n"));
                Dispatcher.Invoke((Action)(() => chatTextBox.Focus()));
                Dispatcher.Invoke((Action)(() => chatTextBox.CaretIndex = chatTextBox.Text.Length));
                Dispatcher.Invoke((Action)(() => chatTextBox.ScrollToEnd()));
            });
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            connectWindow = new ConnectWindow();
            connectWindow.ShowDialog();
            connectWindow.Owner = this;
            if (connectWindow.DialogResult != null && connectWindow.DialogResult.Value)
            {
                client.Start(Properties.Settings.Default.nickname);
            }
        }
    }
}
