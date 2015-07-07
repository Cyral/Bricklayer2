using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Send response to Auth server
    /// Client => Auth Server
    /// String: Response
    /// </summary>
    public class PingAuthMessage : IMessage
    {
        public string Response { get; set; }

        public PingAuthMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public PingAuthMessage(string response)
        {
            Response = response;
        }

        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.PingAuth;

        public void Decode(NetIncomingMessage im)
        {
            Response = im.ReadString();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Response);
        }

        #endregion
    }
}
