using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    public class ChatMessage : IMessage
    {
        /// <summary>
        /// Chat message 
        /// </summary>
        public string Message { get; set; }

        public ChatMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public ChatMessage(string message)
        {
            Message = message;
        }

        #region IMessage Members

        public MessageContext Context { get; set; }

        public MessageTypes MessageType => MessageTypes.Chat;

        public void Decode(NetIncomingMessage im)
        {
            Message = im.ReadString();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Message);
        }

        #endregion
    }
}
