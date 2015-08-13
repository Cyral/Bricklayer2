﻿using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Common.World;
using Microsoft.Xna.Framework;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Controls
{
    /// <summary>
    /// A block item in the inventory panel.
    /// </summary>
    public sealed class InventoryBlockControl : Control
    {
        public BlockType Block
        {
            get { return block; }
            set
            {
                block = value;
                if (Block.IsRenderable)
                    ((BlockToolTip)ToolTip).SetBlock(Block);
            }
        }

        public bool IsSelected { get; internal set; }
        private readonly GameScreen screen;
        private BlockType block;

        public InventoryBlockControl(Manager manager, BlockType block, GameScreen screen) : base(manager)
        {
            this.screen = screen;
            ToolTipType = typeof(BlockToolTip);
            if (block != null)
                Block = block;

            Width = Tile.Width + 2; // Border for selection.
            Height = Tile.Height + 2;
        }

        public override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            if (block != null)
            {
                //Draw block image and selection
                renderer.Draw(screen.Client.Content["gui.blockoutline"], rect.X + 0, rect.Y + 0,
                    IsSelected ? Color.White : Color.Black);
                if (Block.IsRenderable)
                    renderer.Draw(Block.Texture, new Rectangle(rect.X + 1, rect.Y + 1, Tile.Width, Tile.Height),
                        BlockType.SourceRect, Color.White);
                //base.DrawControl(renderer, rect, gameTime);
            }
        }
    }
}