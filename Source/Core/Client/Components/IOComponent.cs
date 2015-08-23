using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Data;
using Microsoft.Xna.Framework.Graphics;
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
        internal static JsonSerializerSettings SerializationSettings => IOHelper.SerializerSettings;

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
        /// File containing list of plugin statuses (Whether or not they are enabled or disabled)
        /// </summary>
        private readonly string pluginsFile;

        /// <summary>
        /// File containing list of servers
        /// </summary>
        private readonly string serverFile;

        /// <summary>
        /// Loader to load textures properly without the content pipeline.
        /// </summary>
        private TextureLoader textureLoader;

        public IOComponent(Client client) : base(client)
        {
            serverFile = Path.Combine(MainDirectory, "servers.json");
            pluginsFile = Path.Combine(MainDirectory, "plugins.json");
        }

        public override async Task Init()
        {
            // Set up JSON.NET settings.
            IOHelper.SerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new JsonContractResolver()
                // Use a custom contract resolver that can read private and internal properties
            };

            textureLoader = new TextureLoader(Client.GraphicsDevice);

            if (!Directory.Exists(MainDirectory))
                Directory.CreateDirectory(MainDirectory);
            ConfigFile = Path.Combine(MainDirectory, "config.json");

            // Add the sub-directories to the directory list
            Directories.Add("Content", Path.Combine(ExecutableDirectory, "Content"));
            Directories.Add("Plugins", Path.Combine(MainDirectory, "Plugins"));
            Directories.Add("Maps", Path.Combine(MainDirectory, "Maps"));
            Directories.Add("Screenshots", Path.Combine(MainDirectory, "Screenshots"));

            CheckFiles();
            await LoadClientConfig();
            await base.Init();
        }

        /// <summary>
        /// Loads a MonoGame Effect file.
        /// </summary>
        public async Task<Effect> LoadEffect(string path)
        {
            var bytes = await Task.Run(() => File.ReadAllBytes(path));
            return new Effect(Client.GraphicsDevice, bytes);
        }

        /// <summary>
        /// Load a Texture2D from path.
        /// </summary>
        public Texture2D LoadTexture(string path)
        {
            return textureLoader.FromFile(path);
        }

        /// <summary>
        /// Reads a list of servers from the json server config file
        /// </summary>
        public async Task<List<ServerData>> ReadServers()
        {
            var fileName = serverFile;
            string json;
            if (!File.Exists(fileName))
            {
                // If server config does not exist, create it and write the default server to it
                json = await WriteServers(new List<ServerData> {CreateDefaultServer()});
            }
            else
                json = await Task.Run(() => File.ReadAllText(fileName));
            if (string.IsNullOrWhiteSpace(json))
            {
                json = await WriteServers(new List<ServerData> {CreateDefaultServer()});
            }
            var servers = JsonConvert.DeserializeObject<List<ServerData>>(json);
            return servers;
        }

        /// <summary>
        /// Save servers into a configurable json file
        /// </summary>
        public async Task<string> WriteServers(List<ServerData> servers)
        {
            return await Task.Factory.StartNew(() =>
            {
                var fileName = serverFile;
                if (!File.Exists(fileName))
                {
                    var str = File.Create(fileName);
                    str.Close();
                }
                var json = JsonConvert.SerializeObject(servers, SerializationSettings);
                File.WriteAllText(fileName, json);
                return json;
            });
        }

        /// <summary>
        /// Reads and returns collection of enabled and disabled plugins.
        /// </summary>
        public Dictionary<string, bool> ReadPluginStatus()
        {
            var plugins = new Dictionary<string, bool>();
            var fileName = pluginsFile;
            var json = string.Empty;
            if (!File.Exists(fileName))
            {
                // If plugin config doesn't exist, create it.
                Client.Plugins.Plugins.ForEach(p => plugins.Add(p.Identifier, true));
                json = WritePluginStatus(plugins);
            }
            else
                json = File.ReadAllText(fileName);
            if (string.IsNullOrWhiteSpace(json))
            {
                // If file is empty, create the contents.
                Client.Plugins.Plugins.ForEach(p => plugins.Add(p.Identifier, true));
                json = WritePluginStatus(plugins);
            }
            return JsonConvert.DeserializeObject<Dictionary<string, bool>>(json);
        }

        /// <summary>
        /// Save plugin statuses into a configurable json file.
        /// </summary>
        public string WritePluginStatus(Dictionary<string, bool> plugins)
        {
            var fileName = pluginsFile;
            if (!File.Exists(fileName))
            {
                var str = File.Create(fileName);
                str.Close();
            }
            var json = JsonConvert.SerializeObject(plugins, SerializationSettings);
            File.WriteAllText(fileName, json);
            return json;
        }

        /// <summary>
        /// Deserialize a JSON file into an IConfig. If the config does not exist, it will be created.
        /// </summary>
        /// <typeparam name="T">Config type.</typeparam>
        /// <param name="path">Full path to config file.</param>
        public async Task<T> LoadConfig<T>(string path) where T : IConfig, new()
        {
            return await IOHelper.LoadConfig<T>(path);
        }

        /// <summary>
        /// Deserialize a JSON file in the root of a plugin's directory into an IConfig. If the config does not exist, it will be created.
        /// </summary>
        /// <typeparam name="T">Config type.</typeparam>
        /// <param name="file">File name relative to plugin root.</param>
        /// <param name="plugin">Plugin directory.</param>
        public async Task<T> LoadConfig<T>(PluginData plugin, string file) where T : IConfig, new()
        {
            return await IOHelper.LoadConfig<T>(Path.Combine(plugin.Path, file));
        }

        /// <summary>
        /// Save a config into a JSON file.
        /// </summary>
        /// <param name="config">Config instance.</param>
        /// <param name="path">Full path to file.</param>
        public async Task SaveConfig(IConfig config, string path)
        {
            await IOHelper.SaveConfig(config, path);
        }

        /// <summary>
        /// Save a config to a JSON file in the plugin's root directory.
        /// </summary>
        /// <param name="config">Config instance.</param>
        /// <param name="plugin">Plugin directory.</param>
        /// <param name="file">File name relative to plugin root.</param>
        public async Task SaveConfig(IConfig config, PluginData plugin, string file)
        {
            await IOHelper.SaveConfig(config, Path.Combine(plugin.Path, file));
        }

        /// <summary>
        /// Checks to make sure the application files are there, if not it will create them.
        /// </summary>
        internal void CheckFiles()
        {
            // Check if the main directory exists. If its dosent, Create the main directory
            if (!Directory.Exists(MainDirectory))
                Directory.CreateDirectory(MainDirectory);
            // Now check for each sub-folder. If they dont exist, then add them
            foreach (var kv in Directories.Where(kv => !Directory.Exists(kv.Value)))
                Directory.CreateDirectory(kv.Value);
        }

        /// <summary>
        /// Decrypts and gets the user's password.
        /// </summary>
        internal string GetPassword()
        {
            if (string.IsNullOrWhiteSpace(Config.Client.Password))
                return string.Empty;
            // Get password
            using (var secureString = Config.Client.Password.DecryptString())
                return (secureString.ToInsecureString());
        }

        /// <summary>
        /// Saves client settings to the client config.
        /// </summary>
        internal async Task SaveClientConfig(Config settings)
        {
            await IOHelper.SaveConfig(settings, ConfigFile);
        }

        /// <summary>
        /// Encrypts and sets the user's password.
        /// </summary>
        internal void SetPassword(string password)
        {
            using (var secureString = password.ToSecureString())
                Config.Client.Password = secureString.EncryptString();
        }

        /// <summary>
        /// Opens the server settings and loads them into the config.
        /// </summary>
        private async Task LoadClientConfig()
        {
            Config = await IOHelper.LoadConfig<Config>(ConfigFile);
        }

        private static ServerData CreateDefaultServer()
        {
            return new ServerData("Local Server", "127.0.0.1", Globals.Values.DefaultServerPort);
        }
    }
}