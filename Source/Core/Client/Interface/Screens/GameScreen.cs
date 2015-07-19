using System.Diagnostics;
using Bricklayer.Core.Client.Interface.Windows;
using Microsoft.Xna.Framework;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Screens
{
    internal class GameScreen : Screen
    {
        protected internal override GameState State => GameState.Game;

        private StatusBar sbStats;

        /// <summary>
        /// Setup the game UI.
        /// </summary>
        public override void Add(ScreenManager screenManager)
        {
            sbStats = new StatusBar(Manager);
            sbStats.Init();
            sbStats.Bottom = Manager.ScreenHeight;
            sbStats.Left = 0;
            sbStats.Width = Manager.ScreenWidth;
            Manager.Add(sbStats);

            base.Add(screenManager);      
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