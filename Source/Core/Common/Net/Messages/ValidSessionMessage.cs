using System;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Sent from auth server to game server to inform whether or not the user with the public key has a valid session.
    /// Auth Server => Game Server
    /// Bool: Valid
    /// If Valid:
    /// String: Username
    /// String: Unique ID
    /// </summary>
    public class ValidSessionMessage : IMessage
    {
        /// <summary>
        /// The user's name.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// The user's unique ID.
        /// </summary>
        public Guid UUID { get; private set; }

        /// <summary>
        /// Is the session valid?
        /// </summary>
        public bool Valid { get; private set; }

        public ValidSessionMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.ValidSession;

        public void Decode(NetIncomingMessage im)
        {
            Valid = im.ReadBoolean();
            Username = im.ReadString();
            UUID = im.ReadGuid();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Valid);
            om.Write(Username);
            om.Write(UUID);
        }
    }
}