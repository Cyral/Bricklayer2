using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Common;
using Newtonsoft.Json;

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
        /// The path to the server root directory.
        /// </summary>
        public string ServerDirectory { get; private set; }

        protected override LogType LogType => LogType.IO;
        private DateTime lastLog;
        private StreamWriter logWriter;
        private StringBuilder sb;
        private JsonSerializerSettings serializationSettings;

        public IOComponent(Server server) : base(server)
        {
        }

        /// <summary>
        /// Returns a list plugins from the plugin directory.
        /// These plugins are not executable, and only provide data for the plugin loader.
        /// </summary>
        public IEnumerable<PluginData> GetPlugins()
        {
            var plugins = new List<PluginData>();
            //Unzip any plugin folders that are zipped (Possible if user didn't extract of left .zip inside)
            var zipped = Directory.GetFiles(PluginsDirectory, "*.zip");
            foreach (var file in zipped)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                var dir = Path.Combine(PluginsDirectory, Path.GetFileNameWithoutExtension(file));
                if (!Directory.Exists(dir))  //If an unzipped version doesn't exist, unzip this plugin
                    System.IO.Compression.ZipFile.ExtractToDirectory(file, dir);
                File.Delete(file);
            }
            
            //Get all of the directories in the plugin folder
            var dirs = Directory.GetDirectories(PluginsDirectory);
            foreach (var dir in dirs)
            {
                //Make sure there is a plugin metadata file in this folder
                if (!File.Exists(Path.Combine(dir, "plugin.json")))
                {
                    Logger.WriteLine(LogType.Error, $"Plugin directory '{new DirectoryInfo(dir).Name}' does not contain a plugin.json");
                    continue;
                }
                //Make sure there is a plugin in this folder
                if (!File.Exists(Path.Combine(dir, "plugin.dll")))
                    Logger.WriteLine(LogType.Error,
                        $"Plugin directory '{new DirectoryInfo(dir).Name}' does not contain a plugin.dll");
                else
                {
                    var metadata = File.ReadAllText(Path.Combine(dir, "plugin.json"));
                    var data = JsonConvert.DeserializeObject<PluginData>(metadata, serializationSettings);
                    data.Path = dir;
                    plugins.Add(data);
                }
            }
            return plugins;
        }

        public override async Task Init()
        {
            //Paths.
            ServerDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (ServerDirectory != null)
            {
                LogDirectory = Path.Combine(ServerDirectory, "logs");
                PluginsDirectory = Path.Combine(ServerDirectory, "plugins");
                ConfigFile = Path.Combine(ServerDirectory, "config.json");
            }

            //Create directories that don't exist.
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);
            if (!Directory.Exists(PluginsDirectory))
                Directory.CreateDirectory(PluginsDirectory);

            sb = new StringBuilder();

            //Set up JSON.net settings.
            serializationSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new JsonContractResolver()
                //Use a custom contract resolver that can read private and internal properties
            };

            //Log a message to the log file stating the startup time and version.
            LogMessage(
                $"SERVER STARTUP:\n{Constants.Strings.ServerTitle} {Constants.VersionString}\n\nServer is starting now, on {DateTime.Now.ToString("U")}\n\n");

            //Load configuration.
            await LoadConfig();

            await base.Init();
        }

        /// <summary>
        /// Loads a plugin from the specified path.
        /// </summary>
        public Assembly LoadPlugin(AppDomain domain, string path)
        {
            //Load the raw bytes of the file, to prevent locking it while the server is running, then load that into the assembly
            return domain.Load(File.ReadAllBytes(Path.Combine(path, "plugin.dll")));
        }

        /// <summary>
        /// Opens the server settings and loads them into the config
        /// </summary>
        internal async Task LoadConfig()
        {
            try
            {
                //If server config does not exist, create it and write the default settings
                if (!File.Exists(ConfigFile))
                {
                    Config = Config.GenerateDefaultConfig();
                    await SaveConfig(Config);
                    Log("Configuration created successfully.");
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
                        delegate { Config = JsonConvert.DeserializeObject<Config>(json, serializationSettings); });


                Log("Configuration loaded. Port: {0}", Config.Server.Port.ToString());
            }
            catch (Exception ex)
            {
                Logger.WriteLine(LogType.Error, "IOComponent.LoadConfig - {0}", ex.ToString());
            }
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
                    if (logWriter != null) //On day change, make a new stream/file
                    {
                        logWriter.Close();
                        logWriter.Dispose();
                    }
                    logWriter = new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write), Encoding.UTF8,
                        4096, true);
                }

                if (sb != null)
                    await
                        logWriter.WriteLineAsync(
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
            //Logging errors occur on MONO, this fixes it (Although the performance increase on the Windows version (less file opening) is not present)
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
            catch (Exception ex)
            {
                Logger.WriteLine(LogType.Error, "IOComponent.SaveConfig - {0}", ex.ToString());
            }
        }
    }
}