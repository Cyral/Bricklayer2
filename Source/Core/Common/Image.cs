using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bricklayer.Core.Common
{
    /// <summary>
    /// Represents a texture and a source rectangle. In the case of a texture atlas, the source rectangle
    /// represents the area the texture is inside the atlas.
    /// </summary>
    public class Image
    {
        /// <summary>
        /// The texture.
        /// </summary>
        public Texture2D Texture
        {
            get { return texture; }
            set
            {
                texture = value;
                if (SourceRect == Rectangle.Empty)
                    SourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
            }
        }

        /// <summary>
        /// In the case of a texture atlas, the source rectangle represents the area the texture is inside the atlas, otherwise it
        /// is just the width and height of the texture.
        /// </summary>
        public Rectangle SourceRect { get; internal set; }

        private Texture2D texture;

        /// <summary>
        /// In the case of a texture atlas, this is the original (single) texture.
        /// </summary>
        internal Texture2D OriginalTexture { get; set; }

        private Image(Texture2D texture)
        {
            Texture = texture;
        }

        // Make the Image class appear as a Texture2D, as the SourceRect is not always needed (just for texture atlases)
        public static implicit operator Texture2D(Image image) => image.Texture;
        public static implicit operator Image(Texture2D texture) => new Image(texture);
    }
}