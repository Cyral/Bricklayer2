using Bricklayer.Core.Common.Entity;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Message sent from server broadcasting about a player leaving.
    /// </summary>
    public class PlayerLeaveMessage : IMessage
    {
        public Player Player { get; set; }

        public PlayerLeaveMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public PlayerLeaveMessage(Player player)
        {
            Player = player;
        }

        public MessageContext Context { get; set; }

        public MessageTypes MessageType => MessageTypes.PlayerJoin;

        public void Decode(NetIncomingMessage im)
        {
            Player = new Player(im);
        }

        public void Encode(NetOutgoingMessage om)
        {
            Player.Encode(om);
        }
    }
}
