using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chatclient
{
    public class Receiver
    {
        private TCPServer Server;
        private const int BufferSize = 1024;
        private const int Timeout = 2;
        private int TimeoutCounter;
        public NetworkStream Stream;
        protected TcpClient Client;
        private List<MessageBase> MessageQueue;
        private EndPoint ip;
        private byte[] ReadBuffer = new byte[BufferSize];
        private byte[] WriteBuffer = new byte[BufferSize];
        private Thread thread;

        public Receiver(TCPServer server, TcpListener listener, TcpClient client)
        {
            Server = server;
            Stream = client.GetStream();
            this.Client = client;
            ip = client.Client.RemoteEndPoint;
            MessageQueue = new List<MessageBase>();
            thread = new Thread(Start);
            thread.Start();
        }

        public void Start()
        {

            while (Client.Connected)
            {
                Read();
            }
            Console.WriteLine("{0} disconnected.",ip);
            Server.DisposeReceiver(this);
        }
        /*
        * Only reads 0 bytes when socket on the other end is not responding.
        */
        private void Read()
        {
            try
            {
                int x = Stream.Read(ReadBuffer, 0, BufferSize);
                if (x == 0)
                {
                    TimeoutCounter = TimeoutCounter + 1;
                    if (TimeoutCounter > Timeout)
                    {
                        Client.Close();
                        return;
                    }
                    Thread.Sleep(2000);
                    Console.WriteLine("0 bytes received, sleeping thread...");
                }
                else
                {
                    TimeoutCounter = 0;
                    byte[] bytes = BitConverter.GetBytes(x);
                    string str = Encoding.UTF8.GetString(ReadBuffer, 0, x);
                    Console.WriteLine("Message received: {0}\nBytes received: {1}", str, x);
                    OnMessageReceived(str);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Exception {0}", e);
                Client.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine("IOException {0}", e);
            }
        }

        private void Write(string msg)
        {
            WriteBuffer = Encoding.UTF8.GetBytes(msg);
            try
            {
                Stream.Write(WriteBuffer, 0, WriteBuffer.Length);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Exception {0}", e);
            }
            catch (IOException e)
            {
                Console.WriteLine("IOException {0}", e);
            }
        }

        public Task WriteAsync(string msg)
        {
            WriteBuffer = Encoding.UTF8.GetBytes(msg);
            try
            {
                var write = Stream.WriteAsync(WriteBuffer, 0, WriteBuffer.Length);
                write.ContinueWith(task =>
                {
                    Console.WriteLine("Message sent: {0}\nBytes sent: {1}", msg, WriteBuffer.Length);
                });
                return write;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception {0}", e);
            }
            return null;
        }

        private void OnMessageReceived(string msg)
        {
            Server.BroadcastMessage(msg);
        }

        private void SendMessage(MessageBase msg)
        {
            MessageQueue.Add(msg);
        }

        private Task ReadAsync()
        {
            try
            {
                var read = Stream.ReadAsync(ReadBuffer, 0, BufferSize);
                read.ContinueWith(task =>
                {
                    byte[] bytes = BitConverter.GetBytes(task.Result);
                    string str = Encoding.UTF8.GetString(ReadBuffer, 0, task.Result);
                    Console.WriteLine("Message received: {0}\nBytes received: {1}", str, task.Result);
                });
                return read;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception {0}", e);
            }
            return null;
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
                        f.Serialize(Stream, msg);
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine("IOException {0}", e);
                    }
                    MessageQueue.Remove(msg);
                }
                Thread.Sleep(60);
            }
        }

        private void ReceivingMethod()
        {
            while (true)
            {
                try
                {
                    if (!Stream.DataAvailable) continue;
                    BinaryFormatter f = new BinaryFormatter();
                    MessageBase msg = f.Deserialize(Stream) as MessageBase;
                }
                catch (IOException e)
                {
                    Console.WriteLine("IOException {0}", e);
                }
                Thread.Sleep(60);
            }
        }

        private void ValidationRequestHandler(ValidationRequest request)
        {
            ValidationResponse response = new ValidationResponse();

            if (request.Nickname.Length <= 10)
            {
                response.IsValid = true;
                response.HasError = false;
                SendMessage(response);
            }

        }
    }
}
