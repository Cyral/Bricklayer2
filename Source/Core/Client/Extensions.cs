using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Client
{
    /// <summary>
    /// Client specific extension methods.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Checks to see if any of the object(s) in the collection are equal to the source value.
        /// </summary>
        /// <typeparam name="T">The type of object used.</typeparam><param name="source">The source object.</param><param name="list">The possible objects to equal.</param>
        /// <returns>
        /// True of any object(s) are equal to the source, false otherwise.
        /// </returns>
        public static bool EqualsAny<T>(this T source, params T[] list)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return list.Contains(source);
        }
    }

    /// <summary>
    ///  Encrypts and decrypts string for saving password configs for example
    /// </summary>
    /// <see cref="https://stackoverflow.com/questions/8871337/how-can-i-encrypt-user-settings-such-as-passwords-in-my-application" />
    internal static class ProtectString
    {
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
