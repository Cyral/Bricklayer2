using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    public class RatingMessage : IMessage
    {
        public Guid Level { get; set; }
        public int Rating { get; set; }

        public RatingMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public RatingMessage(Guid level, int rating)
        {
            Level = level;
            Rating = rating;
        }

        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.Rating;

        public void Decode(NetIncomingMessage im)
        {
            Level = im.ReadGuid();
            Rating = im.ReadInt32();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Level);
            om.Write(Rating);
        }

        #endregion
    }
}
