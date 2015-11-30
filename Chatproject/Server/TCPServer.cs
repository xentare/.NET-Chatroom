using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Chatclient
{
    public class TCPServer
    {

        private List<Receiver> receivers = new List<Receiver>();
        private Dictionary<string,Receiver> clients = new Dictionary<string, Receiver>();

        public TCPServer()
        {
        }

        public static ManualResetEvent allDone = new ManualResetEvent(false);

        /*
        * Starts listening for incoming socket connections.
        * Accepts connections asyncronously without blocking the main thread.
        */
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
        }

        public void AddReceiver(string auth, Receiver receiver)
        {
            clients.Add(auth,receiver);
            BroadcastLoggedIn();
        }

        public void BroadcastLoggedIn()
        {
            MessageBase msg = new MessageBase();
            string users = null;
            msg.Type = (int)MessageBase.Types.LoggedInBroadcast;

            foreach (var key in clients.Keys)
            {
                users += clients[key].Nickname+",";
            }
            msg.LoggedInUsers = users;
            BroadcastMessage(msg);
        }

        public void BroadcastMessage(MessageBase msg)
        {
            foreach(var key in clients.Keys)
            {
                clients[key].SendMessageAsync(msg);
            }
        }

        public void DisposeReceiver(Receiver receiver)
        {
            clients.Remove(receiver.Nickname);
            BroadcastLoggedIn();
        }

    }
}