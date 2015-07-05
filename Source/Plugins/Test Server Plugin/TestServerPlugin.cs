using System;
using Bricklayer.Core.Server;
using Pyratron.Frameworks.Commands.Parser;

namespace Bricklayer.Plugins.TestServerPlugin
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
            Server.Commands.AddCommand(
                Command.Create("Test", "test", "A test command!")
                    .SetAction(((arguments, o) => { Console.WriteLine("Hello!"); })));

            /* Server.Events.Connection.Connection.AddHandler(
                args => { Logger.WriteLine(LogType.Plugin, $"{args.Username} has joined!"); }); */

            Logger.WriteLine(LogType.Plugin, "Test Plugin Loaded!");
        }

        protected override void Unload()
        {
            Logger.WriteLine(LogType.Plugin, "Test Plugin Unloaded!");
        }
    }
}