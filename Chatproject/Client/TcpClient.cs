using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Client
{
    internal class TcpClient
    {
        private const int BufferSize = 1024;
        private const int Timeout = 2;
        private int TimeoutCounter;
        private NetworkStream Stream;
        private List<MessageBase> MessageQueue = new List<MessageBase>();
        private Thread receivingThread;
        private System.Net.Sockets.TcpClient Client;
        private MainWindow Window;
        private TextBoxOutputter outputter;

        public TcpClient(MainWindow window)
        {
            this.Window = window;
            outputter = new TextBoxOutputter(window.logTextBox);
            Console.SetOut(outputter);
        }
        public void Close()
        {
            try
            {
                //SendMessageAsync(msg);
                Client.Close();
                receivingThread.Abort();
                Client = null;
                Console.WriteLine("Disconnected");
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException {0}", e);
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("Not connected");
                //Console.WriteLine("NullReferenceException {0}", e);
            }
        }
        /*
        * Connects to server, logins and starts receiving thread.
        */
        public void Start(string nickname)
        {
            if (Client != null) return;
            try
            {
                Client = new System.Net.Sockets.TcpClient();
                Client.Connect("localhost", 5555);
                Stream = Client.GetStream();
                Login(Properties.Settings.Default.nickname);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketExecption {0}", e);
            }
            if (!Client.Connected) return;
            Console.WriteLine("Connected.");
            receivingThread = new Thread(ReadSerializable);
            receivingThread.Start();
        }
        public Task SendMessageAsync(MessageBase msg)
        {
            try
            {
                XmlSerializer x = new XmlSerializer(typeof (MessageBase));
                x.Serialize(Stream, msg);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("{0}", e);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("ArgumentException {0}",e);
            }

            return null;
        }
        public Task SendMessageAsync(string msg)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            try
            {
                var writeTask = Stream.WriteAsync(buffer, 0, buffer.Length);
                writeTask.ContinueWith(
                    task =>
                    {
                        if (task.Status == TaskStatus.RanToCompletion)
                        {
                            Console.WriteLine("Bytes sent: {0}", buffer.Length);
                        }
                    });
                return writeTask;
            }
            catch (IOException e)
            {
                //Console.WriteLine("IOException {0}", e);
            }
            catch (ObjectDisposedException e)
            {
                //Console.WriteLine("ObjectDisposedException {0}", e);
                Console.WriteLine("Not connected");
            }
            catch (NullReferenceException e)
            {
                //Console.WriteLine("NullReferenceException {0}", e);
                Console.WriteLine("Not connected");
            }
            return null;
        }
        /*
        * Runs on a separate thread. Passes readed bytes to asyncronous OnSerializableReceived and keeps reading.
        */
        private void ReadSerializable()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    var x = Stream.Read(buffer, 0, BufferSize);

                    if (x == 0)
                    {
                        TimeoutCounter = TimeoutCounter + 1;
                        if (TimeoutCounter > Timeout)
                        {
                            Client.Close();
                            Console.WriteLine("Disconnected");
                            break;
                        }
                        Thread.Sleep(2000);
                        Console.WriteLine("0 bytes received, sleeping thread...");
                        Console.WriteLine("Host not connected...");
                        return;
                    }

                    OnSerializableReceived(ref buffer);
                }
                catch (SocketException e)
                {
                    Console.WriteLine("Exception {0}", e);
                    Close();
                }
                catch (IOException e)
                {
                    Console.WriteLine("IOException {0}", e);
                    Close();
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("SerializationException {0}", e);
                }
                catch (ObjectDisposedException e)
                {
                    Console.WriteLine("ObjectDisposedException {0}", e);
                    Close();
                }
            }
        }
        /*
        * Handles received messages.
        */
        public Task OnSerializableReceived(ref byte[] buffer)
        {
            try
            {
                XmlSerializer x = new XmlSerializer(typeof(MessageBase));
                MemoryStream s = new MemoryStream();
                s.Write(buffer, 0, buffer.Length);
                s.Position = 0;
                var msg = x.Deserialize(s) as MessageBase;

                switch (msg.Type)
                {
                    case (int)MessageBase.Types.Message:
                        Window.PostMessage(msg.Nickname+": "+msg.Message);
                        break;
                    case (int)MessageBase.Types.LoggedInBroadcast:
                        Window.PostUsers(msg.LoggedInUsers);
                        break;
                    default:
                        break;
                }

            }
            catch (SerializationException e)
            {
                Console.WriteLine("SerializationException {0}", e);
            }
            return null;
        }
        public void Login(string nickname)
        {
            MessageBase msg = new MessageBase();
            msg.Type = (int)MessageBase.Types.Login;
            msg.Nickname = nickname;
            SendMessageAsync(msg);
        }
        /*
        * Previously used method to read strings from network stream.
        */
        private void ReceivingMethod()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[BufferSize];
                    int x = Stream.Read(buffer, 0, BufferSize);
                    if (x == 0)
                    {
                        TimeoutCounter = TimeoutCounter + 1;
                        if (TimeoutCounter > Timeout)
                        {
                            Client.Close();
                            Console.WriteLine("Disconnected");
                            break;
                        }
                        Thread.Sleep(2000);
                        Console.WriteLine("0 bytes received, sleeping thread...");
                        Console.WriteLine("Host not connected...");
                    }
                    else
                    {
                        TimeoutCounter = 0;
                        byte[] bytes = BitConverter.GetBytes(x);
                        string str = Encoding.UTF8.GetString(buffer, 0, x);
                        Console.WriteLine("Bytes received: {0}",x);
                        Window.PostMessage(str);
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine("IOException {0}", e);
                    break;
                }
            }
        }

    }
}