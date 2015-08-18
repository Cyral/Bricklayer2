using System;
using System.IO;
using Bricklayer.Core.Common.Data;
using Bricklayer.Core.Common.Net.Messages;
using Bricklayer.Core.Common.World;

namespace Bricklayer.Core.Server.World
{
    public class Level : Common.World.Level
    {
        /// <summary>
        /// The game server.
        /// </summary>
        public Server Server { get; internal set; }

        public Level(Server server, PlayerData creator, string name, Guid uuid, string description, int plays, int rating) :
            base(creator, name, uuid, description, plays, rating)
        {
            Server = server;
            Tiles.BlockPlaced = BlockPlaced; 
        }

        public Level(Server server, LevelData level) : base(level)
        {
            Server = server;
            Tiles.BlockPlaced = BlockPlaced;
        }

        /// <summary>
        /// Action to be run when a tile is changed after the world is generated.
        /// This is called by the tilemap array indexer.
        /// </summary>
        private void BlockPlaced(int x, int y, int z, Tile newTile, Tile oldTile)
        {
            //Send block placed message to all users in this level.
            Server.Net.Broadcast(this, new BlockPlaceMessage(x, y, z, newTile));

            //Fire event so plugins are aware of the block placement.
            //Blocks placed by players are handled by the network component and do not flow through this method.
            Server.Events.Game.Level.BlockPlaced.Invoke(new EventManager.GameEvents.LevelEvents.BlockPlacedEventArgs(this, x, y, z, newTile, oldTile));
        }

        internal override void DecodeTiles(BinaryReader reader)
        {
            base.DecodeTiles(reader);
            Tiles.BlockPlaced = BlockPlaced;
        }
    }
}
