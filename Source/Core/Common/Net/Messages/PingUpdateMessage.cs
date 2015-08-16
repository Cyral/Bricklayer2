using System;
using System.Collections.Generic;
using Bricklayer.Core.Common.World;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Send ping times to all users in a room.
    /// Server => Client.
    /// For each user:
    ///   Byte(16): GUID
    ///   UShort: Ping Time (ms)
    /// </summary>
    public class PingUpdateMessage : IMessage
    {
        public Dictionary<Guid, int> Pings { get; private set; }

        public PingUpdateMessage(Level level)
        {
            Pings = new Dictionary<Guid, int>();
            foreach (var player in level.Players)
            {
                Pings.Add(player.UUID, (int)(player.Connection.AverageRoundtripTime*1000));
            }
        }

        public PingUpdateMessage(NetIncomingMessage im, MessageContext context)
        {
            Pings = new Dictionary<Guid, int>();
            Context = context;
            Decode(im);
        }

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.PingUpdate;

        public void Decode(NetIncomingMessage im)
        {
            var count = im.ReadInt32();

            for (var i = 0; i < count; i++)
                Pings.Add(im.ReadGuid(), im.ReadUInt16());
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Pings.Count);
            foreach (var ping in Pings)
            {
                om.Write(ping.Key);
                om.Write((ushort)ping.Value);
            }
        }
    }
}
