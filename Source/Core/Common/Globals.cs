using System.Text.RegularExpressions;

namespace Bricklayer.Core.Common
{
    /// <summary>
    /// Contains constants used throughout the client and server.
    /// </summary>
    /// <remarks>
    /// Please do not use actual 'const' values, use 'static readonly', or better yet, a property with only a getter.
    /// </remarks>
    internal static class Globals
    {
        /// <summary>
        /// Globally used regular expressions used for verifying names of entities, rooms, etc.
        /// </summary>
        public class Regexes
        {
            /// <summary>
            /// Regex used for verifying usernames.
            /// This is also checked by the authentication server.
            /// </summary>
            public static Regex NameRegex { get; } = new Regex(@"^[a-zA-Z0-9()_-]{3,32}$");
        }

        /// <summary>
        /// Globably used strings.
        /// </summary>
        public class Strings
        {
            /// <summary>
            /// The app identification ID used for the Lidgren networking library.
            /// The client and server must have matching IDs in order to connect.
            /// </summary>
            public static string NetworkID { get; } = "Bricklayer01";
        }
    }
}