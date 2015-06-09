using Bricklayer.Core.Common.Net;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Message sent from server to inform client of wrong login credentials
    /// </summary>
    public class FailedLoginMessage : IMessage
    {

        public string ErrorMessage { get; set; } // Reason of failed login

        public double MessageTime { get; set; }

        public MessageContext Context { get; set; }

        public FailedLoginMessage(NetIncomingMessage im, MessageContext context) { Context = context; Decode(im); }

        public FailedLoginMessage(string errorMessage)
        {
            ErrorMessage = errorMessage;
            MessageTime = NetTime.Now;
        }

        #region IMessage Members

        public MessageTypes MessageType { get { return MessageTypes.FailedLogin; } }

        public void Decode(NetIncomingMessage im)
        {
            ErrorMessage = im.ReadString();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(ErrorMessage);
        }

        #endregion
    }
}
