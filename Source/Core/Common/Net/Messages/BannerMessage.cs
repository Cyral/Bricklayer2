using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Server.Data;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Sends lobby banner to client
    /// Server => Client
    /// </summary>
    public class BannerMessage : IMessage
    {
        public double MessageTime { get; set; }
        public byte[] Banner { get; set; }

        public BannerMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public BannerMessage(byte[] banner)
        {
            MessageTime = NetTime.Now;
            Banner = banner;
        }


        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.Banner;

        public void Decode(NetIncomingMessage im)
        {
            int bannerLength = im.ReadInt32();
            im.SkipPadBits();
            Banner = im.ReadBytes(bannerLength);
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Banner.Length);
            om.WritePadBits();
            om.Write(Banner);
        }

        #endregion
    }
}
