﻿using System;
using System.Linq;
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
        private static readonly int inventorySlots = 11;

        /// <summary>
        /// Indicates if the inventory is trazsitioning from open to closed or vice-versa.
        /// </summary>
        public bool SizeChanging { get; internal set; }

        /// <summary>
        /// Indicates if the inventory panel is in the extended/open position.
        /// </summary>
        public bool IsOpen { get; internal set; }

        private readonly InventoryBlockControl[] blockControls;
        private readonly InventoryBlockControl cursorBlock;
        private readonly StatusBar gradient;
        private readonly int normalHeight = Tile.Height + 7;
        private readonly int normalWidth, extendedWidth, extendedHeight;
        private readonly InventoryBlockControl[] packBlockControls;
        private readonly GameScreen screen;
        private readonly TabControl tabControl;
        private BlockType draggingBlock;
        private bool isDragging;
        private float realWidth, realHeight;
        private int lastBlock;

        public InventoryControl(GameScreen screen, Manager manager) : base(manager)
        {
            this.screen = screen;

            blockControls = new InventoryBlockControl[inventorySlots];
            packBlockControls = new InventoryBlockControl[BlockType.Blocks.Count];
            var packLabels = new Label[BlockPack.Packs.Count];
            
            // Find width of normal inventory size, and set an expanded target size.
            normalWidth = ((inventorySlots + 1)*(Tile.Width + 2)) + 8;
            extendedWidth = (int) (normalWidth*2.5f);

            realWidth = Width = normalWidth;
            realHeight = Height = normalHeight;
            Left = Manager.TargetWidth/2 - (Width/2);
            Top = 0;
            Height += 2;

            // Background "gradient" image
            // TODO: Make an actual control. not a statusbar
            gradient = new StatusBar(manager);
            gradient.Init();
            gradient.Width = Width;
            gradient.Height = Height;
            Add(gradient);

            // Block image on the cursor when a block is dragged.
            cursorBlock = new InventoryBlockControl(Manager, null, screen) {Visible = false};
            cursorBlock.Init();
            cursorBlock.Passive = true;
            Manager.Add(cursorBlock);

            // Create main blocks.
            for (var i = 0; i < Math.Min(blockControls.Length, BlockType.Blocks.Count); i++)
            {
                // Block icon image.
                blockControls[i] = new InventoryBlockControl(Manager, BlockType.Blocks[i], screen)
                {
                    Top = 3,
                    Left = 11 + (Width/2) - (((inventorySlots + 1)*(Tile.Width + 4))/2) + ((Tile.Width + 4)*i)
                    // Center.
                };
                blockControls[i].Click += (sender, args) => SelectBlock(((InventoryBlockControl) sender).Block);

                blockControls[i].Init();
                Add(blockControls[i]);
            }

            tabControl = new TabControl(Manager)
            {
                Left = 8,
                Top = 3 + Tile.FullHeight + 3
            };
            tabControl.Init();
            // For each type of block pack (blocks, interactive, etc.), add a tab.
            foreach (var category in PackCategory.Categories)
                tabControl.AddPage(category.Name);
            Add(tabControl);
            tabControl.BringToFront();

            // For each block pack, add it to the appropriate tab.
            // Keep spacing in mind and go to a new line when space runs out.
            var y = 0;
            int packIndex = 0, blockIndex = 0;
            int maxY = 0;
            for (var catIndex = 0; catIndex < PackCategory.Categories.Count; catIndex++)
            {
                var x = 8;
                y = 8;
                var category = PackCategory.Categories[catIndex];
                var page = tabControl.TabPages[catIndex];
                foreach (var pack in category.Packs)
                {
                    // Calculate width this block pack will use.
                    // TODO: Support for extremely long block packs that may need multiple lines.
                    var width =
                        (int) Manager.Skin.Fonts[FontSize.Default8.ToString()].Resource.MeasureString(pack.Name + ":").X;
                    var futureWidth = width + (pack.Blocks.Count()*(Tile.Width + 4));

                    // If more than the tab control size, move down.
                    if (x + futureWidth > extendedWidth - 32)
                    {
                        y += Tile.Height + 8;
                        x = 8;
                    }

                    // Add label for pack.
                    packLabels[packIndex] = new Label(Manager)
                    {
                        Top = y + 2,
                        Left = x,
                        Text = pack.Name + ":",
                        TextColor = Color.White,
                        Width = width
                    };
                    packLabels[packIndex].Init();
                    page.Add(packLabels[packIndex]);
                    x += width + 8; // Add width of label + padding to width.

                    // For each block in this pack, add a block icon.
                    foreach (var block in pack.Blocks)
                    {
                        packBlockControls[blockIndex] = new InventoryBlockControl(Manager, block, screen)
                        {
                            Top = y,
                            Left = x
                        };
                        packBlockControls[blockIndex].Init();
                        packBlockControls[blockIndex].MouseMove += (sender, args) =>
                        {
                            if (args.Button == MouseButton.Left)
                            {
                                DragBlock(((InventoryBlockControl) sender).Block);
                            }
                        };
                        packBlockControls[blockIndex].Click +=
                            (sender, args) => SelectBlock(((InventoryBlockControl) sender).Block);
                        page.Add(packBlockControls[blockIndex]);
                        x += packBlockControls[blockIndex].Width + 2;
                        blockIndex++;
                    }
                    x += 8;
                    packIndex++;
                }
                maxY = Math.Max(maxY, y); // Record the highest Y value for each category, so the extended height can be set.
            }
            extendedHeight = tabControl.Top + maxY + 40 + Tile.Height;
            SelectBlock(0);
        }

        private void DragBlock(BlockType block)
        {
            isDragging = true;
            draggingBlock = block;
            cursorBlock.Block = block;
            cursorBlock.Visible = true;
            cursorBlock.Passive = true;
        }

        /// <summary>
        /// Select the specified block as the current block and highlight all occurences of it in the inventory.
        /// </summary>
        /// <param name="block"></param>
        private void SelectBlock(BlockType block)
        {
            // Set all occurences of the block to selected status.
            foreach (var ctrl in blockControls)
                ctrl.IsSelected = ctrl.Block == block;
            foreach (var ctrl in packBlockControls)
                ctrl.IsSelected = ctrl.Block == block;

            // Record the last block set, so pressing shift can quickly switch.
            for (var i = 1; i < blockControls.Length; i++)
            {
                if (blockControls[i].Block.ID == block.ID)
                {
                    lastBlock = i;
                    break;
                }
            }
            screen.SelectedBlock = block;
        }

        /// <summary>
        /// Select the block at the specified index of the inventory bar.
        /// </summary>
        private void SelectBlock(int index)
        {
            SelectBlock(blockControls[index].Block);
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

            // Handle dragging of blocks.
            if (isDragging)
            {
                // Update block position to match cursor.
                cursorBlock.Left = screen.Client.Input.MousePosition.X - Tile.Width/2;
                cursorBlock.Top = screen.Client.Input.MousePosition.Y - Tile.Height/2;
            }
            var rect = new Rectangle(screen.Client.Input.MousePosition.X, screen.Client.Input.MousePosition.Y, 1, 1);
            if (isDragging)
            {
                // If clicked on a main block, replace it, else, stop dragging.
                if (screen.Client.Input.IsLeftUp())
                {
                    foreach (var ctrl in blockControls.Where(ctrl => ctrl.AbsoluteRect.Intersects(rect)))
                    {
                        ctrl.Block = draggingBlock;
                        SelectBlock(draggingBlock);
                    }
                    isDragging = false;
                    cursorBlock.Visible = false;
                }
            }

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
            // 0-9 controls selected block, holding shift selects the first (empty) block.
            var key = screen.Client.Input.GetDigitPressed();
            if (key >= 0)
                SelectBlock(key == 0 ? 10 : key);
            else if (screen.Client.Input.IsKeyPressed(Keys.LeftShift))
                SelectBlock(0);
            else if (screen.Client.Input.WasKeyPressed(Keys.LeftShift))
                SelectBlock(lastBlock);
        }

        /// <summary>
        /// Update transition effect of inventory opening and closing.
        /// </summary>
        private void HandleTransition(GameTime gameTime)
        {
            if (!SizeChanging) return;

            var delta = (float) gameTime.ElapsedGameTime.TotalSeconds*1700f;

            if (IsOpen) // Make inventory bigger as it opens.
            {
                if (Width >= extendedWidth)
                {
                    Width = extendedWidth;
                }
                else
                {
                    realWidth += delta;
                    Width = (int) realWidth;
                }

                if (Height >= extendedHeight)
                {
                    Height = extendedHeight;
                }
                else
                {
                    realHeight += delta/(extendedWidth/(float) extendedHeight)*1.5f; // Use ratio for height.
                    Height = (int) realHeight;
                }

                if (Width >= extendedWidth && Height >= extendedHeight)
                {
                    SizeChanging = false;
                }
            }
            else
            {
                if (Width <= normalWidth)
                {
                    Width = normalWidth;
                }
                else
                {
                    realWidth -= delta;
                    Width = (int) realWidth;
                }

                if (Height <= normalHeight)
                {
                    Height = normalHeight;
                }
                else
                {
                    realHeight -= delta/(extendedWidth/(float) extendedHeight)*1.5f; // Use ratio for height.
                    Height = (int) realHeight;
                }

                if (Width <= normalWidth && Height <= normalHeight)
                {
                    SizeChanging = false;
                }
            }

            // Keep inventory box and controls in the center.
            Left = Manager.TargetWidth/2 - (Width/2);
            for (var i = 0; i < blockControls.Length; i++)
            {
                blockControls[i].Left = 11 + (Width/2) - (((inventorySlots + 1)*(Tile.Width + 4))/2) +
                                        ((Tile.Width + 4)*i);
            }

            tabControl.Width = Width - 14;
            tabControl.Height = Height - tabControl.Top - 6;
            gradient.Width = Width;
            gradient.Height = Height;
        }
    }
}