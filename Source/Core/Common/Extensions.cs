using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Bricklayer.Core.Common
{
    public static class MouseStateExtensions
    {
        /// <summary>
        /// Gets the position of the cursor as a Point value.
        /// </summary>
        public static Point GetPositionPoint(this MouseState ms)
        {
            return new Point(ms.X, ms.Y);
        }

        /// <summary>
        /// Gets the position of the cursor as a Vector2 value.
        /// </summary>
        public static Vector2 GetPositionVector(this MouseState ms)
        {
            return new Vector2(ms.X, ms.Y);
        }
    }

    public static class HashExtensions
    {
        public const int Start = 17;

        /// <summary>
        /// Creates a hash between this object and other object.
        /// </summary>
        public static int Hash<T>(this int hash, T obj)
        {
            var c = EqualityComparer<T>.Default;
            var h = c.Equals(obj, default(T)) ? 0 : obj.GetHashCode();
            return unchecked((hash * 31) + h);
        }
    }

    public static class IOExtensions
    {
        /// <summary>
        /// Writes an XNA color value to a binary writer.
        /// </summary>
        public static void Write(this BinaryWriter writer, Color color)
        {
            writer.Write(color.R);
            writer.Write(color.G);
            writer.Write(color.B);
            writer.Write(color.A);
        }

        /// <summary>
        /// Reads an XNA color value from a binary reader.
        /// </summary>
        public static Color ReadColor(this BinaryReader reader)
        {
            Color color = Color.White;
            color.R = reader.ReadByte();
            color.G = reader.ReadByte();
            color.B = reader.ReadByte();
            color.A = reader.ReadByte();
            return color;
        }
    }

    public static class RectangleExtensions
    {
        /// <summary>
        /// Constructs a rectangle from a Point's X and Y position
        /// </summary>
        public static Rectangle FromPoint(Point p, int width = 0, int height = 0)
        {
            return new Rectangle(p.X, p.Y, width, height);
        }

        /// <summary>
        /// Constructs a rectangle from a Vector2's X and Y position
        /// </summary>
        public static Rectangle FromVector2(Vector2 v, int width = 0, int height = 0)
        {
            return new Rectangle((int)Math.Round(v.X), (int)Math.Round(v.Y), width, height);
        }
    }

    public static class Vector2Extensions
    {
        /// <summary>
        /// Creates a new Vector2 from a Point.
        /// </summary>
        public static Vector2 FromPoint(Point p)
        {
            return new Vector2(p.X, p.Y);
        }

        /// <summary>
        /// Transforms a Vector2 into a Point.
        /// </summary>
        public static Point ToPoint(this Vector2 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }

        /// <summary>
        /// Rounds a vector's X and Y values to the nearest integer.
        /// </summary>
        /// <param name="vector">Vector2 to round.</param>
        /// <returns>A Vector2 with integer X and Y values.</returns>
        public static Vector2 Round(this Vector2 vector)
        {
            vector.X = (int)Math.Round(vector.X);
            vector.Y = (int)Math.Round(vector.Y);
            return vector;
        }

        /// <summary>
        /// Rotates a vector around a center point.
        /// </summary>
        /// <param name="point">Vector position to rotate.</param>
        /// <param name="origin">Point to rotate around.</param>
        /// <param name="rotation">Rotation to be applied.</param>
        /// <returns>A Vector2 that has been rotated around a center point.</returns>
        public static Vector2 RotateAboutOrigin(this Vector2 point, Vector2 origin, float rotation)
        {
            return Vector2.Transform(point - origin, Matrix.CreateRotationZ(rotation)) + origin;
        }
    }

    public static class PointExtensions
    {
        /// <summary>
        /// Creates a new Point from a Vector2
        /// </summary>
        public static Point FromPoint(Vector2 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }

        /// <summary>
        /// Transforms a Point into a Vector2
        /// </summary>
        public static Vector2 ToVector2(this Point p)
        {
            return new Vector2(p.X, p.Y);
        }
    }
}
