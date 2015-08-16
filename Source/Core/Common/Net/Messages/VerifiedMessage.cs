using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// What the auth server will send back when it has checked if the user's public key matches the stored public key to join
    /// a game server.
    /// </summary>
    public class VerifiedMessage : IMessage
    {
        public bool Verified { get; private set; }

        public VerifiedMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.Verified;

        public void Decode(NetIncomingMessage im)
        {
            Verified = im.ReadBoolean();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Verified);
        }
    }
}