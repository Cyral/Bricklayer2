using Bricklayer.Core.Client;
using Bricklayer.Core.Client.Interface.Screens;
using MonoForce.Controls;
using Console = System.Console;

namespace Bricklayer.Plugins.TestClientPlugin
{
    /// <summary>
    /// A test Client plugin.
    /// </summary>
    public class TestClientPlugin : ClientPlugin
    {
        private Window window;

        public TestClientPlugin(Client host) : base(host) {}

        public override void Load()
        {
            // Demo window.
            window = new Window(Client.UI);
            window.Init();
            window.SetSize(300, 120);
            window.Text = "Test Plugin";

            var textbox = new TextBox(Client.UI) { Text = "This window was created\n by a loaded plugin!"};
            textbox.Init();
            textbox.Mode = TextBoxMode.Multiline;
            textbox.SetSize(window.ClientWidth - 16, window.ClientHeight - 16);
            textbox.SetPosition(8, 8);
            window.Add(textbox);
            window.SetPosition(8,8);

            Client.UI.Add(window);

           var  d = new Derp(Client);

            Console.WriteLine("Test Plugin Loaded!");
        }

        public override void Unload()
        {
            Client.UI.Remove(window);
            Console.WriteLine("Test Plugin Unloaded!");
        }
    }

    public class Derp
    {
        public Derp(Client client)
        {

            client.Events.Game.Level.BlockPlaced.AddHandler(args =>
            {
                Console.Write(args.Type);
            });

        }
    }
}