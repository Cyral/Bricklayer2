using Bricklayer.Core.Common.Entity;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Message sent from server broadcasting about a player joining
    /// </summary>
    public class PlayerJoinMessage : IMessage
    {
        public Player Player { get; set; }

        public PlayerJoinMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public PlayerJoinMessage(Player player)
        {
            Player = player;
        }

        #region IMessage Members

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

        #endregion
    }
}
