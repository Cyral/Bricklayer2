using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    public class LoginMessage : IMessage
    {
        public MessageContext Context { get; set; }
        public MessageTypes MessageType { get; }

        public void Decode(NetIncomingMessage im)
        {
            throw new NotImplementedException();
        }

        public void Encode(NetOutgoingMessage om)
        {
            throw new NotImplementedException();
        }
    }
}
