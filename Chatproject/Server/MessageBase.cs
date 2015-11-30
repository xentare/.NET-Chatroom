using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Chatclient
{
    [Serializable]
    public class MessageBase
    {
        public enum Types
        {
            Message,
            Login,
            LoggedInBroadcast
        }

        public string LoggedInUsers { get; set; }
        public int Type { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public string Nickname { get; set; }
    }
}
