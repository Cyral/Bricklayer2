using System.Net;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Info sent from client to auth server to request session for a game server.
    /// </summary>
    public class SessionMessage : IMessage
    {
        public IPAddress Address { get; set; }
        public double MessageTime { get; set; }
        public int Port { get; set; }
        public string PrivateKey { get; set; }
        public string Username { get; set; }
        public int ID { get; set; }

        public SessionMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public SessionMessage(string username, int id, string privateKey, IPAddress address, int port)
        {
            ID = id;
            Username = username;
            PrivateKey = privateKey;
            Address = address;
            Port = port;
            MessageTime = NetTime.Now;
        }

        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.Session;

        public void Decode(NetIncomingMessage im)
        {
            Username = im.ReadString();
            PrivateKey = im.ReadString();
            ID = im.ReadInt32();
            Address = IPAddress.Parse(im.ReadString());
            Port = im.ReadInt32();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Username);
            om.Write(PrivateKey);
            om.Write(ID);
            om.Write(Address.ToString());
            om.Write(Port);
        }

        #endregion
    }
}