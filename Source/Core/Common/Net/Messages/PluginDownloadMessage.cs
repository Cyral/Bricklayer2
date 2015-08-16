using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Sends plugin info to client (to be automatically downloaded)
    /// Auth Server => Client
    /// Int: ID
    /// String: Mod Name
    /// String: File Name
    /// </summary>
    public class PluginDownloadMessage : IMessage
    {
        public int ID { get; private set; }
        public string ModName { get; private set; }
        public string FileName { get; private set; }

        public PluginDownloadMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.PluginDownload;

        public void Decode(NetIncomingMessage im)
        {
            ID = im.ReadInt32();
            ModName = im.ReadString();
            FileName = im.ReadString();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(ID);
            om.Write(ModName);
            om.Write(FileName);
        }
    }
}
