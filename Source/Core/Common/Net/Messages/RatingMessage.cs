using System;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Client => Server
    /// Rates a level.
    /// </summary>
    public class RatingMessage : IMessage
    {
        public Guid Level { get; private set; }
        public int Rating { get; private set; }

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