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
    class TestPlugin : ServerPlugin
    {
        public override string Name { get; } = "Test Server Plugin";
        public override string Description { get; } = "A plugin for testing!";
        public override string Author { get; } = "Bricklayer Devs";
        public override Version Version { get; } = new Version(0, 1, 0, 0);

        public TestPlugin(Server host) : base(host)
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
