using System;
using Bricklayer.Client.Interface;
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
        private const float FadeSpeed = 2f;

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

        private readonly ImageBox fadeImage;
        private readonly Texture2D fadeTexture;
        private float fadeAlpha = 1;
        private Screen fadeTo;
        private FadeState state = FadeState.In;

        public ScreenManager(MainWindow window)
        {
            Window = window;
            Manager = window.Manager;

            //Setup a solid black image to use for fading
            fadeTexture = new Texture2D(Manager.GraphicsDevice, 1, 1);
            fadeTexture.SetData(new[] {Color.Black});

            fadeImage = new ImageBox(Manager)
            {
                Passive = true,
                Left = 0,
                Top = 0,
                Width = Window.Width,
                Height = Window.Height,
                StayOnTop = true,
                Image = fadeTexture,
                SizeMode = SizeMode.Stretched
            };
            fadeImage.Init();
            fadeImage.Image = fadeTexture;
            fadeImage.Alpha = 0;
            fadeImage.Color = Color.White * fadeImage.Alpha;
            window.Add(fadeImage);
            fadeImage.BringToFront();
        }

        /// <summary>
        /// Forces a fade in.
        /// </summary>
        public void FadeIn()
        {
            fadeAlpha = 1;
            state = FadeState.In;
            fadeImage.BringToFront();
        }

        /// <summary>
        /// Transitions from the current screen to the next.
        /// If the screen is the same screen as the current screeen, it will simply fade in and out without removing and adding the controls.
        /// </summary>
        public void SwitchScreen(Screen newScreen)
        {
            //Set the new screen and start fading to it
            fadeTo = newScreen;
            state = FadeState.Out;
            fadeImage.BringToFront();
        }

        public void Update(GameTime gameTime)
        {
            var elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Fade the screen in or out, and add/remove its controls accordingly.
            if (state == FadeState.In)
            {
                fadeAlpha = MathHelper.Clamp(fadeAlpha - (elapsed * FadeSpeed), 0, 1);
                fadeImage.Alpha = fadeAlpha;
                fadeImage.Color = Color.White * fadeImage.Alpha;
                if (fadeAlpha <= 0)
                {
                    state = FadeState.Idle;
                }
            }
            else if (state == FadeState.Out)
            {
                fadeAlpha = MathHelper.Clamp(fadeAlpha + (elapsed * FadeSpeed), 0, 1);
                fadeImage.Alpha = fadeAlpha;
                fadeImage.Color = Color.White * fadeImage.Alpha;
                if (fadeAlpha >= 1) //Done fading to black, now set new screen and fade into it
                {
                    Window.Client.Events.Game.ScreenChanged.Invoke(new EventManager.GameEvents.GameScreenEventArgs(fadeTo, Current));
                    //Destory objects from the first screen
                    if (Current != fadeTo)
                    {
                        Current?.Remove();
                        Current = fadeTo;
                        Current.Add(this);
                        Window.Client.State = Current.State;
                    }
                    state = FadeState.In;
                    fadeImage.BringToFront();
                }
            }
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