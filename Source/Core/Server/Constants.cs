using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bricklayer.Core.Common;
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
        public static readonly Version Version = Assembly.GetEntryAssembly().GetName().Version;
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
