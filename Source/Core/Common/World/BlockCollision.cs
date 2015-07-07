using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Common.World
{
    /// <summary>
    /// Represents the type of collision a <c>BlockType</c> has, and how it interacts with a player.
    /// </summary>
    public enum BlockCollision
    {
        Impassable,
        Passable,
        Platform,
        Gravity,
    }
}
