using Bricklayer.Core.Common.Entity;
using Lidgren.Network;
using Microsoft.Win32;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Chat message.
    /// </summary>
    public class ChatMessage : IMessage
    {
        public static readonly int MaxChatLength = 1024;

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

        public MessageContext Context { get; set; }

        public MessageTypes MessageType => MessageTypes.Chat;

        public void Decode(NetIncomingMessage im)
        {
            Message = im.ReadString().Truncate(MaxChatLength);
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Message.Truncate(MaxChatLength));
        }
    }
}
