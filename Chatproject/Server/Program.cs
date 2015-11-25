using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chatclient
{
    internal class Program
    {
        private static void Main()
        {
            TCPServer server = new Chatclient.TCPServer();
            server.StartListening();
        }
    }
}
