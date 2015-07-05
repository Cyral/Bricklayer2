using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bricklayer.Core.Common;
using Bricklayer.Core.Server.Data;
using Newtonsoft.Json;

namespace Bricklayer.Core.Client.Components
{
    /// <summary>
    /// Handles disk saving/loading operations.
    /// </summary>
    public class IOComponent : ClientComponent
    {
        /// <summary>
        /// The current configuration loaded from the config file.
        /// </summary>
        public Config Config { get; private set; }

        /// <summary>
        /// The path to the configuration file.
        /// </summary>
        public string ConfigFile { get; set; }

        /// <summary>
        /// Settings for the JSON.NET serializer
        /// </summary>
        internal JsonSerializerSettings SerializationSettings { get; private set; }

        /// <summary>
        /// Directories relevant to the client, such as screenshots, content, etc.
        /// </summary>
        public readonly Dictionary<string, string> Directories = new Dictionary<string, string>();

        /// <summary>
        /// The path to the Bricklayer directory where the .exe is.
        /// </summary>
        public readonly string ExecutableDirectory =
            Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath);

        /// <summary>
        /// The path to the main Bricklayer directory located in AppData.
        /// </summary>
        public readonly string MainDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".Bricklayer");

        /// <summary>
        /// File containing list of servers
        /// </summary>
        private readonly string serverFile;

        public IOComponent(Client client) : base(client)
        {
            serverFile = Path.Combine(MainDirectory, "servers.config");
        }

        public override async Task Init()
        {
            //Set up JSON.NET settings.
            SerializationSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new JsonContractResolver()
                //Use a custom contract resolver that can read private and internal properties
            };

            if (!Directory.Exists(MainDirectory))
                Directory.CreateDirectory(MainDirectory);
            ConfigFile = Path.Combine(MainDirectory, "config.json");

            //Add the sub-directories to the directory list
            Directories.Add("Content", Path.Combine(ExecutableDirectory, "Content"));
            Directories.Add("Plugins", Path.Combine(MainDirectory, "Plugins"));
            Directories.Add("Maps", Path.Combine(MainDirectory, "Maps"));
            Directories.Add("Screenshots", Path.Combine(MainDirectory, "Screenshots"));

            CheckFiles();
            await base.Init();
        }

        /// <summary>
        /// Reads a list of servers from the json server config file
        /// </summary>
        public List<ServerSaveData> ReadServers()
        {
            var fileName = serverFile;
            if (!File.Exists(fileName))
            {
                //If server config does not exist, create it and write the default server to it
                WriteServers(new List<ServerSaveData> {CreateDefaultServer()});
            }
            var json = File.ReadAllText(fileName);
            if (string.IsNullOrWhiteSpace(json))
            {
                WriteServers(new List<ServerSaveData> {CreateDefaultServer()});
                json = File.ReadAllText(fileName);
            }
            var servers = JsonConvert.DeserializeObject<List<ServerSaveData>>(json);
            return servers;
        }

        /// <summary>
        /// Save servers into a configurable json file
        /// </summary>
        public void WriteServers(List<ServerSaveData> servers)
        {
            var fileName = serverFile;
            if (!File.Exists(fileName))
            {
                var str = File.Create(fileName);
                str.Close();
            }
            var json = JsonConvert.SerializeObject(servers, SerializationSettings);
            File.WriteAllText(fileName, json);
        }

        /// <summary>
        /// Checks to make sure the application files are there, if not it will create them.
        /// </summary>
        internal void CheckFiles()
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
                    return (secureString.ToInsecureString());
                }
            }
            return string.Empty;
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
                    () => Config = JsonConvert.DeserializeObject<Config>(json, SerializationSettings));
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
                var json = JsonConvert.SerializeObject(settings, SerializationSettings);
                File.WriteAllText(ConfigFile, json);
            });
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

        private static ServerSaveData CreateDefaultServer()
        {
            return new ServerSaveData("Local Server", "127.0.0.1", Globals.Values.DefaultServerPort);
        }
    }
}