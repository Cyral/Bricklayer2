using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bricklayer.Core.Common;

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
        public int PluginCount => plugins.Count;

        private readonly List<ClientPlugin> plugins;

        public PluginComponent(Client client) : base(client)
        {
            plugins = new List<ClientPlugin>();
        }

        public override async Task Init()
        {
            if (!Client.IO.Initialized)
                throw new InvalidOperationException("The IO component must be initialized first.");
            LoadPlugins();
            await base.Init();
        }

        private void LoadPlugins()
        {
            //Get a list of all the .dlls in the directory
            var files = IOHelper.GetPlugins(Client.IO.Directories["Plugins"], Client.IO.SerializationSettings);
            foreach (var file in files)
            {
                //TODO: Use AppDomains for security
                //Load the assembly
                try
                {
                    var asm = IOHelper.LoadPlugin(AppDomain.CurrentDomain, file.Path);
                    RegisterPlugin(IOHelper.CreatePluginInstance<ClientPlugin>(asm, Client, file));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private void RegisterPlugin(ClientPlugin plugin)
        {
            plugins.Add(plugin);
            plugin.Load();
        }
    }
}