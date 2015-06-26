using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
     /// <summary>
    /// Contains the public key and the username that goes with it. 
    /// Client sends this to Game server and Game server sends this to the Auth server to confirm.
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
            Description = im.ReadString();
            Players = im.ReadInt32();
            MaxPlayers = im.ReadInt32();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Description);
            om.Write(Players);
            om.Write(MaxPlayers);
        }

        #endregion
    }
}
