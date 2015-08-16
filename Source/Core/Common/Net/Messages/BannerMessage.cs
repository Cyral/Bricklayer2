using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Sends lobby banner to client.
    /// Server => Client
    /// </summary>
    public class BannerMessage : IMessage
    {
        public byte[] Banner { get; private set; }

        public BannerMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public BannerMessage(byte[] banner)
        {
            Banner = banner;
        }

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.Banner;

        public void Decode(NetIncomingMessage im)
        {
            var bannerLength = im.ReadInt32();
            im.SkipPadBits();
            Banner = im.ReadBytes(bannerLength);
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Banner.Length);
            om.WritePadBits();
            om.Write(Banner);
        }
    }
}