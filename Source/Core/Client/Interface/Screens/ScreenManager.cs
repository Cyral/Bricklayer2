using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Screens
{
    /// <summary>
    /// Manages transitions and states of game screens
    /// </summary>
    public class ScreenManager
    {
        private const float FadeSpeed = 3f;

        /// <summary>
        /// The current screen playing
        /// </summary>
        public Screen Current { get; set; }

        /// <summary>
        /// The NeoForce UI manager for the Window
        /// </summary>
        public Manager Manager { get; set; }

        /// <summary>
        /// The main window instance (main game layout)
        /// </summary>
        public MainWindow Window { get; set; }

        private float fadeAlpha = 1;
        private Screen fadeTo;
        private FadeState state = FadeState.In;

        public ScreenManager(MainWindow window)
        {
            Window = window;
            Manager = window.Manager;
        }

        /// <summary>
        /// Forces a fade in.
        /// </summary>
        public void FadeIn()
        {
            fadeAlpha = 1;
            state = FadeState.In;
        }

        /// <summary>
        /// Transitions from the current screen to the next.
        /// If the screen is the same screen as the current screeen, it will simply fade in and out without removing and adding the
        /// controls.
        /// </summary>
        public void SwitchScreen(Screen newScreen)
        {
            // Set the new screen and start fading to it
            fadeTo = newScreen;
            state = FadeState.Out;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Current.Draw(spriteBatch, gameTime);
            spriteBatch.DrawRectangle(Window.ControlRect, Color.Black * fadeAlpha);
        }

        public void Update(GameTime gameTime)
        {
            var elapsed = (float) gameTime.ElapsedGameTime.TotalSeconds;

            // Fade the screen in or out, and add/remove its controls accordingly.
            if (state == FadeState.In)
            {
                fadeAlpha = MathHelper.Clamp(fadeAlpha - (elapsed*FadeSpeed), 0, 1);
                if (fadeAlpha <= 0)
                {
                    state = FadeState.Idle;
                }
            }
            else if (state == FadeState.Out)
            {
                fadeAlpha = MathHelper.Clamp(fadeAlpha + (elapsed*FadeSpeed), 0, 1);
                if (fadeAlpha >= 1) // Done fading to black, now set new screen and fade into it
                {
                    // Destory objects from the first screen
                    if (Current != fadeTo)
                    {
                        // Each screen should cleanup itself here.
                        // Controls added through AddControl will also be removed.
                        Current?.Clear();
                        // Automatically remove any dialogs or message boxes left over.
                        for (var i = Manager.Controls.Count() - 1; i >= 0; i--)
                        {
                            var control = Manager.Controls.ElementAt(i);
                            var dialog = control as Dialog;
                            if (dialog != null)
                            {
                                Manager.UnsetModal();
                                dialog.Dispose();
                                Manager.Remove(dialog);
                                i--;
                            }
                        }
                        Current = fadeTo;
                        Current.Setup(this);
                        Window.Client.State = Current.State;
                    }
                    Window.Client.Events.Game.ScreenChanged.Invoke(
                        new EventManager.GameEvents.GameScreenEventArgs(fadeTo, Current));
                    state = FadeState.In;
                }
            }
            foreach (var control in Manager.Controls)
                control.Invalidate();
            Current.Update(gameTime);
        }

        #region Nested type: Enum

        private enum FadeState
        {
            Idle,
            In,
            Out
        }

        #endregion
    }
}