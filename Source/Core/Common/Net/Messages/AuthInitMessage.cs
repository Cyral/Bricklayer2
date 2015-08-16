using System;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Sends the initialization response to the client after logging in.
    /// Auth Server => Client
    /// String: Player's username (In case of incorrect case, or automatically assigned guest username)
    /// Byte(16): Player's UUID
    /// String: Public Key
    /// String: Private Key
    /// </summary>
    public class AuthInitMessage : IMessage
    {
        public byte[] PrivateKey { get; private set; }
        public byte[] PublicKey { get; private set; }
        public string Username { get; private set; }
        public Guid UUID { get; private set; }

        public AuthInitMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.AuthInit;

        public void Decode(NetIncomingMessage im)
        {
            Username = im.ReadString();
            UUID = im.ReadGuid();
            var privLength = im.ReadUInt16();
            PrivateKey = im.ReadBytes(privLength);
            var pubLength = im.ReadUInt16();
            PublicKey = im.ReadBytes(pubLength);
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Username);
            om.Write(UUID);
            om.Write((ushort)PrivateKey.Length);
            om.Write(PrivateKey);
            om.Write((ushort)PublicKey.Length);
            om.Write(PublicKey);
        }
    }
}