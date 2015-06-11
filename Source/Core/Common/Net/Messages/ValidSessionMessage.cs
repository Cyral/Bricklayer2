using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Sent from auth server to game server to inform whether or not the user with the public key has a valid session.
    /// Auth Server => Game Server
    /// Bool: Valid
    /// If Valid:
    /// String: Username
    /// Int: ID
    /// </summary>
    public class ValidSessionMessage : IMessage
    {
        public double MessageTime { get; set; }
        public string Username { get; set; }
        public int ID { get; set; }
        public bool Valid { get; set; }

        public ValidSessionMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public ValidSessionMessage(string username, int id, bool valid)
        {
            Username = username;
            ID = id;
            Valid = valid;
            MessageTime = NetTime.Now;
        }

        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.ValidSession;

        public void Decode(NetIncomingMessage im)
        {
            Valid = im.ReadBoolean();
            if (Valid)
            {
                Username = im.ReadString();
                ID = im.ReadInt32();
            }
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Valid);
            if (Valid)
            {
                om.Write(Username);
                om.Write(ID);
            }
        }

        #endregion
    }
}
