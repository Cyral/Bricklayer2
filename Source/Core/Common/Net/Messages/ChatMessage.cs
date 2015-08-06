using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Chat message.
    /// </summary>
    public class ChatMessage : IMessage
    {
        /// <summary>
        /// Message text.
        /// </summary>
        public string Message { get; private set; }

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
