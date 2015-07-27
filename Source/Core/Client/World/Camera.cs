using Microsoft.Xna.Framework;
using static Microsoft.Xna.Framework.Matrix;
using static Microsoft.Xna.Framework.Vector2;

namespace Bricklayer.Core.Client.World
{
    /// <summary>
    /// A camera object which can focus around a point and be used for drawing at a certain position, zoom, and rotation.
    /// </summary>
    public class Camera
    {
        /// <summary>
        /// The position of the upper left corner of the camera.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set
            {
                position.X = MathHelper.Clamp(value.X, MinBounds.X, MaxBounds.X - size.X);
                position.Y = MathHelper.Clamp(value.Y, MinBounds.Y, MaxBounds.Y - size.Y);
            }
        }

        /// <summary>
        /// The position of the center of the camera.
        /// </summary>
        public Vector2 Origin
        {
            get { return new Vector2(size.X / 2.0f, size.Y / 2.0f); }
            set { Position = new Vector2(value.X - size.X / 2.0f, value.Y - size.Y / 2.0f); }
        }

        /// <summary>
        /// The current zoom factor of the camera.
        /// </summary>
        public float Zoom { get; set; }

        /// <summary>
        /// The rotation, in radians, of the camera.
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// The top (Y) position of the camera.
        /// </summary>
        public float Top => Position.Y;

        /// <summary>
        /// The left (X) position of the camera.
        /// </summary>
        public float Left => Position.X;

        /// <summary>
        /// The bottom bound of the camera (Y + Height).
        /// </summary>
        public float Bottom => Position.Y + size.Y;

        /// <summary>
        /// The right bound of the camera (X + Width).
        /// </summary>
        public float Right => Position.X + size.X;

        /// <summary>
        /// The maximum position the camera can travel to (Using the bottom right position).
        /// </summary>
        public Vector2 MaxBounds { get; set; }

        /// <summary>
        /// The minimum position the camera can travel to.
        /// </summary>
        public Vector2 MinBounds { get; set; }

        private Vector2 size;
        private Vector2 position;

        /// <summary>
        /// Creates a new camera with the specified size.
        /// </summary>
        /// <param name="size">Usually close to the viewport size, defines the size of the camera.</param>
        public Camera(Vector2 size)
        {
            this.size = size;
            Zoom = 1.0f;
        }

        /// <summary>
        /// Get a Matrix that can be used with a spritebatch for drawing objects in the camera.
        /// </summary>
        public Matrix GetViewMatrix(Vector2 parallax)
        {
            return CreateTranslation(new Vector3(-Position * parallax, 0.0f)) *
                   CreateTranslation(new Vector3(-Origin, 0.0f)) *
                   CreateRotationZ(Rotation) *
                   CreateScale(Zoom, Zoom, 1) *
                   CreateTranslation(new Vector3(Origin, 0.0f));
        }

        /// <summary>
        /// Moves the position a certain amount
        /// </summary>
        /// <param name="displacement">Amount to move</param>
        /// <param name="respectRotation">Account for the current rotation</param>
        public void Move(Vector2 displacement, bool respectRotation = false)
        {
            if (respectRotation)
                displacement = Transform(displacement, CreateRotationZ(-Rotation));

            Position += displacement;
        }

        /// <summary>
        /// Sets the position to look at a certain point, automatically factoring in for the center of the camera.
        /// </summary>
        public void LookAt(Vector2 newPosition) => Position = newPosition - Origin;

        /// <summary>
        /// Transforms world coordinates to screen coordinates.
        /// </summary>
        public Vector2 WorldToScreen(Vector2 worldPosition) => Transform(worldPosition, GetViewMatrix(One));

        /// <summary>
        /// Transforms screen coordinates to world coordinates.
        /// </summary>
        public Vector2 ScreenToWorld(Vector2 screenPosition) => Transform(screenPosition, Invert(GetViewMatrix(One)));
    }
}
