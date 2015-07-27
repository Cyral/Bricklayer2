using System;
using Bricklayer.Core.Common.Data;

namespace Bricklayer.Core.Server.World
{
    public class Level : Common.World.Level
    {
        public Level(PlayerData creator, string name, Guid uuid, string description, int plays, double rating) :
            base(creator, name, uuid, description, plays, rating)
        {

        }

        public Level(LevelData level) : base(level)
        {
            
        }
    }
}
