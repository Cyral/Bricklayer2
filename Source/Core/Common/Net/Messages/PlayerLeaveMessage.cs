using System;
using Bricklayer.Core.Common.Data;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Message sent from server broadcasting about a player leaving.
    /// </summary>
    public class PlayerLeaveMessage : IMessage
    {
        public Guid Player { get; private set; }

        public PlayerLeaveMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public PlayerLeaveMessage(PlayerData player)
        {
            Player = player.UUID;
        }

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.PlayerLeave;

        public void Decode(NetIncomingMessage im)
        {
            Player = im.ReadGuid();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Player);
        }
    }
}