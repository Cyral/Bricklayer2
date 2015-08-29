using System;
using Bricklayer.Core.Common.Net;
using Bricklayer.Core.Server;
using Pyratron.Frameworks.Commands.Parser;
using Pyratron.Frameworks.LogConsole;

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
            Server.Plugins.PluginMessages.AddMessage("TestPluginMessage");

            Server.Commands.AddCommand(
                Command.Create("Test", "test", "A test command!")
                    .SetAction(((arguments, o) => { Console.WriteLine("Hello!"); })));

            Server.Events.Network.PluginMessageReceived.AddHandler(args =>
            {
                var plugin = args.Identifier;

                switch (Server.Plugins.PluginMessages.GetType(args.Id))
                {
                    case "TestPluginMessage":
                    {
                        var msg = new TestPluginMessage(args.IncomingMessage, MessageContext.Server);
                        Logger.Log($"Message Recieved from {plugin}: {msg.Test}");
                        break;
                    }
                }
            });

            Logger.Log("Test Plugin Loaded!");
        }

        public override void Unload()
        {
            Server.Commands.Commands.RemoveAll(c => c.Name.Equals("Test"));

            Logger.Log("Test Plugin Unloaded!");
        }
    }
}