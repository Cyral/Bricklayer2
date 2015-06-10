using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
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
        /// <summary>
        /// Directories relevant to the client, such as screenshots, content, etc.
        /// </summary>
        public static Dictionary<string, string> Directories = new Dictionary<string, string>();

        /// <summary>
        /// The path to the Bricklayer directory where the .exe is.
        /// </summary>
        public static readonly string ExecutableDirectory =
            Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath);

        /// <summary>
        /// The path to the main Bricklayer directory located in AppData.
        /// </summary>
        public static readonly string MainDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.Bricklayer";

        private static readonly JsonSerializerSettings serializationSettings;

        /// <summary>
        /// The current configuration loaded from the config file.
        /// </summary>
        public Config Config { get; private set; }

        /// <summary>
        /// The path to the configuration file.
        /// </summary>
        public static string ConfigFile { get; }

        static IO()
        {
            //Set up JSON.net settings.
            serializationSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new JsonContractResolver()
                //Use a custom contract resolver that can read private and internal properties
            };

            if (MainDirectory != null)
            {
                ConfigFile = Path.Combine(MainDirectory, "config.json");
            }

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

        /// <summary>
        /// Decrypts and gets the user's password.
        /// </summary>
        internal string GetPassword()
        {
            if (!string.IsNullOrWhiteSpace(Config.Client.Password))
            {
                //Get password
                using (var secureString = Config.Client.Password.DecryptString())
                {
                    return(secureString.ToInsecureString());
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Encrypts and sets the user's password.
        /// </summary>
        internal void SetPassword(string password)
        {
            using (var secureString = password.ToSecureString())
            {
                Config.Client.Password = secureString.EncryptString();
            }
        }

        /// <summary>
        /// Opens the client settings and loads them into the config.
        /// </summary>
        internal async Task LoadConfig()
        {
            //If server config does not exist, create it and write the default settings
            if (!File.Exists(ConfigFile))
            {
                Config = Config.GenerateDefaultConfig();
                await SaveConfig(Config);
                return;
            }

            var json = string.Empty;
            await Task.Factory.StartNew(() => json = File.ReadAllText(ConfigFile));

            //If config is empty, regenerate and read again
            if (string.IsNullOrWhiteSpace(json))
            {
                var config = Config.GenerateDefaultConfig();
                json = config.ToString();
                await SaveConfig(config);
            }

            await
                Task.Factory.StartNew(
                    () => Config = JsonConvert.DeserializeObject<Config>(json, serializationSettings));
            Console.WriteLine("Client configuration loaded.");
        }

        /// <summary>
        /// Saves client settings to the client config.
        /// </summary>
        internal async Task SaveConfig(Config settings)
        {
            await Task.Factory.StartNew(() =>
            {
                //If server config does not exist, create it
                if (!File.Exists(ConfigFile))
                {
                    var str = File.Create(ConfigFile);
                    str.Close();
                }
                var json = JsonConvert.SerializeObject(settings, serializationSettings);
                File.WriteAllText(ConfigFile, json);
            });
        }
    }
}