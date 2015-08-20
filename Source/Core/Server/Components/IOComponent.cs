using System;
using System.Collections.Generic;
using System.Drawing;
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
        public string LogDirectory { get; private set; }

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

        protected override LogType LogType => LogType.IO;

        /// <summary>
        /// Byte array of the image data for the lobby banner.
        /// </summary>
        internal byte[] Banner { get; private set; }

        /// <summary>
        /// Settings for the JSON.NET serializer
        /// </summary>
        internal JsonSerializerSettings SerializationSettings { get; private set; }

        /// <summary>
        /// File containing list of plugin statuses (Whether or not they are enabled or disabled)
        /// </summary>
        private string pluginsFile;

        private DateTime lastLog;
        private StreamWriter logWriter;
        private StringBuilder sb;

        public IOComponent(Server server) : base(server)
        {
        }

        public override async Task Init()
        {
            // Paths.
            ServerDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (ServerDirectory != null)
            {
                LogDirectory = Path.Combine(ServerDirectory, "logs");
                LevelsDirectory = Path.Combine(ServerDirectory, "levels");
                PluginsDirectory = Path.Combine(ServerDirectory, "plugins");
                ConfigFile = Path.Combine(ServerDirectory, "config.json");
                pluginsFile = Path.Combine(ServerDirectory, "plugins.json");
            }

            // Create directories that don't exist.
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);
            if (!Directory.Exists(LevelsDirectory))
                Directory.CreateDirectory(LevelsDirectory);
            if (!Directory.Exists(PluginsDirectory))
                Directory.CreateDirectory(PluginsDirectory);

            sb = new StringBuilder();

            // Set up JSON.net settings.
            SerializationSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                // Use a custom contract resolver that can read private and internal properties
                ContractResolver = new JsonContractResolver()
            };

            // Log a message to the log file stating the startup time and version.
            LogMessage(
                $"SERVER STARTUP:\n{Constants.Strings.ServerTitle} {Constants.VersionString}\n\nServer is starting now, on {DateTime.Now.ToString("U")}\n\n");

            // Load configuration.
            await LoadConfig();
            LoadBanner();

            await base.Init();
        }

        /// <summary>
        /// Opens the server settings and loads them into the config
        /// </summary>
        internal async Task LoadConfig()
        {
            try
            {
                // If server config does not exist, create it and write the default settings
                if (!File.Exists(ConfigFile))
                {
                    Config = Config.GenerateDefaultConfig();
                    await SaveConfig(Config);
                    Log("Configuration created successfully. ({0})", ConfigFile);
                    return;
                }

                var json = string.Empty;
                await Task.Run(() => json = File.ReadAllText(ConfigFile));

                // If config is empty, regenerate and read again
                if (string.IsNullOrWhiteSpace(json))
                {
                    var config = Config.GenerateDefaultConfig();
                    json = config.ToString();
                    await SaveConfig(config);
                }

                Config = JsonConvert.DeserializeObject<Config>(json, SerializationSettings);

                if (Config.Server.AutoSaveTime <= 0)
                    //To prevent the timer from firing endlessly if value is 0 or less. (Config file corrupted, missing value, etc.)
                    Config.Server.AutoSaveTime = Config.GenerateDefaultConfig().Server.AutoSaveTime;

                Log("Configuration loaded. Port: {0}, Auto Save: {1}m", Config.Server.Port.ToString(),
                    Config.Server.AutoSaveTime.ToString());
            }
            catch (Exception ex)
            {
                Logger.WriteLine(LogType.Error, "IOComponent.LoadConfig - {0}", ex.ToString());
            }
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
        /// Logs a message to the server log for the day.
        /// Each day a new log file is created.
        /// </summary>
        internal async void LogMessage(string message)
        {
#if !MONO
            try
            {
                message = message.Replace("\n", Environment.NewLine);
                var date = DateTime.Now;
                if (logWriter == null || date.Date != lastLog.Date)
                {
                    var path = Path.Combine(LogDirectory, date.ToString("MM-dd-yyyy") + ".txt");
                    if (logWriter != null) // On day change, make a new stream/file
                    {
                        logWriter.Close();
                        logWriter.Dispose();
                    }
                    logWriter = new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write), Encoding.UTF8,
                        4096, true);
                }

                if (sb != null)
                    await logWriter.WriteLineAsync(
                        sb.Clear().Append('[').Append(date.ToString("G")).Append("] ").Append(message).ToString());

                lastLog = date;
            }
            catch (Exception e)
            {
                Console.WriteLine("LOGGING ERROR! " + e);
            }
            finally
            {
                logWriter?.Close();
            }
#endif
            // Logging errors occur on MONO, this fixes it (Although the performance increase on the Windows version (less file opening) is not present)
#if MONO
            try
            {
                message = message.Replace("\n", Environment.NewLine);
                var date = DateTime.Now;
                var path = Path.Combine(LogDirectory, date.ToString("MM-dd-yyyy") + ".txt");
                using (var monoWriter = File.AppendText(path))
                {
                    monoWriter.WriteLine(
                        sb.Clear().Append('[').Append(date.ToString("G")).Append("] ").Append(message).ToString());
                }

                lastLog = date;
            }
            catch (Exception e)
            {
                Console.WriteLine("LOGGING ERROR! " + e);
            }
            finally
            {
                if (logWriter != null) logWriter.Close();
            }
#endif
        }

        /// <summary>
        /// Logs a message to the server log. (Which is created in a new file once per day)
        /// Use the logger class if you wish to provide console output.
        /// </summary>
        internal void LogMessage(LogType logType, string message)
        {
            LogMessage(sb.Clear().Append(logType.Prefix).Append(": ").Append(message).ToString());
        }

        /// <summary>
        /// Saves server settings to the server config.
        /// </summary>
        internal async Task SaveConfig(Config settings)
        {
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    // If server config does not exist, create it
                    if (!File.Exists(ConfigFile))
                    {
                        var str = File.Create(ConfigFile);
                        str.Close();
                    }
                    var json = JsonConvert.SerializeObject(settings, SerializationSettings);
                    File.WriteAllText(ConfigFile, json);
                });
            }
            catch (Exception ex)
            {
                Logger.WriteLine(LogType.Error, $"IOComponent.SaveConfig - {ex}");
            }
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
                Logger.WriteLine(LogType.Error,
                    $"Banner size exceeds the size limit of {Globals.Values.MaxBannerWidth}x{Constants.MaxBannerHeight}");
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
                Logger.WriteLine(LogType.Error, $"Level file \"{uuid.ToString("N")}\" not found.");
            return level;
        }
    }
}