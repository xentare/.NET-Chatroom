using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatclient
{
    [Serializable]
    internal class MessageBase
    {
        public Guid CallbackID { get; set; }
        public bool IsValid { get; set; }
        public bool HasError { get; set; }
        public Exception Exception { get; set; }

        public MessageBase()
        {
            Exception = new Exception();
        }
    }
    [Serializable]
    internal class RequestMessageBase : MessageBase
    {
        
    }
    [Serializable]
    internal class ResponseMessageBase : MessageBase
    {
        public bool DeleteCallbackAfterInvoke { get; set; }

    }
    [Serializable]
    internal class ResponseTextMessage : ResponseMessageBase
    {
        public string Text { get; set; }
    }
    [Serializable]
    internal class ValidationRequest : RequestMessageBase
    {
        public string Nickname { get; set; }
    }
    [Serializable]
    internal class ValidationResponse : ResponseMessageBase
    {
        
    }
}
