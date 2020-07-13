using System;
using System.Collections.Generic;
using System.Text;

namespace Twelvedata.Client.WebSocket.Models
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class MessageTypeAttribute : Attribute
    {
        public string Type { get; }

        public MessageTypeAttribute(string type)
        {
            Type = type;
        }
    }
}
