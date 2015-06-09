using Bricklayer.Core.Common.Net;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Common.Net.Messages
{
    public class AuthInitMessage : IMessage
    {
        public double MessageTime { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }

        public AuthInitMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public AuthInitMessage(string privateKey, string publicKey)
        {
            PrivateKey = privateKey;
            PublicKey = publicKey;
            MessageTime = NetTime.Now;
        }
        
        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.AuthInit;

        public void Decode(NetIncomingMessage im)
        {
            PrivateKey = im.ReadString();
            PublicKey = im.ReadString();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(PrivateKey);
            om.Write(PublicKey);
        }

        #endregion
    }
}
