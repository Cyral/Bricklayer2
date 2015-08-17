using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bricklayer.Core.Client.Interface.Windows;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Net.Messages;

namespace Bricklayer.Core.Client.Components
{
    /// <summary>
    /// Handles plugin management.
    /// </summary>
    public class PluginComponent : ClientComponent
    {
        /// <summary>
        /// The number of plugins currently loaded.
        /// </summary>
        public int PluginCount => Plugins.Count;

        internal List<ClientPlugin> Plugins { get; }
        private string loadingPlugin;

        public PluginComponent(Client client) : base(client)
        {
            Plugins = new List<ClientPlugin>();
        }

        public override async Task Init()
        {
            if (!Client.IO.Initialized)
                throw new InvalidOperationException("The IO component must be initialized first.");
            // Resolve assembly references for plugins.
            AppDomain.CurrentDomain.AssemblyResolve +=
                (sender, args) =>
                {
                    // If a plugin.dll is trying to load a referenced assembly.
                    if (args.RequestingAssembly.FullName.Split(',')[0] != "plugin")
                        return null;
                    var path = Path.Combine(loadingPlugin, args.Name.Split(',')[0] + ".dll");
                    if (File.Exists(path))
                        return Assembly.LoadFile(path);
                    path = Path.Combine(loadingPlugin, args.Name.Split(',')[0] + ".exe");
                    // Try to load a .exe if .dll doesn't exist.
                    return File.Exists(path) ? Assembly.LoadFile(path) : null;
                };

            LoadPlugins();

            Client.Events.Network.Auth.PluginDownloadRequested.AddHandler(args =>
            {
                if (!PluginDownloadWindow.IsDownloading(args.Message.ID))
                {
                    var pluginWindow = new PluginDownloadWindow(Client.UI, Client.Window, args.Message.ModName,
                        args.Message.ID,
                        args.Message.FileName, false);
                    pluginWindow.Init();
                    Client.Window.Add(pluginWindow);
                    pluginWindow.Show();
                }
                // Tell the auth server it got the message.
                Client.Network.PingAuthMessage(PingAuthMessage.PingResponse.GotPlugin, args.Message.ID.ToString());
            });

            await base.Init();
        }

        /// <summary>
        /// Loads all plugins that are not already loaded.
        /// </summary>
        internal async void LoadPlugins()
        {
            // Load list of enabled/disabled plugins.
            var statuses = await Client.IO.ReadPluginStatus();

            // Get a list of all the .dlls in the directory.
            List<PluginData> files = null;
            try
            {
                files = IOHelper.GetPlugins(Client.IO.Directories["Plugins"], Client.IO.SerializationSettings).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (files == null)
                return;

            foreach (var file in files.Where(file => !Plugins.Contains(file)))
            {
                // TODO: Use AppDomains for security (Ask Cyral)
                // Load the assembly.
                try
                {
                    // If plugin is enabled.
                    if (!statuses.ContainsKey(file.Identifier) || statuses[file.Identifier])
                    {
                        // Make sure dependencies are met.
                        if (file.Dependencies.Count > 0)
                            foreach (var dep in
                                file.Dependencies.Where(dep => files.All(plugin => plugin.Identifier != dep)))
                                throw new FileNotFoundException(
                                    $"Dependency \"{dep}\" for plugin \"{file.Name}\" not found.");
                        var asm = IOHelper.LoadPlugin(AppDomain.CurrentDomain, file.Path);
                        loadingPlugin = file.Path;
                        RegisterPlugin(IOHelper.CreatePluginInstance<ClientPlugin>(asm, Client, file));
                    }
                    else
                    {
                        // Create plugin instance with no actual assembly, to be displayed in the plugin manager list.
                        var pluginData = new FakePlugin(Client, file);
                        LoadIcon(pluginData);
                        Plugins.Add(pluginData);   
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            // Write updated statuses.
            statuses = new Dictionary<string, bool>();
            Plugins.ForEach(f => statuses.Add(f.Identifier, f.IsEnabled));
            await Client.IO.WritePluginStatus(statuses);
        }

        private void RegisterPlugin(ClientPlugin pluginData)
        {
            pluginData.IsEnabled = true;
            LoadIcon(pluginData);
            Plugins.Add(pluginData);

            // Load plugin content.
            Client.Content.LoadTextures(Path.Combine(pluginData.Path, Path.Combine("Content", "Textures")), Client);
            // Load plugin.
            pluginData.Load();
            Console.WriteLine($"Plugin: Loaded {pluginData.GetInfoString()}");
        }

        /// <summary>
        /// Load icon for plugin.
        /// </summary>
        private void LoadIcon(ClientPlugin pluginData)
        {
            if (Directory.Exists(pluginData.Path))
            {
                var dir = new DirectoryInfo(pluginData.Path);

                string[] extensions = {".jpg", ".jpeg", ".png"};

                var icon =
                    dir.GetFiles()
                        .FirstOrDefault(
                            f => f.Extension.EqualsAny(extensions) && Path.GetFileNameWithoutExtension(f.Name) == "icon");

                if (icon != null)
                {
                    var texture = Client.TextureLoader.FromFile(icon.FullName);
                    if (texture.Height <= 64 && texture.Width <= 64)
                        pluginData.Icon = texture;
                    else
                        Console.WriteLine($"Plugin icon is bigger than the size limit of 64x64. Not loading icon.");
                }
            }
            else
                Console.WriteLine($"Directory {pluginData} does not exist.");
        }

        /// <summary>
        /// Dummy class for disabled plugins, as an instance is needed to display in the plugin manager.
        /// </summary>
        public class FakePlugin : ClientPlugin
        {
            public FakePlugin(Client host, PluginData file) : base(host)
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

        /// <summary>
        /// Disables a plugin and updates the statuses file.
        /// </summary>
        public async Task DisablePlugin(ClientPlugin plugin, Dictionary<string, bool> pluginStatuses)
        {
            if (plugin != null)
            {
                if (Plugins.Any(p => p.Dependencies.Contains(plugin.Identifier)))
                    throw new InvalidOperationException("Other plugins depend on this plugin and must be disabled first.");
                // Set enabled status to false.
                pluginStatuses[plugin.Identifier] = plugin.IsEnabled = false;
                await Client.IO.WritePluginStatus(pluginStatuses);

                // Remove all event handlers which match the main type name.
                foreach (var e in BaseEvent.Events)
                    e.RemoveHandlers(plugin.MainTypeName);

                // Call plugin unload method.
                plugin.Unload();
            }
        }

        /// <summary>
        /// Enables a disabled plugin and updates the statuses file.
        /// </summary>
        /// <returns>The new plugin, if it was loaded from a FakePlugin for the first time.</returns>
        public async Task<ClientPlugin> EnablePlugin(ClientPlugin plugin, Dictionary<string, bool> pluginStatuses)
        {
            if (plugin != null)
            {
                if (Plugins.Contains(plugin) && !(plugin is FakePlugin))
                {
                    // Set enabled status to true.
                    pluginStatuses[plugin.Identifier] = plugin.IsEnabled = true;
                    await Client.IO.WritePluginStatus(pluginStatuses);

                    // Call plugin load method.
                    plugin.Load();
                }
                else
                {
                    if (Plugins.Contains(plugin) && (plugin is FakePlugin))
                        Plugins.Remove(plugin);
                    if (plugin.Dependencies.Count > 0)
                        foreach (var dep in
                            plugin.Dependencies.Where(dep => Plugins.All(p => p.Identifier != dep)))
                            throw new FileNotFoundException(
                                $"Dependency \"{dep}\" for plugin \"{plugin.Name}\" not loaded or enabled.");
                    var asm = IOHelper.LoadPlugin(AppDomain.CurrentDomain, plugin.Path);
                    loadingPlugin = plugin.Path;
                    var retPlugin = IOHelper.CreatePluginInstance<ClientPlugin>(asm, Client, plugin);
                    RegisterPlugin(retPlugin);
                    return retPlugin;
                }
            }
            return plugin;
        }

        /// <summary>
        /// Removes a plugin from the status list, deletes its directory, and calls its unload method.
        /// </summary>
        public async Task DeletePlugin(ClientPlugin plugin, Dictionary<string, bool> pluginStatuses)
        {
            if (plugin != null)
            {
                // Remove from status list.
                pluginStatuses.Remove(plugin.Identifier);
                plugin.IsEnabled = false;
                await Client.IO.WritePluginStatus(pluginStatuses);
                try
                {
                    // Delete directory.
                    if (Directory.Exists(plugin.Path))
                        Directory.Delete(plugin.Path, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Plugins.Remove(plugin);

                // Remove all event handlers which match the main type name.
                //Client.Events.Game.Level.BlockPlaced.RemoveHandlers(plugin.MainTypeName);
                foreach (var e in BaseEvent.Events)
                    e.RemoveHandlers(plugin.MainTypeName);

                // Call plugin unload method.
                plugin.Unload();
            }
        }
    }
}