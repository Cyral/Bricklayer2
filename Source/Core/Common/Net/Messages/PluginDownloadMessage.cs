using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Sends plugin info to client
    /// Auth Server => Client
    /// Int: ID
    /// String: Mod Name
    /// String: File Name
    /// </summary>
    public class PluginDownloadMessage : IMessage
    {
        public int ID { get; set; }
        public string ModName { get; set; }
        public string FileName { get; set; }

        public PluginDownloadMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        #region IMessage Members

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

        #endregion
    }
}
