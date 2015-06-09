using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Sent from Auth server to game server to inform whether or not the user with the public key has a valid session
    /// </summary>
    public class ValidSessionMessage : IMessage
    {
        public double MessageTime { get; set; }
        public string Username { get; set; }
        public bool Valid { get; set; }

        public ValidSessionMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public ValidSessionMessage(string username, bool valid)
        {
            Username = username;
            Valid = valid;
            MessageTime = NetTime.Now;
        }

        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.ValidSession;

        public void Decode(NetIncomingMessage im)
        {
            Username = im.ReadString();
            Valid = im.ReadBoolean();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Username);
            om.Write(Valid);
        }

        #endregion
    }
}
