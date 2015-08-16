using System;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Contains the public key and the username that goes with it. 
    /// Client => Game Server => Auth Server
    /// String: Username
    /// Byte(16): UUID
    /// String: Public Key
    /// </summary>
    public class PublicKeyMessage : IMessage
    {
        public string Username { get; private set; }
        public Guid UUID { get; private set; }
        public byte[] PublicKey { get; private set; }

        public PublicKeyMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public PublicKeyMessage(string username, Guid uuid, byte[] publicKey)
        {
            UUID = uuid;
            Username = username;
            PublicKey = publicKey;
        }

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.PublicKey;

        public void Decode(NetIncomingMessage im)
        {
            Username = im.ReadString();
            UUID = im.ReadGuid();
            var length = im.ReadUInt16();
            PublicKey = im.ReadBytes(length);
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Username);
            om.Write(UUID);
            om.Write((ushort)PublicKey.Length);
            om.Write(PublicKey);
        }
    }
}
