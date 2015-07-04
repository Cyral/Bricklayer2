using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Server;
using Pyratron.Frameworks.Commands.Parser;

namespace Bricklayer.Plugins.TestPlugin
{
    /// <summary>
    /// A test server plugin.
    /// </summary>
    public class TestServerPlugin : ServerPlugin
    {
        public TestServerPlugin(Server host) : base(host)
        {

        }

        public override void Load()
        {
            Server.Commands.AddCommand(Command.Create("Test", "test", "A test command!").SetAction(((arguments, o) =>
            {
                Console.WriteLine("Hello!");
            })));
            Logger.WriteLine(LogType.Plugin, "Test Plugin Loaded!");
        }

        protected override void Unload()
        {
            Logger.WriteLine(LogType.Plugin, "Test Plugin Unloaded!");
        }
    }
}
