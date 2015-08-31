using System;
using System.Linq;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Level = Bricklayer.Core.Client.World.Level;

namespace Bricklayer.Core.Client
{
    /// <summary>
    /// Gets current mouse and keyboard states and provides easy access to them.
    /// </summary>
    public class InputHandler
    {
        public MouseState CurrentMouseState { get; set; }
        public MouseState PreviousMouseState { get; set; }
        public KeyboardState CurrentKeyboardState { get; set; }
        public KeyboardState PreviousKeyboardState { get; set; }

        /// <summary>
        /// Returns the position of the mouse.
        /// </summary>
        public Point MousePosition => CurrentMouseState.GetPositionPoint();

        /// <summary>
        /// Returns the position of the mouse, in (foreground) world/grid coordinates.
        /// </summary>
        public Point MouseGridPosition => GetMouseGridPositon(Layer.Foreground);

        internal Level Level { get; set; }

        /// <summary>
        /// Creats a new InputHandler.
        /// </summary>
        internal InputHandler()
        {
            // Initialize
        }

        /// <summary>
        /// Returns the position of the mouse in world/grid coordinates, transformed by the level camera. This differs between each layer as the block perspective
        /// causes the foreground and background layers to be slightly off.
        /// </summary>
        public Point GetMouseGridPositon(Layer layer)
        {
            var pos = Level?.Camera?.ScreenToWorld(CurrentMouseState.GetPositionVector()).ToPoint() ?? CurrentMouseState.GetPositionPoint();
            if (layer.HasFlag(Layer.Foreground))
            {
                return new Point((int) Math.Floor(pos.X / (float) Tile.Width),
                    (int) Math.Floor((pos.Y - (Tile.FullHeight - Tile.Height)) / (float) Tile.Height));
            }
            return new Point((int) Math.Floor((pos.X - (Tile.FullWidth - Tile.Width)) / (float) Tile.Width),
                    (int) Math.Floor(pos.Y / (float) Tile.Height));
        }

        /// <summary>
        /// Updates the input states.
        /// </summary>
        public void Update()
        {
            PreviousKeyboardState = CurrentKeyboardState;
            PreviousMouseState = CurrentMouseState;

            CurrentMouseState = Mouse.GetState();
            CurrentKeyboardState = Keyboard.GetState();
        }

        /// <summary>
        /// Checks if any of the keys specified are pressed/toggled.
        /// </summary>
        public bool AnyKeysPressed(params Keys[] keys)
        {
            return keys.Any(k => CurrentKeyboardState.IsKeyUp(k) && PreviousKeyboardState.IsKeyDown(k));
        }

        /// <summary>
        /// Checks if any of the keys specified are down.
        /// </summary>
        public bool AnyKeysDown(params Keys[] keys)
        {
            return keys.Any(k => CurrentKeyboardState.IsKeyDown(k));
        }

        /// <summary>
        /// Checks if any of the keys specified were down last frame.
        /// </summary>
        public bool WasAnyKeysDown(params Keys[] keys)
        {
            return keys.Any(k => PreviousKeyboardState.IsKeyDown(k));
        }

        /// <summary>
        /// Checks if all of the keys specified were up last frame.
        /// </summary>
        public bool WasAllKeysUp(params Keys[] keys)
        {
            return keys.All(k => PreviousKeyboardState.IsKeyUp(k));
        }

        /// <summary>
        /// Checks if any of the keys specified were up the last frame.
        /// </summary>
        public bool WereAnyKeysUp(params Keys[] keys)
        {
            return keys.Any(k => PreviousKeyboardState.IsKeyUp(k));
        }

        /// <summary>
        /// Checks if all of the keys specified are down.
        /// </summary>
        public bool AllKeysDown(params Keys[] keys)
        {
            return keys.All(k => CurrentKeyboardState.IsKeyDown(k));
        }

        /// <summary>
        /// Checks if a given key is currently down.
        /// </summary>
        public bool IsKeyDown(Keys key)
        {
            return CurrentKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if a given key is currently up.
        /// </summary>
        public bool IsKeyUp(Keys key)
        {
            return CurrentKeyboardState.IsKeyUp(key);
        }

        /// <summary>
        /// Checks if a given key is was down last frame.
        /// </summary>
        public bool WasKeyDown(Keys key)
        {
            return PreviousKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if a given key is was up last frame.
        /// </summary>
        public bool WasKeyUp(Keys key)
        {
            return PreviousKeyboardState.IsKeyUp(key);
        }

        /// <summary>
        /// Checks if a given key is currently being pressed (Was not pressed last state, but now is).
        /// </summary>
        public bool IsKeyPressed(Keys key)
        {
            return CurrentKeyboardState.IsKeyDown(key) && PreviousKeyboardState.IsKeyUp(key);
        }

        /// <summary>
        /// Checks if a given key has been toggled (Was pressed last state, but now isn't).
        /// </summary>
        public bool WasKeyPressed(Keys key)
        {
            return CurrentKeyboardState.IsKeyUp(key) && PreviousKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Checks if the right button is being held down.
        /// </summary>
        public bool IsRightDown()
        {
            return CurrentMouseState.RightButton == ButtonState.Pressed;
        }

        /// <summary>
        /// Checks if the left button is being held down.
        /// </summary>
        public bool IsLeftDown()
        {
            return CurrentMouseState.LeftButton == ButtonState.Pressed;
        }

        /// <summary>
        /// Checks if the right button is currently up.
        /// </summary>
        public bool IsRightUp()
        {
            return CurrentMouseState.RightButton == ButtonState.Released;
        }

        /// <summary>
        /// Checks if the left button is currently up.
        /// </summary>
        public bool IsLeftUp()
        {
            return CurrentMouseState.LeftButton == ButtonState.Released;
        }

        /// <summary>
        /// Checks if the left button is being clicked (Currently is down, wasn't last frame).
        /// </summary>
        /// <returns></returns>
        public bool IsLeftClicked()
        {
            return CurrentMouseState.LeftButton == ButtonState.Pressed &&
                   PreviousMouseState.LeftButton == ButtonState.Released;
        }

        /// <summary>
        /// Checks if the right button is being clicked (Currently is down, wasn't last frame).
        /// </summary>
        /// <returns></returns>
        public bool IsRightClicked()
        {
            return CurrentMouseState.RightButton == ButtonState.Pressed &&
                   PreviousMouseState.RightButton == ButtonState.Released;
        }

        /// <summary>
        /// Gets the current number key pressed, returns -1 if none.
        /// </summary>
        public int GetDigitPressed()
        {
            var pressedDigitKeys = CurrentKeyboardState.GetPressedKeys()
                // Select those that are within D0 and D9
                .Where(x => x >= Keys.D0 && x <= Keys.D9)
                // Select those that weren't pressed last time
                .Where(x => !PreviousKeyboardState.GetPressedKeys().Contains(x)).ToArray();

            if (pressedDigitKeys.Length == 0)
                return -1;

            // D0 is 9, D1 is 0, D2 is 1, and so on...
            return pressedDigitKeys[0] == Keys.D0 ? 0 : ((int) pressedDigitKeys[0]) - 48;
        }

        /// <summary>
        /// Returns a direction from WASD or arrow keys.
        /// </summary>
        public Vector2 GetDirection()
        {
            int x = 0, y = 0;
            if (IsKeyDown(Keys.A) || IsKeyDown(Keys.Left))
                x -= 1;
            if (IsKeyDown(Keys.D) || IsKeyDown(Keys.Right))
                x += 1;
            if (IsKeyDown(Keys.W) || IsKeyDown(Keys.Up))
                y -= 1;
            if (IsKeyDown(Keys.S) || IsKeyDown(Keys.Down))
                y += 1;
            var vector = new Vector2(x, y);
            if (vector == Vector2.Zero) return Vector2.Zero;
            vector.Normalize();
            return vector;
        }
    }
}