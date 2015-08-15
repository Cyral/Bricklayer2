using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Sent to the server to create a new level.
    /// Client => Server
    /// String: Name
    /// String: Description
    /// </summary>
    public class CreateLevelMessage : IMessage
    {
        public const int MaxNameLength = 24, MaxDescriptionLength = 128, MaxDescriptionLines = 2;
        public string Description { get; set; }
        public string Name { get; set; }

        public CreateLevelMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public CreateLevelMessage(string name, string description)
        {
            Name = name;
            Description = description;
        }

        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.CreateLevel;

        public void Decode(NetIncomingMessage im)
        {
            Name = im.ReadString().Truncate(MaxNameLength);
            Description = im.ReadString().Truncate(MaxDescriptionLength);
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Name);
            om.Write(Description);
        }

        #endregion
    }
}