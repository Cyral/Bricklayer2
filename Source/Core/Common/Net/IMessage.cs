using Lidgren.Network;

namespace Bricklayer.Core.Common.Net
{
    /// <summary>
    /// Represents a networking message.
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// The host the message is being read on. (Server or client)
        /// </summary>
        MessageContext Context { get; set; }

        /// <summary>
        /// The type/id of the message, so the reciever knows what content is expected.
        /// </summary>
        MessageTypes MessageType { get; }

        /// <summary>
        /// Reads the message.
        /// </summary>
        void Decode(NetIncomingMessage im);

        /// <summary>
        /// Writes the message.
        /// </summary>
        void Encode(NetOutgoingMessage om);
    }
}