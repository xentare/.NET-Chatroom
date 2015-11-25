using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    internal class TcpClient
    {
        private const int BufferSize = 1024;
        private const int Timeout = 2;
        private int TimeoutCounter;
        private NetworkStream stream;
        private List<MessageBase> MessageQueue = new List<MessageBase>();
        private Thread receivingThread;
        private System.Net.Sockets.TcpClient client;
        private MainWindow window;

        public TcpClient(MainWindow window)
        {
            this.window = window;
            //receivingThread = new Thread(ReceivingMethod);
            //sendingThread = new Thread(SendingMethod);
        }

        public void Close()
        {
            //ResponseTextMessage msg = new ResponseTextMessage();
            //msg.Text = "<CLOSE>";
            try
            {
                //SendMessageAsync(msg);
                client.Close();
                receivingThread.Abort();
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException {0}", e);
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("Cant disconnected since we are not connected");
                Console.WriteLine("NullReferenceException {0}", e);
            }
        }

        public void Start()
        {
            try
            {
                client = new System.Net.Sockets.TcpClient();
                client.Connect("127.0.0.1", 5555);
                stream = client.GetStream();
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketExecption {0}", e);
            }
            if (!client.Connected) return;
            receivingThread = new Thread(ReceivingMethod);
            receivingThread.Start();
        }

        public void AddMessageToQueue(MessageBase message)
        {
            MessageQueue.Add(message);
        }

        public Task SendMessageAsync(MessageBase msg)
        {
            string str = (msg as ResponseTextMessage).Text;
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            try
            {
                var writeTask = stream.WriteAsync(buffer, 0, buffer.Length);
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
                Console.WriteLine("IOException {0}", e);
            }
            return null;
        }

        private Task ReadMessageAsync()
        {
            if (!stream.DataAvailable)
            {
                byte[] buffer = new byte[BufferSize];
                var read = stream.ReadAsync(buffer, 0, BufferSize).ContinueWith(task =>
                {
                    byte[] bytes = BitConverter.GetBytes(task.Result);
                    string str = Encoding.UTF8.GetString(buffer, 0, task.Result);
                    Console.WriteLine("Message received: {0}\nBytes received: {1}", str, task.Result);
                });
                return read;
            }
            return ReadMessageAsync();
        }

        private void ReceivingMethod()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[BufferSize];
                    int x = stream.Read(buffer, 0, BufferSize);
                    if (x == 0)
                    {
                        TimeoutCounter = TimeoutCounter + 1;
                        if (TimeoutCounter > Timeout)
                        {
                            client.Close();
                            break;
                        }
                        Thread.Sleep(2000);
                        Console.WriteLine("0 bytes received, sleeping thread...");
                    }
                    else
                    {
                        TimeoutCounter = 0;
                        byte[] bytes = BitConverter.GetBytes(x);
                        string str = Encoding.UTF8.GetString(buffer, 0, x);
                        Console.WriteLine("Message received: {0}\nBytes received: {1}", str, x);
                        window.PostMessage(str);
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine("IOException {0}", e);
                    break;
                }
            }
        }

        private void SendingMethod()
        {
            while (true)
            {
                if (MessageQueue.Count > 0)
                {
                    MessageBase msg = MessageQueue[0];
                    try
                    {
                        BinaryFormatter f = new BinaryFormatter();
                        f.Serialize(stream, msg);
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                    MessageQueue.Remove(msg);
                }
                Thread.Sleep(60);
            }
        }

        public void Login(String nickname)
        {
            ValidationRequest request = new ValidationRequest { Nickname = nickname };

            AddMessageToQueue(request);
        }

        private void OnMessageReceived(MessageBase msg)
        {
            Type type = msg.GetType();

            if (type == typeof(ResponseTextMessage))
            {
                Console.WriteLine("{0}", (msg as ResponseTextMessage).Text);
            }

            if (type == typeof(ValidationResponse))
            {
                if (msg.IsValid)
                {
                    Console.WriteLine("Succesfully validated client!");
                }
                else
                {
                    Console.WriteLine("Validation failed!");
                }
            }

        }


    }
}