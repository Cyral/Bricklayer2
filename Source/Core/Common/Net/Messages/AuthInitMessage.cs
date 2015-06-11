using Bricklayer.Core.Common.Net;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Sends the initialization response to the client after logging in.
    /// Auth Server => Client
    /// String: User's username (In case of incorrect case, or automatically assigned guest username)
    /// Int: User's ID (From database)
    /// String: Public Key
    /// String: Private Key
    /// </summary>
    public class AuthInitMessage : IMessage
    {
        public double MessageTime { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        public int UID { get; set; }
        public string Username { get; set; }

        public AuthInitMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public AuthInitMessage(string username, int databaseID, string privateKey, string publicKey)
        {
            Username = username;
            UID = databaseID;
            PrivateKey = privateKey;
            PublicKey = publicKey;
            MessageTime = NetTime.Now;
        }

        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.AuthInit;

        public void Decode(NetIncomingMessage im)
        {
            Username = im.ReadString();
            UID = im.ReadInt32();
            PrivateKey = im.ReadString();
            PublicKey = im.ReadString();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Username);
            om.Write(UID);
            om.Write(PrivateKey);
            om.Write(PublicKey);
        }

        #endregion
    }
}
