using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bricklayer.Core.Client.Interface.Windows;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Net;
using Bricklayer.Core.Common.Net.Messages;
using Microsoft.Xna.Framework.Graphics;

/* 
Plugin System Explanation:

When the game starts, all folders in the plugin directory that are valid are loaded to a list.
Any dependencies are checked for, and referenced .dlls are resolved with AssemblyResolve.
Each plugin is loaded using Activator.CreateInstance. If a plugin is disabled (plugins.config), it
is not loaded, but a 'FakePlugin' class instance is created. This FakePlugin has all of the details
(name, authors, etc.), but no loaded assembly. This is used so the plugin manager can list disabled plugins.

In my research, it seems references (in this case, to the Client or Server), cannot be shared across AppDomains.
This makes it impossible to use AppDomains to sandbox plugins (which we would like to do).

Plugin reloading was decided not to be included, but disabling/enabling a plugin will call the plugin's Load or
Unload method, respectively. It is up to the plugin author to implement functionality to completely reset a plugin.
However, any events registered by the plugin's main assembly (plugin.dll) will automatically be unregistered.
(When an event is added, the event manager tries to find the executing plugin using reflection. When a plugin
is disabled, it finds all events that have the plugin assembly's FullName, and removes them.) To keep a list of all
of these events, Event<TArgs> derives from BaseEvent, which has a static list of events and a method that is overloaded
to unload all events with the specified full name.

For the client, plugins should only be disabled/enabled from the login screen.
On the server, plugins can be disabled at any time, however this could cause issues. (Imagine disabling the default blocks plugin
while the server is running...) As said above, it is not recommended to run the client or server after disabling a plugin.

Note: Any .zip files in the plugin folder will be extracted at runtime. 

Plugins can also automatically be installed (on the client) by using the plugin database on the website. The install button
on the website sends a message to the auth server, which tells the client to download the plugin (in the case of it recently
being online), if it doesn't get a response back from the client, it will queue the download for next time the user is online.
*/

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

        /// <summary>
        /// Class contains a list of plugin messages. Server and Client plugins will use this
        /// to recieve an id that both the server and the client knows for the plugin message.
        /// </summary>
        public PluginMessages Pluginmessages;

        internal List<ClientPlugin> Plugins { get; }
        private readonly Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
        private string loadingPlugin;

        public PluginComponent(Client client) : base(client)
        {
            Plugins = new List<ClientPlugin>();
            Pluginmessages = new PluginMessages();
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
        internal void LoadPlugins()
        {
            // Load list of enabled/disabled plugins.
            var statuses = Client.IO.ReadPluginStatus();

            // Get a list of all the .dlls in the directory.
            List<PluginData> files = null;
            try
            {
                files = IOHelper.GetPlugins(Client.IO.Directories["Plugins"], IOComponent.SerializationSettings).ToList();
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
                        statuses[file.Identifier] = true;
                        var asm = IOHelper.LoadPlugin(AppDomain.CurrentDomain, file.Path);
                        assemblies[file.Identifier] = asm;
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
            Client.IO.WritePluginStatus(statuses);
        }

        internal void ReloadContent()
        {
            foreach (var plugin in Plugins.Where(p => p.IsEnabled))
            {
                Client.Content.LoadPluginContent(plugin);
            }
        }

        private void RegisterPlugin(ClientPlugin plugin)
        {
            plugin.IsEnabled = true;
            LoadIcon(plugin);
            Plugins.Add(plugin);

            // Load plugin content.
            Client.Content.LoadPluginContent(plugin);

            // Load plugin.
            plugin.Load();
            Client.Events.Game.PluginStatusChanged.Invoke(new EventManager.GameEvents.PluginStatusEventArgs(plugin));
            Console.WriteLine($"Plugin: Loaded {plugin.GetInfoString()}");
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
                    var texture = Client.IO.LoadTexture(icon.FullName);
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
        /// Disables a plugin and updates the statuses file.
        /// </summary>
        internal void DisablePlugin(ClientPlugin plugin, Dictionary<string, bool> pluginStatuses)
        {
            if (plugin != null)
            {
                if (Plugins.Any(p => p.Dependencies.Contains(plugin.Identifier)))
                    throw new InvalidOperationException(
                        "Other plugins depend on this plugin and must be disabled first.");
                // Set enabled status to false.
                pluginStatuses[plugin.Identifier] = plugin.IsEnabled = false;
                Client.IO.WritePluginStatus(pluginStatuses);

                // Remove all event handlers which match the main type name.
                // See explanation at top of file.
                foreach (var e in BaseEvent.Events)
                    e.RemoveHandlers(plugin.MainTypeName);

                Client.Events.Game.PluginStatusChanged.Invoke(new EventManager.GameEvents.PluginStatusEventArgs(plugin));

                // Call plugin unload method.
                plugin.Unload();
                Console.WriteLine($"Plugin: Disabled {plugin.GetInfoString()}");
            }
        }

        /// <summary>
        /// Enables a disabled plugin and updates the statuses file.
        /// </summary>
        /// <returns>The new plugin, if it was loaded from a FakePlugin for the first time.</returns>
        internal ClientPlugin EnablePlugin(ClientPlugin plugin, Dictionary<string, bool> pluginStatuses)
        {
            if (plugin != null)
            {
                if (Plugins.Contains(plugin) && !(plugin is FakePlugin))
                {
                    // Set enabled status to true.
                    pluginStatuses[plugin.Identifier] = plugin.IsEnabled = true;
                    Client.IO.WritePluginStatus(pluginStatuses);

                    // Create new instance and call plugin load method.
                    loadingPlugin = plugin.Path;
                    if (Plugins.Contains(plugin))
                        Plugins.Remove(plugin);
                    Texture2D icon = null;
                    if (plugin.Icon != null)
                        icon = plugin.Icon;
                    var newPlugin = IOHelper.CreatePluginInstance<ClientPlugin>(assemblies[plugin.Identifier], Client, plugin);
                    Client.Content.LoadPluginContent(newPlugin);
                    if (icon != null)
                        newPlugin.Icon = icon;
                    newPlugin.Load();
                    Plugins.Add(newPlugin);
                    Client.Events.Game.PluginStatusChanged.Invoke(new EventManager.GameEvents.PluginStatusEventArgs(plugin));
                    Console.WriteLine($"Plugin: Enabled {plugin.GetInfoString()}");
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
                    var asm = IOHelper.LoadPlugin(AppDomain.CurrentDomain, plugin.Path);
                    assemblies[plugin.Identifier] = asm;
                    loadingPlugin = plugin.Path;
                    var retPlugin = IOHelper.CreatePluginInstance<ClientPlugin>(asm, Client, plugin);
                    RegisterPlugin(retPlugin);
                    pluginStatuses[retPlugin.Identifier] = true;
                    Client.IO.WritePluginStatus(pluginStatuses);
                    return retPlugin;
                }
            }
            return plugin;
        }

        /// <summary>
        /// Removes a plugin from the status list, deletes its directory, and calls its unload method.
        /// </summary>
        internal void DeletePlugin(ClientPlugin plugin, Dictionary<string, bool> pluginStatuses)
        {
            if (plugin != null)
            {
                // Remove from status list.
                pluginStatuses.Remove(plugin.Identifier);
                plugin.IsEnabled = false;
                Client.IO.WritePluginStatus(pluginStatuses);
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
                foreach (var e in BaseEvent.Events)
                    e.RemoveHandlers(plugin.MainTypeName);

                Client.Events.Game.PluginStatusChanged.Invoke(new EventManager.GameEvents.PluginStatusEventArgs(plugin));

                // Call plugin unload method.
                plugin.Unload();
                Console.WriteLine($"Plugin: Deleted {plugin.GetInfoString()}");
            }
        }

        /// <summary>
        /// Dummy class for disabled plugins, as an instance is needed to display in the plugin manager.
        /// </summary>
        private class FakePlugin : ClientPlugin
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

        internal void Unload()
        {
            foreach (var plugin in Plugins)
            {
                plugin.Unload();
            }
        }
    }
}