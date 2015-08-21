using Bricklayer.Core.Common.Net;
using Bricklayer.Core.Common.Net.Messages;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Plugins.TestClientPlugin
{
    public class TestPluginMessage : PluginMessage
    {
        public string Test { get; private set; }

        public TestPluginMessage(string test, TestClientPlugin plugin) : base("TestPluginMessage", plugin, plugin.Client.Plugins.Pluginmessages)
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
