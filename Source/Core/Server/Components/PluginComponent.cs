using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Net;
using Pyratron.Frameworks.LogConsole;

/*
 * See PluginComponent in the Client project for full explanation on how the system works.
*/

namespace Bricklayer.Core.Server.Components
{
    /// <summary>
    /// Handles plugin management.
    /// </summary>
    public class PluginComponent : ServerComponent
    {
        /// <summary>
        /// The number of plugins currently loaded.
        /// </summary>
        public int PluginCount => Plugins.Count;

        protected internal override LogType LogType { get; } = new LogType("Plugin", ConsoleColor.Green);
        internal List<ServerPlugin> Plugins { get; }
        private readonly Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
        private string loadingPlugin;

        /// <summary>
        /// Class contains a list of plugin messages. Server and Client plugins will use this
        /// to recieve an id that both the server and the client knows for the plugin message.
        /// </summary>
        public PluginMessages PluginMessages { get; private set; }

        private Dictionary<string, bool> pluginStatuses = new Dictionary<string, bool>();


        public PluginComponent(Server server) : base(server)
        {
            Plugins = new List<ServerPlugin>();
            PluginMessages = new PluginMessages();
        }

        public override async Task Init()
        {
            if (!Server.IO.Initialized)
                throw new InvalidOperationException("The IO component must be initialized first.");
            // Resolve assembly references for plugins. 
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                // If a plugin.dll is trying to load a referenced assembly
                if (args.RequestingAssembly.FullName.Split(',')[0] != "plugin")
                    return null;

                var path = Path.Combine(loadingPlugin, args.Name.Split(',')[0] + ".dll");
                if (File.Exists(path))
                    return Assembly.LoadFile(path);
                path = Path.Combine(loadingPlugin, args.Name.Split(',')[0] + ".exe");
                // Try to load a .exe if .dll doesn't exist
                return File.Exists(path) ? Assembly.LoadFile(path) : null;
            };

            LoadPlugins();

            await base.Init();
        }

        /// <summary>
        /// Disables a plugin and updates the statuses file.
        /// </summary>
        public void DisablePlugin(ServerPlugin plugin)
        {
            if (plugin != null)
            {
                if (Plugins.Any(p => p.Dependencies.Contains(plugin.Identifier)))
                    throw new InvalidOperationException(
                        "Other plugins depend on this plugin and must be disabled first.");
                // Set enabled status to false.
                pluginStatuses[plugin.Identifier] = plugin.IsEnabled = false;
                Server.IO.WritePluginStatus(pluginStatuses);

                // Remove all event handlers which match the main type name.
                // See explanation at top of file.
                foreach (var e in BaseEvent.Events)
                    e.RemoveHandlers(plugin.MainTypeName);

                Server.Events.Game.PluginStatusChanged.Invoke(new EventManager.GameEvents.PluginStatusEventArgs(plugin));

                // Call plugin unload method.
                plugin.Unload();
                Debug.WriteLine($"Plugin: Disabled {plugin.GetInfoString()}");
            }
        }

        /// <summary>
        /// Enables a disabled plugin and updates the statuses file.
        /// </summary>
        /// <returns>The new plugin, if it was loaded from a FakePlugin for the first time.</returns>
        public ServerPlugin EnablePlugin(ServerPlugin plugin)
        {
            if (plugin != null)
            {
                if (Plugins.Contains(plugin) && !(plugin is FakePlugin))
                {
                    // Set enabled status to true.
                    pluginStatuses[plugin.Identifier] = plugin.IsEnabled = true;
                    Server.IO.WritePluginStatus(pluginStatuses);

                    // Create new instance and call plugin load method.
                    loadingPlugin = plugin.Path;
                    if (Plugins.Contains(plugin))
                        Plugins.Remove(plugin);
                    var newPlugin = IOHelper.CreatePluginInstance<ServerPlugin>(assemblies[plugin.Identifier], Server,
                        plugin);
                    newPlugin.Load();
                    Plugins.Add(newPlugin);
                    Server.Events.Game.PluginStatusChanged.Invoke(
                        new EventManager.GameEvents.PluginStatusEventArgs(plugin));
                    Debug.WriteLine($"Plugin: Enabled {plugin.GetInfoString()}");
                }
                else
                {
                    // If the plugin hasn't been loaded yet (disabled at startup), create an instance and load it.
                    if (Plugins.Contains(plugin) && (plugin is FakePlugin))
                        Plugins.Remove(plugin);
                    if (plugin.Dependencies.Count > 0)
                        foreach (var dep in
                            plugin.Dependencies.Where(dep => Plugins.All(p => p.Identifier != dep)))
                            throw new FileNotFoundException(
                                $"Dependency \"{dep}\" for plugin \"{plugin.Name}\" not loaded or enabled.");
                    ServerPlugin retPlugin = null;
                    try
                    {
                        var asm = IOHelper.LoadPlugin(AppDomain.CurrentDomain, plugin.Path);
                        assemblies[plugin.Identifier] = asm;
                        loadingPlugin = plugin.Path;
                        retPlugin = IOHelper.CreatePluginInstance<ServerPlugin>(asm, Server, plugin);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(LogType, $"Error creating instance of {plugin.Identifier}: {e}");
                    }
                    RegisterPlugin(retPlugin);
                    if (retPlugin != null)
                    {
                        pluginStatuses[retPlugin.Identifier] = true;
                        Server.IO.WritePluginStatus(pluginStatuses);
                        return retPlugin;
                    }
                }
            }
            return plugin;
        }

        /// <summary>
        /// Loads all plugins that are not already loaded.
        /// </summary>
        internal void LoadPlugins()
        {
            // Get a list of all the .dlls in the directory
            List<PluginData> files = null;
            try
            {
                // Load list of enabled/disabled plugins.
                pluginStatuses = Server.IO.ReadPluginStatus();
                files = IOHelper.GetPlugins(Server.IO.PluginsDirectory, IOComponent.SerializationSettings).ToList();
            }
            catch (Exception e)
            {
                Logger.Error(LogType, e.Message);
            }
            if (files == null)
                return;

            foreach (var file in files.Where(file => !Plugins.Contains(file)))
            {
                // TODO: Use AppDomains for security
                // Load the assembly
                try
                {
                    // If plugin is enabled.
                    if (!pluginStatuses.ContainsKey(file.Identifier) || pluginStatuses[file.Identifier])
                    {
                        // Make sure dependencies are met.
                        if (file.Dependencies.Count > 0)
                        {
                            // ReSharper disable once PossibleMultipleEnumeration
                            foreach (
                                var dep in
                                    file.Dependencies.Where(dep => files.All(plugin => plugin.Identifier != dep)))
                                throw new FileNotFoundException(
                                    $"Dependency \"{dep}\" for plugin \"{file.Name}\" not found.");
                        }
                        // Load plugin
                        pluginStatuses[file.Identifier] = true;
                        loadingPlugin = file.Path;
                        var asm = IOHelper.LoadPlugin(AppDomain.CurrentDomain, file.Path);
                        assemblies[file.Identifier] = asm;
                        var loadedPlugin = IOHelper.CreatePluginInstance<ServerPlugin>(asm, Server, file);
                        RegisterPlugin(loadedPlugin);
                        Logger.Log(LogType, $"Loaded {loadedPlugin.GetInfoString()}");
                    }
                    else
                    {
                        // Create plugin instance with no actual assembly, to be displayed in the plugin manager list.
                        var pluginData = new FakePlugin(Server, file);
                        Plugins.Add(pluginData);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(LogType, $"Error creating instance of {file.Identifier}: {e}");
                }
            }
            // Write updated statuses.
            pluginStatuses = new Dictionary<string, bool>();
            Plugins.ForEach(f => pluginStatuses.Add(f.Identifier, f.IsEnabled));
            Server.IO.WritePluginStatus(pluginStatuses);
        }

        private void RegisterPlugin(ServerPlugin plugin)
        {
            try
            {
                Plugins.Add(plugin);
                plugin.Load();
                Server.Events.Game.PluginStatusChanged.Invoke(new EventManager.GameEvents.PluginStatusEventArgs(plugin));
                Debug.WriteLine($"Loaded {plugin.GetInfoString()}");
            }
            catch (Exception e)
            {
                Logger.Error(LogType, $"Error calling Load on {plugin.Identifier}: {e}");
            }
        }

        /// <summary>
        /// Dummy class for disabled plugins, as an instance is needed to display in the plugin manager.
        /// </summary>
        private class FakePlugin : ServerPlugin
        {
            public FakePlugin(Server host, PluginData file) : base(host)
            {
                MainTypeName = ">FakePlugin";
                Identifier = file.Identifier;
                Name = file.Name;
                Description = file.Name;
                Authors = file.Authors;
                Dependencies = file.Dependencies;
                Version = file.Version;
                Path = file.Path;
                IsEnabled = false;
            }

            public override void Load()
            {
            }

            public override void Unload()
            {
            }
        }
    }
}