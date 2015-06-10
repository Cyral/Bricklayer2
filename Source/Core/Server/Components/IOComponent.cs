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

        public override async Task Init()
        {
            //Paths.
            ServerDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (ServerDirectory != null)
            {
                LogDirectory = Path.Combine(ServerDirectory, "logs");
                ConfigFile = Path.Combine(ServerDirectory, "config.json");
            }

            //Create directories that don't exist.
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);

            sb = new StringBuilder();

            //Set up JSON.net settings.
            serializationSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new JsonContractResolver() //Use a custom contract resolver that can read private and internal properties
            };

            //Log a message to the log file stating the startup time and version.
            LogMessage(
                $"SERVER STARTUP:\n{Constants.Strings.ServerTitle} {Constants.VersionString}\n\nServer is starting now, on {DateTime.Now.ToString("U")}\n\n");

            //Load configuration.
            await LoadConfig();

            await base.Init();
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
                        () => Config = JsonConvert.DeserializeObject<Config>(json, serializationSettings));

                Log("Configuration loaded. Port: {0}", Config.Server.Port.ToString());
            }
            catch (Exception ex)
            {
                Logger.WriteLine(LogType.Error, "IOComponent.LoadConfig - {0}", ex.ToString());
            }
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
