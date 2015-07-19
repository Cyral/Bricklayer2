using Bricklayer.Core.Client.World;
using Microsoft.Xna.Framework;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Screens
{
    internal class GameScreen : Screen
    {
        protected internal override GameState State => GameState.Game;
        internal Level Level => Client.Level;
        private StatusBar sbStats;
        private Label lblStats;

        /// <summary>
        /// Setup the game UI.
        /// </summary>
        public override void Add(ScreenManager screenManager)
        {
            base.Add(screenManager);

            sbStats = new StatusBar(Manager);
            sbStats.Init();
            sbStats.Bottom = Manager.ScreenHeight;
            sbStats.Left = 0;
            sbStats.Width = Manager.ScreenWidth;

            lblStats = new Label(Manager) { Top = 4, Left = 8, Width = Manager.ScreenWidth - 16 };
            lblStats.Init();

            sbStats.Add(lblStats);
            Window.Add(sbStats);
        }

        public override void Update(GameTime gameTime)
        {
            lblStats.Text = "FPS: " + Window.FPS;
            base.Update(gameTime);
        }

        /// <summary>
        /// Remove the game UI.
        /// </summary>
        public override void Remove()
        {
            Manager.Remove(sbStats);
        }
    }
}