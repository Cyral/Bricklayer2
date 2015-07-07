using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Common.Entity
{
    /// <summary>
    /// Represents the direction a player is facing
    /// </summary>
    public enum FacingDirection
    {
        Left,
        Right,
    }

    /// <summary>
    /// Represents a direction to check for world collisions
    /// </summary>
    public enum CollisionDirection
    {
        Vertical,
        Horizontal,
    }
}
