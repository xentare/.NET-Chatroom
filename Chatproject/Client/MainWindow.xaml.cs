using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
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
            msgTextBox.MaxLength = 256;
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBase msg = new MessageBase();
            msg.Message = msgTextBox.Text;
            msg.Type = (int) MessageBase.Types.Message;
            msg.Nickname = Properties.Settings.Default.nickname;
            client.SendMessageAsync(msg);
        }

        private void disconnectButton_Click(object sender, RoutedEventArgs e)
        {
            client.Close();
            usersTextBox.Text = "";
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

        public void PostUsers(string users)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                string[] usersString = users.Split(',');
                string usersList = null;
                foreach (string s in usersString)
                {
                    usersList += s + "\n";
                }
                usersTextBox.Text = usersList;
            }));
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
