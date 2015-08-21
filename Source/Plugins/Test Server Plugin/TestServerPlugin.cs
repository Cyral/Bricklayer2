using System;
using System.Timers;
using Bricklayer.Core.Common.Net.Messages;
using Bricklayer.Core.Common.World;
using Bricklayer.Core.Server;
using Bricklayer.Plugins.DefaultBlocks.Common;
using Pyratron.Frameworks.Commands.Parser;
using Bricklayer.Core.Common.Net;

namespace Bricklayer.Plugins.TestServerPlugin
{
    /// <summary>
    /// A test server plugin.
    /// </summary> what about these errors
    public class TestServerPlugin : ServerPlugin
    {
        public TestServerPlugin(Server host) : base(host) {}

        public override void Load()
        {
            Server.Plugins.PluginMessages.AddMessage("TestPluginMessage");

            Server.Commands.AddCommand(
                Command.Create("Test", "test", "A test command!")
                    .SetAction(((arguments, o) => { Console.WriteLine("Hello!"); })));

            Server.Events.Network.PluginMessageReceived.AddHandler(args =>
            {
                var plugin = args.Identifier;
                
                switch(Server.Plugins.PluginMessages.GetType(args.Id)) {
                    case "TestPluginMessage":
                    {
                            var msg = new TestPluginMessage(args.IncomingMessage, MessageContext.Server);
                            Logger.WriteLine($"Message Recieved from {plugin}: {msg.Test}");
                            break;
                    }
                }
            });

            Logger.WriteLine(LogType.Plugin, "Test Plugin Loaded!");
        }

        public override void Unload()
        {
            Server.Commands.Commands.RemoveAll(c => c.Name.Equals("Test"));

            Logger.WriteLine(LogType.Plugin, "Test Plugin Unloaded!");
        }
    }
}