using Bricklayer.Core.Server;

namespace Bricklayer.Plugins.Blocks.Server
{
    /// <summary>
    /// S part of the default blocks plugin.
    /// </summary>
    public class Plugin : ServerPlugin
    {
        public Plugin(Core.Server.Server host) : base(host)
        {
        }

        public override void Load()
        {
            Common.Blocks.AddBlocks();
        }

        protected override void Unload()
        {

        }
    }
}