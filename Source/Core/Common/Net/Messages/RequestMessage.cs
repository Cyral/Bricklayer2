using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Used by client to request messages from server.
    /// Client => Server
    /// </summary>
    public class RequestMessage : IMessage
    {
        public MessageContext Context { get; set; }
        public MessageTypes Type { get; private set; }
   
        public RequestMessage(NetIncomingMessage im, MessageContext context) { Context = context; Decode(im); }

        public RequestMessage(MessageTypes type)
        {
            Type = type;
        }

        public MessageTypes MessageType => MessageTypes.Request;

        public void Decode(NetIncomingMessage im)
        {
            Type = (MessageTypes) im.ReadByte();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write((byte) Type);
        }
    }
}
