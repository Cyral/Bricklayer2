using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Common;
using Bricklayer.Core.Server.World;
using Newtonsoft.Json;
using Pyratron.Frameworks.LogConsole;
using Image = System.Drawing.Image;

namespace Bricklayer.Core.Server.Components
{
    /// <summary>
    /// Handles disk operations.
    /// </summary>
    public class IOComponent : ServerComponent
    {
        /// <summary>
        /// The current configuration loaded from the config file.
        /// </summary>
        public Config Config { get; private set; }

        /// <summary>
        /// The path to the configuration file.
        /// </summary>
        public string ConfigFile { get; private set; }

        /// <summary>
        /// The path to the logging directory, where a file is created each day for logging.
        /// </summary>
        public string LogDirectory
        {
            get { return logDirectory; }
            private set { logDirectory = Logger.LogDirectory = value; }
        }

        /// <summary>
        /// The path to the plugins directory, which contains the plugin .dlls
        /// </summary>
        public string PluginsDirectory { get; private set; }

        /// <summary>
        /// The path to the directory containing level saves.
        /// </summary>
        public string LevelsDirectory { get; private set; }

        /// <summary>
        /// The path to the server root directory.
        /// </summary>
        public string ServerDirectory { get; private set; }

        protected internal override LogType LogType { get; } = new LogType("IO", ConsoleColor.Cyan);

        /// <summary>
        /// Byte array of the image data for the lobby banner.
        /// </summary>
        internal byte[] Banner { get; private set; }

        /// <summary>
        /// Settings for the JSON.NET serializer
        /// </summary>
        internal static JsonSerializerSettings SerializationSettings => IOHelper.SerializerSettings;

        /// <summary>
        /// File containing list of plugin statuses (Whether or not they are enabled or disabled)
        /// </summary>
        private readonly string pluginsFile;

        private string logDirectory;

        public IOComponent(Server server) : base(server)
        {
            // Paths.
            ServerDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (ServerDirectory != null)
            {
                LogDirectory = Path.Combine(ServerDirectory, "logs");
                if (!Directory.Exists(LogDirectory))
                    Directory.CreateDirectory(LogDirectory);
                LevelsDirectory = Path.Combine(ServerDirectory, "levels");
                PluginsDirectory = Path.Combine(ServerDirectory, "plugins");
                ConfigFile = Path.Combine(ServerDirectory, "config.json");
                pluginsFile = Path.Combine(ServerDirectory, "plugins.json");
            }
        }

        public override async Task Init()
        {
            // Set up JSON.net settings.
            IOHelper.SerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                // Use a custom contract resolver that can read private and internal properties
                ContractResolver = new JsonContractResolver()
            };

            // Create directories that don't exist.
            if (!Directory.Exists(LevelsDirectory))
                Directory.CreateDirectory(LevelsDirectory);
            if (!Directory.Exists(PluginsDirectory))
                Directory.CreateDirectory(PluginsDirectory);

            // Load configuration.
            await LoadServerConfig();
            LoadBanner();

            await base.Init();
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
        public static async Task SaveConfig(IConfig config, PluginData plugin, string file)
        {
            await IOHelper.SaveConfig(config, Path.Combine(plugin.Path, file));
        }

        /// <summary>
        /// Reads and returns collection of enabled and disabled plugins.
        /// </summary>
        public Dictionary<string, bool> ReadPluginStatus()
        {
            var plugins = new Dictionary<string, bool>();
            var fileName = pluginsFile;
            string json;
            if (!File.Exists(fileName))
            {
                // If plugin config doesn't exist, create it.
                Server.Plugins.Plugins.ForEach(p => plugins.Add(p.Identifier, true));
                json = WritePluginStatus(plugins);
            }
            else
                json = File.ReadAllText(fileName);
            if (string.IsNullOrWhiteSpace(json))
            {
                // If file is empty, create the contents.
                Server.Plugins.Plugins.ForEach(p => plugins.Add(p.Identifier, true));
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
        /// Opens the server settings and loads them into the config.
        /// </summary>
        internal async Task LoadServerConfig()
        {
            try
            {
                Config = await IOHelper.LoadConfig<Config>(ConfigFile);

                // To prevent the timer from firing endlessly if value is 0 or less. (Config file corrupted, missing value, etc.)
                if (Config.Server.AutoSaveTime <= 0)
                    Config.Server.AutoSaveTime = ((Config) new Config().GenerateDefaultConfig()).Server.AutoSaveTime;

                Logger.Log(LogType, "Configuration loaded. Port: {0}, Auto Save: {1}m", Config.Server.Port.ToString(),
                    Config.Server.AutoSaveTime.ToString());
            }
            catch (Exception ex)
            {
                Logger.Error(LogType, "Error loading config: {0}", ex.ToString());
            }
        }
       
        /// <summary>
        /// Saves server settings to the server config.
        /// </summary>
        internal async Task SaveServerConfig(Config settings)
        {
            try
            {
                await IOHelper.SaveConfig(settings, ConfigFile);
            }
            catch (Exception ex)
            {
                Logger.Error(LogType, "Error loading config: {0}", ex.ToString());
            }
        }

        internal async Task SaveLevel(Level level)
        {
            await Task.Run(() =>
            {
                using (var filestream = new BufferedStream(
                    File.Open(Path.Combine(LevelsDirectory, level.UUID.ToString("N") + ".level"), FileMode.Create)))
                using (var gzip = new GZipStream(filestream, CompressionMode.Compress, true))
                using (var writer = new BinaryWriter(gzip))
                {
                    // Version used for save format migrations
                    writer.Write(Constants.Version.Major);
                    writer.Write(Constants.Version.Minor);
                    writer.Write(Constants.Version.Build);
                    writer.Write(Constants.Version.Revision);

                    // Read tiles
                    level.EncodeTiles(writer);
                }
            });
        }

        internal async Task<Level> LoadLevel(Guid uuid)
        {
            var level = new Level(Server, await Server.Database.GetLevelData(uuid));
            var path = Path.Combine(LevelsDirectory, uuid.ToString("N") + ".level");
            if (File.Exists(path))
            {
                await Task.Run(() =>
                {
                    using (var filestream = new BufferedStream(File.Open(path, FileMode.Open)))
                    using (var gzip = new GZipStream(filestream, CompressionMode.Decompress, true))
                    using (var reader = new BinaryReader(gzip))
                    {
                        // Version used for save format migrations
                        // ReSharper disable once UnusedVariable (This may be used in the future)
                        var version = new Version(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(),
                            reader.ReadInt32());

                        // Read tiles
                        level.DecodeTiles(reader);
                    }
                });
            }
            else
                Logger.Error(LogType, $"Level file \"{uuid.ToString("N")}\" not found.");
            return level;
        }

        /// <summary>
        /// Loads the banner JPEG or PNG.
        /// </summary>
        private void LoadBanner()
        {
            var path = string.Empty;

            // Scan for possible names
            var formats = new[] {"jpg", "jpeg", "png"};

            foreach (var formatPath in
                formats.Select(format => Path.Combine(ServerDirectory, "banner." + format)).Where(File.Exists))
            {
                path = formatPath;
                break;
            }

            if (string.IsNullOrEmpty(path))
                return;

            var img = Image.FromFile(path);

            if (img.Height <= Constants.MaxBannerHeight && img.Width <= Globals.Values.MaxBannerWidth)
            {
                using (var ms = new MemoryStream())
                {
                    img.Save(ms, ImageFormat.Png);
                    Banner = ms.ToArray();
                }
            }
            else
                Logger.Warn(LogType,
                    $"Banner size exceeds the size limit of {Globals.Values.MaxBannerWidth}x{Constants.MaxBannerHeight}");
        }
    }
}