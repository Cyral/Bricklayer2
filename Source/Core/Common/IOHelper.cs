using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Bricklayer.Core.Common
{
    /// <summary>
    /// Shared IO methods.
    /// </summary>
    internal static class IOHelper
    {
        public static JsonSerializerSettings SerializerSettings { get; set; }

        /// <summary>
        /// Creates an instance of a plugin from the specified assembly, if valid.
        /// </summary>
        /// <param name="asm">The assembly to load.</param>
        public static T CreatePluginInstance<T>(Assembly asm, object arguments, PluginData plugin) where T : Plugin
        {
            // Search for a type of 'T : Plugin' to use
            Type mainType = null;
            try
            {
                var types = asm.GetTypes();
                var pluginType = typeof (T);
                mainType = types.FirstOrDefault(type => pluginType.IsAssignableFrom(type));
            }
            catch (Exception e)
            {
                throw new InvalidDataException(
                    $"Couldn't load assembly '{Path.GetFileName(plugin.Name)}': {e.InnerException}");
            }

            // Create an instance of this type and register it
            if (mainType == null)
                throw new InvalidOperationException(
                    $"Assembly '{Path.GetFileName(plugin.Name)}' is not a valid plugin.");

            try
            {
                var instance =  (T)Activator.CreateInstance(mainType,
                    BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null,
                    new[] {arguments}, CultureInfo.CurrentCulture);
                instance.MainTypeName = mainType.FullName;
                instance.Path = plugin.Path;
                instance.Identifier = plugin.Identifier;
                instance.Authors = plugin.Authors;
                instance.Description = plugin.Description;
                instance.Name = plugin.Name;
                instance.Version = plugin.Version;
                instance.Dependencies = plugin.Dependencies;
                return instance;
            }
            catch (Exception e)
            {
                throw new InvalidDataException(
                    $"Couldn't create instance of assembly '{Path.GetFileName(plugin.Name)}': {e.InnerException}");
            }
        }

        /// <summary>
        /// Returns a list plugins from the plugin directory.
        /// These plugins are not executable, and only provide data for the plugin loader.
        /// </summary>
        public static IEnumerable<PluginData> GetPlugins(string directory, JsonSerializerSettings serializationSettings)
        {
            var plugins = new List<PluginData>();
            // Unzip any plugin folders that are zipped (Possible if user didn't extract of left .zip inside)
            var zipped = Directory.GetFiles(directory, "*.zip");
            foreach (var file in zipped)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                var dir = Path.Combine(directory, Path.GetFileNameWithoutExtension(file));
                if (!Directory.Exists(dir)) // If an unzipped version doesn't exist, unzip this plugin
                    ZipFile.ExtractToDirectory(file, dir);
                File.Delete(file);
            }

            // Get all of the directories in the plugin folder
            var dirs = Directory.GetDirectories(directory);
            foreach (var dir in dirs)
            {
                // Make sure there is a plugin metadata file in this folder
                if (!File.Exists(Path.Combine(dir, "plugin.json")))
                    throw new InvalidOperationException(
                        $"Plugin directory '{new DirectoryInfo(dir).Name}' does not contain a plugin.json");
                // Make sure there is a plugin in this folder
                if (!File.Exists(Path.Combine(dir, "plugin.dll")))
                    throw new FileNotFoundException(
                        $"Plugin directory '{new DirectoryInfo(dir).Name}' does not contain a plugin.dll");
                var metadata = File.ReadAllText(Path.Combine(dir, "plugin.json"));
                var data = JsonConvert.DeserializeObject<PluginData>(metadata, serializationSettings);
                data.Path = dir;
                plugins.Add(data);
            }
            return plugins;
        }

        /// <summary>
        /// Loads a plugin from the specified path.
        /// </summary>
        public static Assembly LoadPlugin(AppDomain domain, string path)
        {
            // Load the raw bytes of the file, to prevent locking it while the server is running, then load that into the assembly
            return domain.Load(File.ReadAllBytes(Path.Combine(path, "plugin.dll")));
        }

        /// <summary>
        /// Load an IConfig from a JSON file. If the config does not exist, it will be created.
        /// </summary>
        internal static async Task<T> LoadConfig<T>(string path) where T : IConfig, new()
        {
            // If config doesn't exist, create it using the default settings and write it.
            IConfig config;
            if (!File.Exists(path))
            {
                config = new T().GenerateDefaultConfig();
                await SaveConfig((T)config, path);
                return (T)config;
            }

            var json = string.Empty;
            await Task.Run(() => json = File.ReadAllText(path));

            // If config is empty, regenerate and read again
            if (string.IsNullOrWhiteSpace(json))
            {
                config = new T().GenerateDefaultConfig();
                json = config.ToString();
                await SaveConfig((T)config, path);
            }

            config = JsonConvert.DeserializeObject<T>(json, SerializerSettings);
            return (T) config;
        }

        /// <summary>
        /// Save an IConfig to a JSON file.
        /// </summary>
        internal static async Task SaveConfig(IConfig config, string path)
        {
            await Task.Factory.StartNew(() =>
            {
                // If server config does not exist, create it
                if (!File.Exists(path))
                {
                    var str = File.Create(path);
                    str.Close();
                    str.Dispose();
                }
                var json = JsonConvert.SerializeObject(config, SerializerSettings);
                File.WriteAllText(path, json);
                Console.WriteLine("Wrote: " + json  + " to: " + path);
            });
        }
    }
}