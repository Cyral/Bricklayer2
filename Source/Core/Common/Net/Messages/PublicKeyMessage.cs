using System;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Contains the public key and the username that goes with it. 
    /// Client => Game Server => Auth Server
    /// String: Username
    /// String: UUID
    /// String: Public Key
    /// </summary>
    public class PublicKeyMessage : IMessage
    {
        public string Username { get; set; }
        public string UUID { get; set; }
        public string PublicKey { get; set; }

        public PublicKeyMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public PublicKeyMessage(string username, Guid uuid, string publicKey)
        {
            UUID = uuid.ToString("N");
            Username = username;
            PublicKey = publicKey;
        }

        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.PublicKey;

        public void Decode(NetIncomingMessage im)
        {
            Username = im.ReadString();
            UUID = im.ReadString();
            PublicKey = im.ReadString();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Username);
            om.Write(UUID);
            om.Write(PublicKey);
        }

        #endregion
    }
}
