using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Net;
using Bricklayer.Core.Common.Net.Messages;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Server;

namespace Bricklayer.Plugins.TestServerPlugin
{
    public class TestPluginMessage : PluginMessage
    {
        public string Test { get; private set; }

        public TestPluginMessage(string test, TestServerPlugin plugin) : base("TestPluginMessage", plugin, plugin.Server.Plugins.PluginMessages)
        {
            Test = test;
        }

        public TestPluginMessage(NetIncomingMessage im, MessageContext context) : base(im, context) { }

        public override void Decode(NetIncomingMessage im)
        {
            Test = im.ReadString();
        }

        public override void Encode(NetOutgoingMessage om)
        {
            om.Write(Test);
        }

    }
}
