using Microsoft.Xna.Framework;

namespace Bricklayer.Core.Common.Entity
{
    /// <summary>
    /// Represents a physical state of an entity.
    /// </summary>
    public class EntityState
    {
        /// <summary>
        /// The bounds of the entity in this state.
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// The input data for this state.
        /// </summary>
        public Vector2 Movement { get; set; }

        /// <summary>
        /// The position of this state.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// The velocity (speed/direction) of this state.
        /// </summary>
        public Vector2 Velocity { get; set; }
    }
}