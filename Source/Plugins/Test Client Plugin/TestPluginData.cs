using Bricklayer.Core.Client;
using MonoForce.Controls;
using Console = System.Console;

namespace Bricklayer.Plugins.TestClientPlugin
{
    /// <summary>
    /// A test Client plugin.
    /// </summary>
    public class TestPluginData : ClientPlugin
    {
        public TestPluginData(Client host) : base(host) {}

        public override void Load()
        {
            // Demo window
            var window = new Window(Client.UI);
            window.Init();
            window.SetSize(300, 120);
            window.Text = "Test Plugin";

            var textbox = new TextBox(Client.UI) { Text = "This window was created\n by a loaded plugin!"};
            textbox.Init();
            textbox.Mode = TextBoxMode.Multiline;
            textbox.SetPosition(8, 8);
            textbox.SetSize(window.ClientWidth - 16, window.ClientHeight - 16);
            window.Add(textbox);

            Client.UI.Add(window);

            Console.WriteLine("Test Plugin Loaded!");
        }

        protected override void Unload()
        {
            Console.WriteLine("Test Plugin Unloaded!");
        }
    }
}