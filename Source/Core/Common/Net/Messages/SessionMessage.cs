using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Info sent from client to auth server to request session for a game server
    /// </summary>
    public class SessionMessage : IMessage
    {
        public double MessageTime { get; set; }
        public string Username { get; set; }
        public string PrivateKey { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }

        public SessionMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public SessionMessage(string username, string privateKey, string address, int port)
        {
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
            Address = im.ReadString();
            Port = im.ReadInt32();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Username);
            om.Write(PrivateKey);
            om.Write(Address);
            om.Write(Port);
        }

        #endregion
    }
}
