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
        public static class Strings
        {
            /// <summary>
            /// The app identification ID used for the Lidgren networking library.
            /// The client and server must have matching IDs in order to connect.
            /// </summary>
            public static string NetworkID { get; } = "Bricklayer01";

            /// <summary>
            /// App identification for Auth server
            /// </summary>
            public static string AuthNetworkID { get; } = "BricklayerAuth";

            /// <summary>
            /// IP of the Bricklayer Auth server
            /// </summary>
            public static string AuthServerAddress { get; } = "127.0.0.1";

            /// <summary>
            /// Port of the Bricklayer Auth server
            /// </summary>
            public static int AuthServerPort { get; } = 52142;
        }

        /// <summary>
        /// Globally used values.
        /// </summary>
        public static class Values
        {
            /// <summary>
            /// The default port for servers to run on.
            /// The actual port is configurable through the JSON config.
            /// </summary>
            public static int DefaultServerPort { get; } = 52300;

            /// <summary>
            /// The default port for to send crededentials for authentication to.
            /// The actual port is configurable through the JSON config, although the auth port will most likely never change.
            /// </summary>
            public static int DefaultAuthPort { get; } = 52400;

            /// <summary>
            /// The default address to send crededentials for authentication to.
            /// The actual address is configurable through the JSON config, although the auth address will most likely never change.
            /// </summary>
            public static string DefaultAuthAddress { get; } = "auth.pyratron.com";
        }
    }
}