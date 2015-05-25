using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Bricklayer.Core.Common.Properties;

//Assembly information
[assembly: AssemblyTitle("Bricklayer")]
[assembly: AssemblyProduct("Bricklayer")]
[assembly: AssemblyDescription("Bricklayer Common")]
[assembly: AssemblyCopyright("Copyright © 2015 Pyratron Studios")]
[assembly: AssemblyCulture("")]

//COM information (not needed)
[assembly: ComVisible(false)]
[assembly: Guid("11549dce-3a75-4c4d-94e2-c5b2c6c5144c")]

//Allow the Bricklayer client and server to use internal methods from the common project
[assembly: InternalsVisibleTo("Bricklayer Client")]
[assembly: InternalsVisibleTo("Bricklayer Server")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.2.0.0")]
[assembly: AssemblyFileVersion("0.2.0.0")]
[assembly: AssemblyVersionName("Alpha", "a")]

#region Custom Attributes

namespace Bricklayer.Core.Common.Properties
{
    /// <summary>
    /// A custom assembly attribute that defines a version, along with a name and prefix.
    /// </summary>
    /// <example>
    /// [assembly: AssemblyVersionName("Beta", "b")]
    /// </example>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyVersionName : Attribute
    {
        private static readonly string prefix = "v";
        private readonly string name;
        private readonly string shortName;

        public AssemblyVersionName(string name, string shortName)
        {
            this.name = name;
            this.shortName = shortName;
        }

        /// <summary>
        /// Formats the version info into a readable string, such as "v0.2.0.0a"
        /// </summary>
        public static string GetVersion()
        {
            var version = Assembly.GetEntryAssembly().GetName().Version;
            var name =
                Assembly.GetEntryAssembly()
                    .GetCustomAttributes(typeof(AssemblyVersionName), false)
                    .Cast<AssemblyVersionName>()
                    .ToList()[0];
            return $"{prefix}{version.Major}.{version.Minor}.{version.Build}.{version.Revision}{name.shortName}";
        }
    }
}

#endregion