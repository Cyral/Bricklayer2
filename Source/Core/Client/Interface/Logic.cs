using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface
{
    /// <summary>
    /// Handles the logical updating of UI elements
    /// </summary>
    public sealed partial class MainWindow
    {
        private static readonly TimeSpan second = TimeSpan.FromSeconds(1);

        /// <summary>
        /// The current FPS.
        /// </summary>
        internal int FPS { get; private set; }

        private TimeSpan elapsedTime = TimeSpan.Zero;
        private int totalFrames;

        protected override void Update(GameTime gameTime)
        {
            if (Client.IsActive) //Pause input updates if window isn't active.
                Client.Input.Update();

            // Update current screen
            ScreenManager.Update(gameTime);

            base.Update(gameTime);
        }

        public new void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // Calculate FPS
            elapsedTime += gameTime.ElapsedGameTime;
            if (elapsedTime > second)
            {
                elapsedTime -= second;
                FPS = totalFrames;
                totalFrames = 0;
            }
            totalFrames++;

            // Draw current screen.
            ScreenManager.Draw(spriteBatch, gameTime);
        }

        /// <summary>
        /// Returns true if the mouse position is over a UI control.
        /// </summary>
        public bool IsMouseOverUI()
        {
            var pos = Client.Input.MousePosition;
            var rect = new Rectangle(pos.X, pos.Y, 1, 1);
            return
                Manager.Controls.First()
                    .Controls.First()
                    .Controls.Any(
                        control => control.Visible && !control.Passive && control.AbsoluteRect.Intersects(rect));
        }
    }
}