using System;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoForce.Controls;

namespace Bricklayer.Client.Interface
{
    /// <summary>
    /// Handles the logical updating of UI elements
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        internal int TotalFrames { get; set; }
        internal int FPS { get; private set; }
        internal TimeSpan ElapsedTime { get; private set; }

        protected override void Update(GameTime gameTime)
        {
            ScreenManager.Update(gameTime);
            base.Update(gameTime);
        }
    }
}
    