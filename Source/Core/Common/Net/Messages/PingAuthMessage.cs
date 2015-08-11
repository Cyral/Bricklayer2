using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Send response to Auth server
    /// Client => Auth Server
    /// Byte: Response Type
    /// String: Data
    /// </summary>
    public class PingAuthMessage : IMessage
    {
        public PingResponse Response { get; set; }
        public string Info { get; set; }

        public PingAuthMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public PingAuthMessage(PingResponse response, string info)
        {
            Response = response;
            Info = info;
        }

        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.PingAuth;

        public void Decode(NetIncomingMessage im)
        {
            Response = (PingResponse)im.ReadByte();
            Info = im.ReadString();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write((byte)Response);
            om.Write(Info);
        }

        public enum PingResponse : byte
        {
            GotPlugin
        }

        #endregion
    }
}
