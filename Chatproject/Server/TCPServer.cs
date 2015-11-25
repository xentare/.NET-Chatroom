using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Chatclient
{
    public class TCPServer
    {

        private List<Receiver> clients = new List<Receiver>();

        public TCPServer()
        {
        }

        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public void StartListening()
        {
            TcpListener listener = new TcpListener(System.Net.IPAddress.Any, 5555);
            listener.Start();
            try
            {
                while (true)
                {
                    allDone.Reset();
                    Console.WriteLine("Waiting for connections...");
                    listener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), listener);
                    allDone.WaitOne();
                }

            }
            catch (SocketException e)
            {
                Console.WriteLine("Socket exeption {0}", e);
            }
            finally
            {
                listener.Stop();
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();
            var listener = (TcpListener)ar.AsyncState;
            var handler = listener.EndAcceptTcpClient(ar);
            Console.WriteLine(handler.Client.RemoteEndPoint + " connected.");
            var receiver = new Receiver(this, listener, handler);
            clients.Add(receiver);
        }

        public void BroadcastMessage(string msg)
        {
            foreach(Receiver r in clients)
            {
                var write = r.WriteAsync(msg);
            }
        }

        public void DisposeReceiver(Receiver receiver)
        {
            clients.Remove(receiver);
        }

    }
}