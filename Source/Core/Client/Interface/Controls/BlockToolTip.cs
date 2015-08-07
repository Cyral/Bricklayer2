using System;
using Bricklayer.Core.Common.World;
using Microsoft.Xna.Framework;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Controls
{
    /// <summary>
    /// Tooltip for blocks to provide information about them.
    /// </summary>
    public class BlockToolTip : ToolTip
    {
        private static readonly Color catColor = new Color(50, 50, 50);
        public BlockType Block { get; set; }
        // Visible property should be always overriden in this manner.
        // You can set width and height of the tooltip according to it's content.    
        public override bool Visible
        {
            set
            {
                base.Visible = value;
                if (imgIcon == null)
                    return;

                Height = imgIcon.Height + 24;
                Width =
                    (int)
                        Math.Max(Manager.Skin.Fonts["Default9"].Resource.MeasureString(Block.Name).X,
                            Manager.Skin.Fonts["Default6"].Resource.MeasureString(Block.Category.Name).X) +
                    imgIcon.Width + 26;
            }
        }

        private ImageBox imgIcon;
        private Label lblCategory;

        public BlockToolTip(Manager manager) : base(manager)
        {
            TextColor = Color.Black;
        }

        protected override void InitSkin()
        {
            base.InitSkin();

            // We specify what skin this control uses. We use standard tooltip background here.
            Skin = new SkinControl(Manager.Skin.Controls["ToolTip"]);
        }

        public override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            var l = Skin.Layers[0];

            // Render background of the tooltip
            renderer.DrawLayer(this, l, rect);

            // Text is rendered next to the image
            rect = new Rectangle(imgIcon.Right + 4, rect.Top + 2, rect.Width, rect.Height);

            renderer.DrawString(Manager.Skin.Fonts["Default9"].Resource, Block.Name, rect, Color.Black,
                Alignment.TopLeft, true);
            rect.Y += (int) Manager.Skin.Fonts["Default9"].Resource.MeasureString(Text).Y - 2;
            renderer.DrawString(Manager.Skin.Fonts["Default6"].Resource, Block.Category.Name, rect, catColor,
                Alignment.TopLeft, true);
        }

        public void SetBlock(BlockType block)
        {
            Block = block;
            imgIcon = new ImageBox(Manager)
            {
                Width = Tile.FullWidth,
                Height = Tile.FullHeight,
                Left = 8,
                Top = 8
            };
            imgIcon.Init();
            imgIcon.Image = Block.Texture;
            Add(imgIcon);

            Text = block.Name;
        }
    }
}