using System;
using System.Net;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Info sent from client to auth server to request session for a game server.
    /// String: Username
    /// String: Private Key
    /// Int: ID
    /// Byte Array: Host's IP
    /// Int: Host's Port
    /// </summary>
    public class SessionMessage : IMessage
    {
        public IPAddress Address { get; private set; }
        public int Port { get; private set; }
        public byte[] PrivateKey { get; private set; }
        public string Username { get; private set; }
        public Guid UUID { get; private set; }

        public SessionMessage(string username, Guid uuid, byte[] privateKey, IPAddress address, int port)
        {
            UUID = uuid;
            Username = username;
            PrivateKey = privateKey;
            Address = address;
            Port = port;
        }

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.Session;

        public void Decode(NetIncomingMessage im)
        {
            Username = im.ReadString();
            var length = im.ReadUInt16();
            PrivateKey = im.ReadBytes(length);
            UUID = im.ReadGuid();
            Address = im.ReadIPAddress();
            Port = im.ReadInt32();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Username);
            om.Write((ushort)PrivateKey.Length);
            om.Write(PrivateKey);
            om.Write(UUID);
            om.Write(Address);
            om.Write(Port);
        }
    }
}