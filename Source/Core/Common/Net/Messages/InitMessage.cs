using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Server.Data;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Sends initial server data to the client.
    /// Server => Client
    /// </summary>
    public class InitMessage : IMessage
    {
        public double MessageTime { get; set; }
        public string ServerName { get; set; }
        public string Description { get; set; }
        public string Intro { get; set; }
        public int Online { get; set; }
        public List<LobbySaveData> Rooms { get; set; }

        public InitMessage(NetIncomingMessage im, MessageContext context)
        {
            Rooms = new List<LobbySaveData>();
            Context = context;
            Decode(im);
        }

        public InitMessage(string serverName, string description, string intro, int online, List<LobbySaveData> rooms)
        {
            ServerName = serverName;
            Description = description;
            Intro = intro;
            Rooms = rooms;
            Online = (byte)online;
            MessageTime = NetTime.Now;
        }


        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.Init;

        public void Decode(NetIncomingMessage im)
        {
            ServerName = im.ReadString();
            Description = im.ReadString();
            Intro = im.ReadString();
            Online = im.ReadInt32();
            int roomsLength = im.ReadByte();
            for (var i = 0; i < roomsLength; i++)
            {
                Rooms.Add(new LobbySaveData(im.ReadString(), im.ReadInt16(), im.ReadString(), im.ReadByte(), im.ReadInt16(), im.ReadDouble()));
            }
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(ServerName);
            om.Write(Description);
            om.Write(Intro);
            om.Write(Online);
            om.Write((byte)Rooms.Count);
            foreach (var data in Rooms)
            {
                om.Write(data.Name);
                om.Write((short)data.ID);
                om.Write(data.Description);
                om.Write((byte)data.Online);
                om.Write((short)data.Plays);
                om.Write(data.Rating);
            }
        }

        #endregion
    }
}
