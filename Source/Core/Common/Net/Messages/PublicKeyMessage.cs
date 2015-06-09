using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    public class PublicKeyMessage : IMessage
    {
        public double MessageTime { get; set; }
        public string Username { get; set; }
        public string PublicKey { get; set; }

        public PublicKeyMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public PublicKeyMessage(string username, string publicKey)
        {
            Username = username;
            PublicKey = publicKey;
            MessageTime = NetTime.Now;
        }

        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.PublicKey;

        public void Decode(NetIncomingMessage im)
        {
            Username = im.ReadString();
            PublicKey = im.ReadString();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Username);
            om.Write(PublicKey);
        }

        #endregion
    }
}
