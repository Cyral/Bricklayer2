using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Sends the initialization response to the client after logging in.
    /// Auth Server => Client
    /// String: User's username (In case of incorrect case, or automatically assigned guest username)
    /// String: User's UUID
    /// String: Public Key
    /// String: Private Key
    /// </summary>
    public class AuthInitMessage : IMessage
    {
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public string Username { get; set; }
        public string UUID { get; set; }

        public AuthInitMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.AuthInit;

        public void Decode(NetIncomingMessage im)
        {
            Username = im.ReadString();
            UUID = im.ReadString();
            PrivateKey = im.ReadString();
            PublicKey = im.ReadString();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Username);
            om.Write(UUID);
            om.Write(PrivateKey);
            om.Write(PublicKey);
        }

        #endregion
    }
}