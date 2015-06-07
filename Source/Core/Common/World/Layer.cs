using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Common.World
{
    /// <summary>
    /// Represents a layer for tiles to be drawn on and interacted with.
    /// </summary>
    public enum Layer : byte
    {
        Foreground,
        Background,
        All,
    }
}
