using Bricklayer.Core.Common.World;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Contains information about a level, as well as the array of tiles.
    /// Server => Client
    /// Sent when the client requests to join or create a room.
    /// </summary>
    public class LevelDataMessage : IMessage
    {
        public Level Level { get; private set; }

        public LevelDataMessage(Level level)
        {
            Level = level;
        }

        public LevelDataMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }


        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.LevelData;

        public void Decode(NetIncomingMessage im)
        {
            Level = new Level(im);
        }

        public void Encode(NetOutgoingMessage om)
        {
            Level.Encode(om);
        }
    }
}