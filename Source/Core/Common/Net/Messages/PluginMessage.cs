using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    public abstract class PluginMessage : IMessage
    {
        public PluginMessage(string type, PluginData plugin, PluginMessages pluginMessages)
        {
            this.type = type;
            identifier = plugin.Identifier;
            id = pluginMessages.GetId(type);
        }

        public PluginMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        private int id;
        private string identifier;
        private string type;
        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.PluginMessage;

        public abstract void Decode(NetIncomingMessage im);

        public abstract void Encode(NetOutgoingMessage om);

        /// <summary>
        /// Only used in Client to encode main properties (id, identifier, type) before the overriden Encode
        /// </summary>
        internal void EncodeProperties(NetOutgoingMessage om)
        {
            om.Write(id);
            om.Write(identifier);
            om.Write(type);
        }
    }
}