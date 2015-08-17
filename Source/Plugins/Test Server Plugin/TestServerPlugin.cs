using System;
using System.Timers;
using Bricklayer.Core.Common.Net.Messages;
using Bricklayer.Core.Common.World;
using Bricklayer.Core.Server;
using Bricklayer.Plugins.DefaultBlocks.Common;
using Pyratron.Frameworks.Commands.Parser;

namespace Bricklayer.Plugins.TestServerPlugin
{
    /// <summary>
    /// A test server plugin.
    /// </summary>
    public class TestServerPlugin : ServerPlugin
    {
        public TestServerPlugin(Server host) : base(host) {}

        public override void Load()
        {
            Server.Commands.AddCommand(
                Command.Create("Test", "test", "A test command!")
                    .SetAction(((arguments, o) => { Console.WriteLine("Hello!"); })));

            Logger.WriteLine(LogType.Plugin, "Test Plugin Loaded!");
        }

        public override void Unload()
        {
            Server.Commands.Commands.RemoveAll(c => c.Name.Equals("Test"));

            Logger.WriteLine(LogType.Plugin, "Test Plugin Unloaded!");
        }
    }
}