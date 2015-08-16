using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Response sent to auth server. (Such as to acknoledge a message was received)
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

        /// <summary>
        /// Possible responses.
        /// </summary>
        public enum PingResponse : byte
        {
            /// <summary>
            /// The receiver received the plugin message successfully.
            /// </summary>
            GotPlugin
        }
    }
}
