using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Sent from the client to server to request server information while on the server list. ("Pinging")
    /// Sent from the server to client to repond with such information.
    /// </summary>
    public class ServerInfoMessage : IMessage
    {
        public double MessageTime { get; set; }

        public string Description { get; set; }
        public int Players { get; set; }
        public int MaxPlayers { get; set; }

        public ServerInfoMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public ServerInfoMessage() {}

        public ServerInfoMessage(string description, int players, int maxPlayers)
        {
            Description = description;
            Players = players;
            MaxPlayers = maxPlayers;
            MessageTime = NetTime.Now;
        }

        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.ServerInfo;

        public void Decode(NetIncomingMessage im)
        {
            // Actual data is only sent from server to client
            if (Context == MessageContext.Client)
            {
                Description = im.ReadString();
                Players = im.ReadInt32();
                MaxPlayers = im.ReadInt32();
            }
        }

        public void Encode(NetOutgoingMessage om)
        {
            if (Context == MessageContext.Server)
            {
                om.Write(Description);
                om.Write(Players);
                om.Write(MaxPlayers);
            }
        }

        #endregion
    }
}
