using System.Runtime.InteropServices;
using Bricklayer.Core.Common.World;
using Microsoft.Xna.Framework;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Controls
{
    /// <summary>
    /// Inventory panel used for block selection.
    /// </summary>
    public class InventoryControl : Panel
    {
        private static readonly int inventorySlots = 11;

        /// <summary>
        /// Indicates if the inventory is transitioning from open to closed or vice-versa.
        /// </summary>
        public bool SizeChanging { get; internal set; }

        /// <summary>
        /// Indicates if the inventory panel is in the extended/open position.
        /// </summary>
        public bool IsOpen { get; internal set; }

        private readonly ImageBox[] images;
        private readonly int normalHeight = Tile.Height + 16;
        private readonly int normalWidth;

        public InventoryControl(Manager manager) : base(manager)
        {
            // Block images.
            images = new ImageBox[inventorySlots];

            normalWidth = (Tile.Width*images.Length) + ((images.Length + 1)*(Tile.Width + 2));

            Width = normalWidth;
            Height = normalHeight;
            Left = Manager.TargetWidth/2 - (Width/2);
            Top = 8;

            // Create images.
            for (var i = 0; i < images.Length; i++)
            {
                images[i] = new ImageBox(Manager)
                {
                    Top = 4,
                    // Center.
                    Left = (Width / 2) - (((images.Length + 1) * (Tile.Width + 2)) / 2) + ((Tile.Width + 2) * i),
                    Width = Tile.Width,
                    Height = Tile.Width,
                    Text = "",
                    Passive = false
                };
                images[i].Init();
                if (i < BlockType.Blocks.Count && BlockType.Blocks[i].IsRenderable)
                {
                    images[i].Image = BlockType.Blocks[i].Texture;
                    images[i].ToolTipType = typeof (BlockToolTip);
                    ((BlockToolTip)images[i].ToolTip).SetBlock(BlockType.Blocks[i]);
                }
                images[i].SourceRect = BlockType.SourceRect;
                Add(images[i]);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (SizeChanging)
            {
                var delta = (float) gameTime.ElapsedGameTime.TotalSeconds*20;
                if (IsOpen) // Make inventory bigger as it opens.
                {
                    // 25 is added/subtracted to make the lerp faster, as it will become very slow at the end.
                    Width =
                        (int) MathHelper.Lerp(Width, (normalWidth*1.5f) + 25, delta);
                    Height =
                        (int) MathHelper.Lerp(Height, (normalHeight*2) + 25, delta);
                    if (Width >= normalWidth*1.5)
                        SizeChanging = false;
                }
                else
                {
                    Width =
                        (int) MathHelper.Lerp(Width, normalWidth - 25, delta);
                    Height =
                        (int) MathHelper.Lerp(Height, normalHeight, delta);
                    if (Width <= normalWidth)
                    {
                        SizeChanging = false;
                        Width = normalWidth;
                        Height = normalHeight;
                    }
                }

                // Keep inventory box and controls in the center.
                Left = Manager.TargetWidth/2 - (Width/2);
                for (var i = 0; i < images.Length; i++)
                    images[i].Left = (Width/2) - (((images.Length + 1) * (Tile.Width + 2)) / 2) + ((Tile.Width + 2) * i);
            }

            base.Update(gameTime);
        }
    }
}