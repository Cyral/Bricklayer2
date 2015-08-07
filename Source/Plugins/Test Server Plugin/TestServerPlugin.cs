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

        private Random random = new Random();
        public override void Load()
        {
            Server.Commands.AddCommand(
                Command.Create("Test", "test", "A test command!")
                    .SetAction(((arguments, o) => { Console.WriteLine("Hello!"); })));

            /* Server.Events.Connection.Connection.AddHandler(
                args => { Logger.WriteLine(LogType.Plugin, $"{args.Username} has joined!"); }); */

            Logger.WriteLine(LogType.Plugin, "Test Plugin Loaded!");

            var t = new Timer(10);
            t.Elapsed += T_Elapsed;
            t.Start();
        }

        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var level in Server.Levels)
            {
                var x = random.Next(1, 80);
                var y = random.Next(1, 40);

                level.Tiles[x, y, 1] = new Tile(Blocks.Default);
                Server.Net.Broadcast(level, new BlockPlaceMessage(x, y, 1, Blocks.Default));
            }
        }

        protected override void Unload()
        {
            Logger.WriteLine(LogType.Plugin, "Test Plugin Unloaded!");
        }
    }
}