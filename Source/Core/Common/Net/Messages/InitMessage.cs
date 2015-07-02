using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Server.Data;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
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
            this.ServerName = serverName;
            this.Description = description;
            this.Intro = intro;
            this.Rooms = rooms;
            this.Online = (byte)online;
            MessageTime = NetTime.Now;
        }


        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.Init;

        public void Decode(NetIncomingMessage im)
        {
            this.ServerName = im.ReadString();
            this.Description = im.ReadString();
            this.Intro = im.ReadString();
            this.Online = im.ReadInt32();
            int roomsLength = im.ReadByte();
            for (int i = 0; i < roomsLength; i++)
            {
                Rooms.Add(new LobbySaveData(im.ReadString(), im.ReadInt16(), im.ReadString(), im.ReadByte(), im.ReadInt16(), im.ReadDouble()));
            }
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(this.ServerName);
            om.Write(this.Description);
            om.Write(this.Intro);
            om.Write(this.Online);
            om.Write((byte)Rooms.Count);
            for (int i = 0; i < Rooms.Count; i++)
            {
                om.Write(Rooms[i].Name);
                om.Write((short)Rooms[i].ID);
                om.Write(Rooms[i].Description);
                om.Write((byte)Rooms[i].Online);
                om.Write((short)Rooms[i].Plays);
                om.Write(Rooms[i].Rating);
            }
        }

        #endregion
    }
}
