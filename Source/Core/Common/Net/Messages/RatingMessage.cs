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
        public double Rating { get; set; }

        public RatingMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public RatingMessage(Guid level, double rating)
        {
            Level = level;
            Rating = rating;
        }

        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.Rating;

        public void Decode(NetIncomingMessage im)
        {
            Level = Guid.Parse(im.ReadString());
            Rating = im.ReadDouble();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Level.ToString());
            om.Write(Rating);
        }

        #endregion
    }
}
