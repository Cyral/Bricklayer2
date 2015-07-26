using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Used by client to request packets from server 
    /// </summary>
    public class RequestMessage : IMessage
    {
        public MessageContext Context { get; set; }
        public MessageTypes Type { get; set; }
   
        public RequestMessage(NetIncomingMessage im, MessageContext context) { Context = context; Decode(im); }

        public RequestMessage(MessageTypes type)
        {
            Type = type;
        }

        #region IMessage Members

        public MessageTypes MessageType => MessageTypes.Request;

        public void Decode(NetIncomingMessage im)
        {
            Type = (MessageTypes) im.ReadByte();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write((byte) Type);
        }

        #endregion
    }
}
