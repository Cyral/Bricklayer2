using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Bricklayer.Core.Common.Entity
{
    /// <summary>
    /// Represents the current state of an entity (Its position, velocity, etc)
    /// </summary>
    public class EntityState
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Movement { get; set; }
        public Rectangle Bounds { get; set; }
    }
}
