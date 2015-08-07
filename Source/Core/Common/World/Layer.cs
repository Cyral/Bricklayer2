using System;

namespace Bricklayer.Core.Common.World
{
    /// <summary>
    /// Represents a layer for tiles to be drawn on and interacted with.
    /// </summary>
    [Flags]
    public enum Layer : byte
    {
        Foreground = 1,
        Background = 0,
        All = Foreground | Background
    }
}