using System;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Sent to the server to join a level.
    /// Client => Server
    /// String: Level's UUID
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

        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.JoinLevel;

        public void Decode(NetIncomingMessage im)
        {
            Guid result;
            if (Guid.TryParse(im.ReadString(), out result));
                UUID = result;
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(UUID.ToString("N"));
        }

        #endregion
    }
}
