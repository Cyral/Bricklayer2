using System.Runtime.InteropServices;
using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Common.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Controls
{
    /// <summary>
    /// Inventory panel used for block selection.
    /// </summary>
    public class InventoryControl : Panel
    {
        private readonly GameScreen screen;
        private static readonly int inventorySlots = 11;

        /// <summary>
        /// Indicates if the inventory is transitioning from open to closed or vice-versa.
        /// </summary>
        public bool SizeChanging { get; internal set; }

        /// <summary>
        /// Indicates if the inventory panel is in the extended/open position.
        /// </summary>
        public bool IsOpen { get; internal set; }

        private readonly ImageBox[] blockImages;
        private readonly ImageBox[] selectImages;
        private readonly StatusBar gradient;
        private readonly int normalHeight = Tile.Height + 7;
        private readonly int normalWidth;
        private float realWidth, realHeight;
        private readonly TabControl tabControl;

        public InventoryControl(GameScreen screen, Manager manager) : base(manager)
        {
            this.screen = screen;
            // Block images.
            blockImages = new ImageBox[inventorySlots];
            selectImages = new ImageBox[inventorySlots];

            normalWidth = ((inventorySlots + 1)*(Tile.Width + 2)) + 8;

            realWidth = Width = normalWidth;
            realHeight = Height = normalHeight;
            Left = Manager.TargetWidth/2 - (Width/2);
            Top = 0;
            Height += 1;

            // Background "gradient" image
            // TODO: Make an actual control. not a statusbar
            gradient = new StatusBar(manager);
            gradient.Init();
            gradient.Width = Width;
            gradient.Height = Height;
            Add(gradient);

            // Create images.
            for (var i = 0; i < blockImages.Length; i++)
            {
                // Block icon image.
                blockImages[i] = new ImageBox(Manager)
                {
                    Top = 4,
                    // Center.
                    Left = 12 + (Width / 2) - (((inventorySlots + 1) * (Tile.Width + 4)) / 2) + ((Tile.Width + 4) * i),
                    Width = Tile.Width,
                    Height = Tile.Width,
                    Text = "",
                    Passive = false
                };
                // Select/border image.
                selectImages[i] = new ImageBox(Manager)
                {
                    Top = 3,
                    Left = 12 + (Width / 2) - (((inventorySlots + 1) * (Tile.Width + 4)) / 2) + ((Tile.Width + 4) * i) - 1,
                    Width = Tile.Width + 2,
                    Height = Tile.Width + 2,
                    Text = "",
                    Alpha = 128f,
                };
                blockImages[i].Init();
                selectImages[i].Init();
                selectImages[i].Image = screen.Client.Content["gui.blockoutline"];
                if (i < BlockType.Blocks.Count && BlockType.Blocks[i].IsRenderable)
                {
                    blockImages[i].Image = BlockType.Blocks[i].Texture;
                    blockImages[i].ToolTipType = typeof (BlockToolTip);
                    ((BlockToolTip)blockImages[i].ToolTip).SetBlock(BlockType.Blocks[i]);
                }
                blockImages[i].SourceRect = BlockType.SourceRect;
                Add(selectImages[i]);
                Add(blockImages[i]);
            }
            SelectBlock(1);

            tabControl = new TabControl(Manager)
            {
                Left = 8,
                Top = 3 + Tile.FullHeight + 3,
            };
            tabControl.Init();
            tabControl.AddPage("Blocks");
            tabControl.AddPage("Interactive");
            tabControl.AddPage("Miscellaneous");
            Add(tabControl);
            tabControl.BringToFront();
        }

        private void SelectBlock(int index)
        {
            foreach (var img in selectImages)
                img.Color = Color.Black;
            selectImages[index].Color = Color.White;
            screen.SelectedBlock = BlockType.Blocks[index];
        }

        protected override void Update(GameTime gameTime)
        {
            // Open or close transition.
            HandleTransition(gameTime);

            HandleInput();

            base.Update(gameTime);
        }

        private void HandleInput()
        {
            if (screen.IsChatOpen())
                return;

            // Open or close inventory.
            if (screen.Client.Input.IsKeyPressed(Keys.E))
            {
                if (!SizeChanging)
                {
                    IsOpen = !IsOpen;
                    SizeChanging = true;
                }
            }

            // Change selected block.
            var key = screen.Client.Input.GetDigitPressed();
            if (key >= 0)
                SelectBlock(key == 0 ? 10 : key);
            else if (screen.Client.Input.IsKeyPressed(Keys.LeftShift))
                SelectBlock(0);
        }

        private void HandleTransition(GameTime gameTime)
        {
            if (!SizeChanging) return;

            var delta = (float) gameTime.ElapsedGameTime.TotalSeconds * 1300f;

            if (IsOpen) // Make inventory bigger as it opens.
            {
                realWidth += delta;
                realHeight += delta / 2;
                Width = (int)realWidth;
                Height = (int)realHeight;
                if (Width >= normalWidth*2)
                {
                    SizeChanging = false;
                }
            }
            else
            {
                realWidth -= delta;
                realHeight -= delta / 2;
                Width = (int)realWidth;
                Height = (int)realHeight;
                if (Width <= normalWidth)
                {
                    SizeChanging = false;
                    Width = normalWidth;
                    Height = normalHeight;
                }
            }

            // Keep inventory box and controls in the center.
            Left = Manager.TargetWidth/2 - (Width/2);
            for (var i = 0; i < blockImages.Length; i++)
            {
                blockImages[i].Left = 12 + (Width/2) - (((inventorySlots + 1)*(Tile.Width + 4))/2) + ((Tile.Width + 4)*i);
                selectImages[i].Left =  12 +(Width / 2) - (((inventorySlots + 1) * (Tile.Width + 4)) / 2) + ((Tile.Width + 4) * i) - 1;
            }

            tabControl.Width = Width - 14;
            tabControl.Height = Height - tabControl.Top - 6;
            gradient.Width = Width;
            gradient.Height = Height;
        }
    }
}