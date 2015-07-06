using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Bricklayer.Core.Common
{
    /// <summary>
    /// Shared IO methods.
    /// </summary>
    internal static class IOHelper
    {
        /// <summary>
        /// Creates an instance of a plugin from the specified assembly, if valid.
        /// </summary>
        public static T CreatePluginInstance<T>(Assembly asm, object arguments, PluginData plugin) where T : Plugin
        {
            //Search for a type of 'ServerPlugin' to use
            var types = asm.GetTypes();
            var pluginType = typeof (T);
            var mainType = types.FirstOrDefault(type => pluginType.IsAssignableFrom(type));

            //Create an instance of this type and register it
            if (mainType != null)
            {
                try
                {
                    return (T)Activator.CreateInstance(mainType,
                        BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null,
                        new[] {arguments}, CultureInfo.CurrentCulture);
                }
                catch (Exception e)
                {
                    throw new InvalidDataException(
                        $"Couldn't load assembly '{Path.GetFileName(plugin.Name)}': {e.InnerException}");
                }
            }
            throw new InvalidOperationException(
                $"Assembly '{Path.GetFileName(plugin.Name)}' is not a valid server plugin.");
        }

        /// <summary>
        /// Returns a list plugins from the plugin directory.
        /// These plugins are not executable, and only provide data for the plugin loader.
        /// </summary>
        public static IEnumerable<PluginData> GetPlugins(string directory, JsonSerializerSettings serializationSettings)
        {
            var plugins = new List<PluginData>();
            //Unzip any plugin folders that are zipped (Possible if user didn't extract of left .zip inside)
            var zipped = Directory.GetFiles(directory, "*.zip");
            foreach (var file in zipped)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                var dir = Path.Combine(directory, Path.GetFileNameWithoutExtension(file));
                if (!Directory.Exists(dir)) //If an unzipped version doesn't exist, unzip this plugin
                    ZipFile.ExtractToDirectory(file, dir);
                File.Delete(file);
            }

            //Get all of the directories in the plugin folder
            var dirs = Directory.GetDirectories(directory);
            foreach (var dir in dirs)
            {
                //Make sure there is a plugin metadata file in this folder
                if (!File.Exists(Path.Combine(dir, "plugin.json")))
                    throw new InvalidOperationException(
                        $"Plugin directory '{new DirectoryInfo(dir).Name}' does not contain a plugin.json");
                //Make sure there is a plugin in this folder
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
            //Load the raw bytes of the file, to prevent locking it while the server is running, then load that into the assembly
            return domain.Load(File.ReadAllBytes(Path.Combine(path, "plugin.dll")));
        }
    }
}