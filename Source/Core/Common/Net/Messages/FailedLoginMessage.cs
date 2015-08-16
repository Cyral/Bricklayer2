using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Message sent from server to inform client of wrong login credentials.
    /// String: Error message
    /// </summary>
    public class FailedLoginMessage : IMessage
    {
        /// <summary>
        /// Reason for failed login.
        /// </summary>
        public string ErrorMessage { get; private set; }

        public FailedLoginMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public MessageContext Context { get; set; }

        public MessageTypes MessageType => MessageTypes.FailedLogin;

        public void Decode(NetIncomingMessage im)
        {
            ErrorMessage = im.ReadString();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(ErrorMessage);
        }
    }
}