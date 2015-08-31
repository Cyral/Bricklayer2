using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bricklayer.Core.Client
{
    /// <summary>
    /// Client specific extension methods.
    /// </summary>
    public static class Extensions
    {
        private static readonly Random random;
        private static Texture2D pixel;

        static Extensions()
        {
            random = new Random();
        }

        internal static void Setup(GraphicsDevice graphics)
        {
            pixel = new Texture2D(graphics, 1, 1);
            pixel.SetData(new[] { Color.White });
        }

        /// <summary>
        /// Checks to see if any of the object(s) in the collection are equal to the source value.
        /// </summary>
        /// <typeparam name="T">The type of object used.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="list">The possible objects to equal.</param>
        /// <returns>
        /// True of any object(s) are equal to the source, false otherwise.
        /// </returns>
        public static bool EqualsAny<T>(this T source, params T[] list)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return list.Contains(source);
        }

        /// <summary>
        /// Randomizes the order of elements in a collection.
        /// </summary>
        /// <typeparam name="T">Type of the objects in the collection.</typeparam>
        /// <param name="list">Collection to shuffle.</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            var count = list.Count;
            while (count > 1)
            {
                --count;
                var index = random.Next(count + 1);
                var obj = list[index];
                list[index] = list[count];
                list[count] = obj;
            }
        }

        /// <summary>
        /// Draws a solid color rectangle.
        /// </summary>
        public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle targetRectangle, Color color)
        {
            spriteBatch.Draw(pixel, targetRectangle, color);
        }

        /// <summary>
        /// Draws a solid color rectangle.
        /// </summary>
        public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle targetRectangle)
        {
            spriteBatch.Draw(pixel, targetRectangle, Color.White);
        }

        /// <summary>
        /// Returns the average color of a texture. (Or part of it)
        /// </summary>
        public static Color AverageColor(this Texture2D texture, Rectangle source)
        {
            var data = new Color[texture.Width * texture.Height];
            int r = 0, g = 0, b = 0, amount = 0;
            texture.GetData(data);
            for (var c = 0; c < data.Length; c++) //Foreach colored pixel; Get RGB values
            {
                var x = c % texture.Width;
                var y = (c - x) / texture.Width;

                if (source.Contains(new Point(x, y)))
                {
                    var color = data[c];
                    if (color.A > 0)
                    {
                        r += color.R;
                        g += color.G;
                        b += color.B;
                        amount++;
                    }
                }
            }

            return amount > 0 ? new Color(r / amount, g / amount, b / amount) : Color.Transparent;
        }
    }

    /// <summary>
    /// Encrypts and decrypts string for saving password configs for example
    /// </summary>
    /// <see cref="https://stackoverflow.com/questions/8871337/how-can-i-encrypt-user-settings-such-as-passwords-in-my-application" />
    internal static class ProtectString
    {
        // TODO: Replace this with something better.
        private static readonly byte[] entropy = Encoding.Unicode.GetBytes("=LVXsQTb=|APVi_k");

        public static SecureString DecryptString(this string encryptedData)
        {
            if (string.IsNullOrEmpty(encryptedData))
                return null;

            try
            {
                var decryptedData = ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    entropy,
                    DataProtectionScope.CurrentUser);

                return Encoding.Unicode.GetString(decryptedData).ToSecureString();
            }
            catch
            {
                return new SecureString();
            }
        }

        public static string EncryptString(this SecureString input)
        {
            if (input == null)
                return null;

            var encryptedData = ProtectedData.Protect(
                Encoding.Unicode.GetBytes(input.ToInsecureString()),
                entropy,
                DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(encryptedData);
        }

        public static string ToInsecureString(this SecureString input)
        {
            if (input == null)
                return null;

            var ptr = Marshal.SecureStringToBSTR(input);

            try
            {
                return Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(ptr);
            }
        }

        public static SecureString ToSecureString(this IEnumerable<char> input)
        {
            if (input == null)
                return null;

            var secure = new SecureString();

            foreach (var c in input)
                secure.AppendChar(c);

            secure.MakeReadOnly();
            return secure;
        }
    }
}