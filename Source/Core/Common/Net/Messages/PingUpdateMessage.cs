using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Common.World;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    public class PingUpdateMessage : IMessage
    {
        public Dictionary<Guid, int> Pings { get; private set; }

        public PingUpdateMessage(Level level)
        {
            Pings = new Dictionary<Guid, int>();
            foreach (var player in level.Players)
            {
                Pings.Add(player.UUID, (int) player.Connection.AverageRoundtripTime*1000);
            }
        }

        public PingUpdateMessage(NetIncomingMessage im, MessageContext context)
        {
            Pings = new Dictionary<Guid, int>();
            Context = context;
            Decode(im);
        }


        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.PingUpdate;

        public void Decode(NetIncomingMessage im)
        {
            var count = im.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                var guid = Guid.Parse(im.ReadString());
                var ping = im.ReadInt32();

                Pings.Add(guid, ping);
            }
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Pings.Count);
            foreach (var i in Pings)
            {
                om.Write(i.Key.ToString("N"));
                om.Write(i.Value);
            }
        }

        #endregion
    }
}
