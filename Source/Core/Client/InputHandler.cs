using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Bricklayer.Core.Client
{
    /// <summary>
    /// Gets current mouse and keyboard states and provides easy access to them
    /// </summary>
    public class InputHandler
    {
        public MouseState CurrentMouseState { get; set; }
        public MouseState PreviousMouseState { get; set; }

        public KeyboardState CurrentKeyboardState { get; set; }
        public KeyboardState PreviousKeyboardState { get; set; }

        /// <summary>
        /// Returns the position of the mouse
        /// </summary>
        public Vector2 MousePosition => CurrentMouseState.GetPositionVector();

        /// <summary>
        /// Returns the position of the mouse, in world/grid coordinates
        /// </summary>
        public Point MouseGridPosition
        {
            get
            {
                var position = CurrentMouseState.GetPositionPoint();
                return new Point(position.X / Common.World.Tile.Width, Common.World.Tile.Height);
            }
        }

        /// <summary>
        /// Creats a new InputHandler
        /// </summary>
        internal InputHandler()
        {
            //Initialize
        }

        /// <summary>
        /// Updates the input states
        /// </summary>
        public void Update()
        {
            PreviousKeyboardState = CurrentKeyboardState;
            PreviousMouseState = CurrentMouseState;

            CurrentMouseState = Mouse.GetState();
            CurrentKeyboardState = Keyboard.GetState();
        }

        /// <summary>
        /// Checks if any of the keys specified are pressed/toggled
        /// </summary>
        public bool AnyKeysPressed(params Keys[] keys)
        {
            return keys.Any(k => CurrentKeyboardState.IsKeyUp(k) && PreviousKeyboardState.IsKeyDown(k));
        }

        /// <summary>
        /// Checks if any of the keys specified are down
        /// </summary>
        public bool AnyKeysDown(params Keys[] keys)
        {
            return keys.Any(k => CurrentKeyboardState.IsKeyDown(k));
        }

        /// <summary>
        /// Checks if any of the keys specified were down last frame
        /// </summary>
        public bool WasAnyKeysDown(params Keys[] keys)
        {
            return keys.Any(k => PreviousKeyboardState.IsKeyDown(k));
        }
        /// <summary>
        /// Checks if all of the keys specified were up last frame
        /// </summary>
        public bool WasAllKeysUp(params Keys[] keys)
        {
            return keys.All(k => PreviousKeyboardState.IsKeyUp(k));
        }
        /// <summary>
        /// Checks if any of the keys specified were up the last frame
        /// </summary>
        public bool WereAnyKeysUp(params Keys[] keys)
        {
            return keys.Any(k => PreviousKeyboardState.IsKeyUp(k));
        }

        /// <summary>
        /// Checks if all of the keys specified are down
        /// </summary>
        public bool AllKeysDown(params Keys[] keys)
        {
            return keys.All(k => CurrentKeyboardState.IsKeyDown(k));
        }

        /// <summary>
        /// Checks if a given key is currently down
        /// </summary>
        public bool IsKeyDown(Keys key)
        {
            return CurrentKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if a given key is currently up
        /// </summary>
        public bool IsKeyUp(Keys key)
        {
            return CurrentKeyboardState.IsKeyUp(key);
        }

        /// <summary>
        /// Checks if a given key is was down last frame
        /// </summary>
        public bool WasKeyDown(Keys key)
        {
            return PreviousKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if a given key is was up last frame
        /// </summary>
        public bool WasKeyUp(Keys key)
        {
            return PreviousKeyboardState.IsKeyUp(key);
        }

        /// <summary>
        /// Checks if a given key is currently being pressed (Was not pressed last state, but now is)
        /// </summary>
        public bool IsKeyPressed(Keys key)
        {
            return CurrentKeyboardState.IsKeyDown(key) && PreviousKeyboardState.IsKeyUp(key);
        }

        /// <summary>
        /// Checks if a given key has been toggled (Was pressed last state, but now isn't)
        /// </summary>
        public bool WasKeyPressed(Keys key)
        {
            return CurrentKeyboardState.IsKeyUp(key) && PreviousKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if the right button is being held down
        /// </summary>
        public bool IsRightDown()
        {
            return CurrentMouseState.RightButton == ButtonState.Pressed;
        }


        /// <summary>
        /// Checks if the left button is being held down
        /// </summary>
        public bool IsLeftDown()
        {
            return CurrentMouseState.LeftButton == ButtonState.Pressed;
        }

        /// <summary>
        /// Checks if the right button is currently up
        /// </summary>
        public bool IsRightUp()
        {
            return CurrentMouseState.RightButton == ButtonState.Released;
        }


        /// <summary>
        /// Checks if the left button is currently up
        /// </summary>
        public bool IsLeftUp()
        {
            return CurrentMouseState.LeftButton == ButtonState.Released;
        }

        /// <summary>
        /// Checks if the left button is being clicked (Currently is down, wasn't last frame)
        /// </summary>
        /// <returns></returns>
        public bool IsLeftClicked()
        {
            return CurrentMouseState.LeftButton == ButtonState.Pressed && PreviousMouseState.LeftButton == ButtonState.Released;
        }

        /// <summary>
        /// Checks if the right button is being clicked (Currently is down, wasn't last frame)
        /// </summary>
        /// <returns></returns>
        public bool IsRightClicked()
        {
            return CurrentMouseState.RightButton == ButtonState.Pressed && PreviousMouseState.RightButton == ButtonState.Released;
        }

        /// <summary>
        /// Gets the current number key pressed, returns -1 if none
        /// </summary>
        public int GetDigitPressed()
        {
            var key = -1;

            if (IsKeyPressed(Keys.D1)) key = 0;
            else if (IsKeyPressed(Keys.D2)) key = 1;
            else if (IsKeyPressed(Keys.D3)) key = 2;
            else if (IsKeyPressed(Keys.D4)) key = 3;
            else if (IsKeyPressed(Keys.D5)) key = 4;
            else if (IsKeyPressed(Keys.D6)) key = 5;
            else if (IsKeyPressed(Keys.D7)) key = 6;
            else if (IsKeyPressed(Keys.D8)) key = 7;
            else if (IsKeyPressed(Keys.D9)) key = 8;
            else if (IsKeyPressed(Keys.D0)) key = 9;

            return key;
        }
    }
}
