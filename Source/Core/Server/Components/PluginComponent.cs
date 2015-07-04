using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
        public int PluginCount => plugins.Count;

        protected override LogType LogType => LogType.Plugin;
        private readonly List<ServerPlugin> plugins;

        public PluginComponent(Server server) : base(server)
        {
            plugins = new List<ServerPlugin>();
        }

        public override async Task Init()
        {
            if (!Server.IO.Initialized)
                throw new InvalidOperationException("The IO component must be initialized first.");
            LoadPlugins();
            await base.Init();
        }

        private void LoadPlugins()
        {
            //Get a list of all the .dlls in the directory
            var files = Server.IO.GetPlugins();
            foreach (var file in files)
            {
                //TODO: Use AppDomains for security
                //Load the assembly
                var asm = Server.IO.LoadPlugin(AppDomain.CurrentDomain, file.Path);

                //Search for a type of 'ServerPlugin' to use
                var types = asm.GetTypes();
                var pluginType = typeof (ServerPlugin);
                var mainType = types.FirstOrDefault(type => pluginType.IsAssignableFrom(type));

                //Create an instance of this type and register it
                if (mainType != null)
                {
                    try
                    {
                        RegisterPlugin(
                            (ServerPlugin)
                                Activator.CreateInstance(mainType,
                                    BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null,
                                    new object[] {Server}, CultureInfo.CurrentCulture));
                    }
                    catch (Exception e)
                    {
                        Logger.WriteLine(LogType.Error, $"Couldn't load assembly '{Path.GetFileName(file.Name)}': {e.InnerException}");
                    }
                }
                else
                    Logger.WriteLine(LogType.Error, $"Assembly '{Path.GetFileName(file.Name)}' is not a valid server plugin.");
            }
        }

        private void RegisterPlugin(ServerPlugin plugin)
        {
            plugins.Add(plugin);
            plugin.Load();
        }
    }
}