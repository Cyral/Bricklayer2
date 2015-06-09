using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// What the Auth Server will send back when it has checked if the user's public key matches the stored public key to join a game server
    /// </summary>
    public class VerifiedMessage : IMessage
    {
        public double MessageTime { get; set; }
        public bool Verified { get; set; }

        public VerifiedMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public VerifiedMessage(bool verified)
        {
            Verified = verified;
            MessageTime = NetTime.Now;
        }

        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.Verified;

        public void Decode(NetIncomingMessage im)
        {
            Verified = im.ReadBoolean();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Verified);
        }

        #endregion
    }
}
