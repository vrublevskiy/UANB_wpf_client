using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnbCore
{
    public class NotificationEventArgs : EventArgs
    {
        public NotificationEventArgs(string sender, string receiver, string messageType, string message)
        {
            Sender = sender;
            Receiver = receiver;
            MessageType = messageType;
            Message = message;
        }

        public NotificationEventArgs(string msgText)
        {
            var lines = msgText.Split(new[] { ';' });

            Sender = lines[0].Split(new[] { ':' })[1];
            Receiver = lines[1].Split(new[] { ':' })[1];
            MessageType = lines[2].Split(new[] { ':' })[1];
            Message = lines[3].Split(new[] { ':' })[1];
        }

        public string Sender { get; private set; }

        public string Receiver { get; private set; }

        public string MessageType { get; private set; }

        public string Message { get; private set; }
    }
}
