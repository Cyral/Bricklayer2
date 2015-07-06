using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bricklayer.Core.Common;

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

        /// <summary>
        /// Loads all plugins that are not already loaded.
        /// </summary>
        internal void LoadPlugins()
        {
            //Get a list of all the .dlls in the directory
            IEnumerable<PluginData> files = null;
            try
            {
                files = IOHelper.GetPlugins(Server.IO.PluginsDirectory, Server.IO.SerializationSettings);
            }
            catch (Exception e)
            {
                Logger.WriteLine(LogType.Error, e.Message);
            }
            if (files != null)
            {
                foreach (var file in files.Where(file => !plugins.Contains(file)))
                {
                    //TODO: Use AppDomains for security
                    //Load the assembly
                    try
                    {
                        var asm = IOHelper.LoadPlugin(AppDomain.CurrentDomain, file.Path);
                        RegisterPlugin(IOHelper.CreatePluginInstance<ServerPlugin>(asm, Server, file));
                    }
                    catch (Exception e)
                    {
                        Logger.WriteLine(LogType.Error, e.Message);
                    }
                }
            }
        }

        private void RegisterPlugin(ServerPlugin plugin)
        {
            plugins.Add(plugin);
            plugin.Load();
        }
    }
}