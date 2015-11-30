using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Chatclient
{
    public class Receiver
    {
        private TCPServer Server;
        public string Nickname;
        private const int BufferSize = 1024;
        private const int Timeout = 2; //Every timeout is 2 seconds
        private int TimeoutCounter;
        public NetworkStream Stream;
        protected TcpClient Client;
        private EndPoint ip;
        private byte[] ReadBuffer = new byte[BufferSize];
        private byte[] WriteBuffer = new byte[BufferSize];
        private Thread thread;

        public Receiver(TCPServer server, TcpListener listener, TcpClient client)
        {
            Server = server;
            Stream = client.GetStream();
            Client = client;
            ip = client.Client.RemoteEndPoint;
            thread = new Thread(Start);
            thread.Start();
        }
        public void Start()
        {

            while (Client.Connected)
            {
                ReadSerializable();
            }
            Console.WriteLine("{0} disconnected.",ip);
            Server.DisposeReceiver(this);
        }
        /*
        * Thread blocking method to read network stream. Passes readed bytes to OnSerializableReceived
        * Only reads 0 bytes when socket on the other end is not responding.
        */
        private void ReadSerializable()
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
                        return;
                    }
                    Thread.Sleep(2000);
                    Console.WriteLine("0 bytes received, sleeping thread...");
                    return;
                }
                OnSerializableReceived(ref buffer);
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
            catch (SerializationException e)
            {
                Console.WriteLine("SerializationException {0}", e);
            }
        }
        /*
        * Handles MessageBase objects.
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
                        OnMessageReceived(msg);
                        break;
                    case (int)MessageBase.Types.Login:
                        Nickname = msg.Nickname;
                        Server.AddReceiver(msg.Nickname,this);
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
        /*
        * Serializes message to network stream.
        */
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
                Console.WriteLine("{0}", e);
            }

            return null;
        }
        /*
        * Previously used write method for strings
        */
        public Task WriteAsync(string msg)
        {
            WriteBuffer = Encoding.UTF8.GetBytes(msg);
            try
            {
                var write = Stream.WriteAsync(WriteBuffer, 0, WriteBuffer.Length);
                write.ContinueWith(task =>
                {
                    Console.WriteLine("Bytes sent: {0}",WriteBuffer.Length);
                });
                return write;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception {0}", e);
            }
            return null;
        }
        private void OnMessageReceived(MessageBase msg)
        {
            Server.BroadcastMessage(msg);
        }
        /*
        * Previously used method to read string from stream.
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
                    //Console.WriteLine("0 bytes received, sleeping thread...");
                }
                else
                {
                    TimeoutCounter = 0;
                    byte[] bytes = BitConverter.GetBytes(x);
                    string str = Encoding.UTF8.GetString(ReadBuffer, 0, x);
                    Console.WriteLine("Bytes received: {0}", x);
                    //OnMessageReceived(str);
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
    }
}
