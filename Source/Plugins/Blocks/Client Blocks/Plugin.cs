using Bricklayer.Core.Client;

namespace Bricklayer.Plugins.Blocks.Client
{
    /// <summary>
    /// Client part of the default blocks plugin.
    /// </summary>
    public class Plugin : ClientPlugin
    {
        public Plugin(Core.Client.Client host) : base(host) {}

        public override void Load()
        {
            Common.Blocks.AddBlocks();
        }

        protected override void Unload() {}
    }
}