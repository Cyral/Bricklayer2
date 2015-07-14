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
        public Response Response { get; set; }
        public string Info { get; set; }

        public PingAuthMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public PingAuthMessage(Response response, string info)
        {
            Response = response;
            Info = info;
        }

        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.PingAuth;

        public void Decode(NetIncomingMessage im)
        {
            Response = (Response)im.ReadByte();
            Info = im.ReadString();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write((byte)Response);
            om.Write(Info);
        }

        #endregion
    }

    public enum Response : byte
    {
        GotPlugin
    }
}
