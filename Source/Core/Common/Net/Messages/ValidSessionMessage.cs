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
        public string Username { get; set; }

        /// <summary>
        /// The user's unique ID.
        /// </summary>
        public string UUID { get; set; }

        /// <summary>
        /// Is the session valid?
        /// </summary>
        public bool Valid { get; set; }

        public ValidSessionMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.ValidSession;

        public void Decode(NetIncomingMessage im)
        {
            Valid = im.ReadBoolean();
            if (Valid)
            {
                Username = im.ReadString();
                UUID = im.ReadString();
            }
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Valid);
            if (Valid)
            {
                om.Write(Username);
                om.Write(UUID);
            }
        }

        #endregion
    }
}