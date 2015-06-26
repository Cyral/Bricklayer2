using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    public class RequestServerInfo : IMessage
    {
        public double MessageTime { get; set; }

        public RequestServerInfo(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public RequestServerInfo()
        {
            MessageTime = NetTime.Now;
        }

        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.RequestInfo;

        public void Decode(NetIncomingMessage im)
        {

        }

        public void Encode(NetOutgoingMessage om)
        {


        }

        #endregion
    }
}
