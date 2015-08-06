using System;
using Bricklayer.Core.Common.World;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Sent to the server to join a level.
    /// Client => Server
    /// String: Level's UUID
    /// </summary>
    public class BlockPlaceMessage : IMessage
    {
        /// <summary>
        /// Block coordinate.
        /// </summary>
        public Point Point { get; private set; }

        /// <summary>
        /// Block layer.
        /// </summary>
        public Layer Layer { get; private set; }

        /// <summary>
        /// New block type.
        /// </summary>
        public BlockType Type { get; private set; }

        public BlockPlaceMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public BlockPlaceMessage(int x, int y, BlockType type)
        {
            Point = new Point(x, y);
            Type = type;
            Layer = Layer.Foreground;
        }

        public BlockPlaceMessage(int x, int y, int z, BlockType type)
        {
            if (z > 1) //Throw an exception if the Z is out of range
                throw new ArgumentOutOfRangeException(nameof(x), "The Z coordinate cannot be more than 1.");

            Point = new Point(x, y);
            Type = type;
            Layer = (Layer) z;
        }

        public BlockPlaceMessage(int x, int y, Layer layer, BlockType type)
        {
            Point = new Point(x, y);
            Type = type;
            Layer = layer;
        }

        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.BlockPlace;

        public void Decode(NetIncomingMessage im)
        {
           im.Write((ushort)Point.X);
           im.Write((ushort)Point.Y);
           im.Write((byte)Layer);
           im.Write(Type.ID);
        }

        public void Encode(NetOutgoingMessage om)
        {
            Point = new Point(om.ReadUInt16(), om.ReadUInt16());
            Layer = (Layer) om.ReadByte();
            Type = BlockType.FromID(om.ReadUInt16());
        }

        #endregion
    }
}
