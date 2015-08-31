using Bricklayer.Core.Common;

namespace Bricklayer.Plugins.Minimap
{
    /// <summary>
    /// Minimap configuration options.
    /// </summary>
    public class MinimapConfig : IConfig
    {
        /// <summary>
        /// Height of the minimap.
        /// </summary>
        public int Height;

        /// <summary>
        /// Width of the minimap.
        /// </summary>
        public int Width;

        /// <summary>
        /// Left coordinate.
        /// </summary>
        public int X;

        /// <summary>
        /// Top coordinate.
        /// </summary>
        public int Y;

        /// <summary>
        /// Transparency (0 to 1)
        /// </summary>
        public float Alpha;

        /// <summary>
        /// Transparency of the camera region. (0 to 1)
        /// </summary>
        public float RegionAlpha;

        public IConfig GenerateDefaultConfig()
        {
            return new MinimapConfig
            {
                Width = 400,
                Height = 200,
                X = 8,
                Y = 8,
                Alpha = .95f,
                RegionAlpha = .05f,
            };
        }
    }
}