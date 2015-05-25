using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bricklayer.Core.Common.Properties;

namespace Bricklayer.Core.Server
{
    /// <summary>
    /// Contains constants used throughout the server.
    /// </summary>
    /// <remarks>
    /// Please do not use actual 'const' values, use 'static readonly', or better yet, a property with only a getter.
    /// </remarks>
    internal static class Constants
    {
        /// <summary>
        /// The current version of the server.
        /// </summary>
        public static readonly Version Version = Assembly.GetEntryAssembly().GetName().Version;

        /// <summary>
        /// The version of the server in readable format. (Example: v0.2.0.0b)
        /// </summary>
        public static readonly string VersionString = AssemblyVersionName.GetVersion();

        /// <summary>
        /// Globably used strings.
        /// </summary>
        public class Strings
        {
            /// <summary>
            /// The title of the server console.
            /// </summary>
            public static string ServerTitle { get; } = $"Bricklayer {VersionString}";
        }
    }
}
