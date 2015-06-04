using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Common;
using Newtonsoft.Json;

namespace Bricklayer.Core.Client
{
    /// <summary>
    /// Handles disk saving/loading operations
    /// </summary>
    public class IO
    {
        public static Dictionary<string, string> Directories = new Dictionary<string, string>();

        /// <summary>
        /// The path to the main Bricklayer directory located in AppData.
        /// </summary>
        public static readonly string MainDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.Bricklayer";

        /// <summary>
        /// The path to the Bricklayer directory where the .exe is.
        /// </summary>
        public static readonly string ExecutableDirectory =
            Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath);

        private static readonly string configFile = MainDirectory + "\\settings.config";
        private static readonly string serverFile = MainDirectory + "\\servers.config";
        private static readonly JsonSerializerSettings serializationSettings;

        static IO()
        {
            //Set up JSON.net settings.
            serializationSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new JsonContractResolver() //Use a custom contract resolver that can read private and internal properties
            };

            //Add the sub-directories to the directory list
            Directories.Add("Content", Path.Combine(ExecutableDirectory, "Content"));
            Directories.Add("Maps", Path.Combine(MainDirectory, "Maps"));
            Directories.Add("Screenshots", Path.Combine(MainDirectory, "Screenshots"));
        }

        /// <summary>
        /// Checks to make sure the application files are there, if not it will create them.
        /// </summary>
        internal static void CheckFiles()
        {
            //Check if the main directory exists. If its dosent, Create the main directory
            if (!Directory.Exists(MainDirectory))
                Directory.CreateDirectory(MainDirectory);
            //Now check for each sub-folder. If they dont exist, then add them
            foreach (var kv in Directories.Where(kv => !Directory.Exists(kv.Value)))
                Directory.CreateDirectory(kv.Value);
        }
    }
}
