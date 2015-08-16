using System;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Sent to the server to join a level.
    /// Client => Server
    /// Byte(16): Level's GUID
    /// </summary>
    public class JoinLevelMessage : IMessage
    {
        /// <summary>
        /// The UUID of the level to join.
        /// </summary>
        public Guid UUID { get; private set; }

        public JoinLevelMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public JoinLevelMessage(Guid uuid)
        {
            UUID = uuid;
        }

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.JoinLevel;

        public void Decode(NetIncomingMessage im)
        {
            UUID = im.ReadGuid();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(UUID);
        }
    }
}
