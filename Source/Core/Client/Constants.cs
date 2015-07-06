using System;
using System.Reflection;
using Bricklayer.Core.Common.Properties;

namespace Bricklayer.Core.Client
{
    /// <summary>
    /// Contains constants used throughout the client.
    /// </summary>
    /// <remarks>
    /// Please do not use actual 'const' values, use 'static readonly', or better yet, a property with only a getter.
    /// </remarks>
    internal static class Constants
    {
        /// <summary>
        /// The current version of the client.
        /// </summary>
        public static Version Version { get; } = Assembly.GetEntryAssembly().GetName().Version;

        /// <summary>
        /// The version of the client in readable format. (Example: v0.2.0.0b)
        /// </summary>
        public static string VersionString { get; } = AssemblyVersionName.GetVersion();

        /// <summary>
        /// Globably used strings.
        /// </summary>
        public static class Strings
        {
            /// <summary>
            /// URL to the GitHub repo.
            /// </summary>
            public static string GithubURL { get; } = "https://github.com/Pyratron/Bricklayer";

            /// <summary>
            /// URL to Pyratron Studios.
            /// </summary>
            public static string PyratronURL { get; } = "https://www.pyratron.com";

            /// <summary>
            /// URL to reset Pyratron account password.
            /// </summary>
            internal static string ForgotPasswordURL { get; } = "https://www.pyratron.com/component/users/?view=reset";

            /// <summary>
            /// URL to create a Pyratron account.
            /// </summary>
            internal static string CreateAccountURL { get; } = "https://www.pyratron.com/component/users/?view=registration&variant=Bricklayer";

            /// <summary>
            /// URL to where plugin downloads come from. Revision/attachment ID must be appended.
            /// </summary>
            internal static string DownloadPluginURL { get; } = "https://community.pyratron.com/app.php/db/download/";
        }
    }
}
