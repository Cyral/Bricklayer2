using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Common.Data;
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
        public List<LevelData> Levels { get; set; }

        public InitMessage(NetIncomingMessage im, MessageContext context)
        {
            Levels = new List<LevelData>();
            Context = context;
            Decode(im);
        }

        public InitMessage(string serverName, string description, string intro, int online, List<LevelData> levels)
        {
            ServerName = serverName;
            Description = description;
            Intro = intro;
            Levels = levels;
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
            int levelsLength = im.ReadByte();
            for (var i = 0; i < levelsLength; i++)
            {
                Levels.Add(new LevelData(new PlayerData(im.ReadString(), Guid.Parse(im.ReadString())), im.ReadString(), Guid.Parse(im.ReadString()), im.ReadString(), im.ReadByte(), im.ReadInt16(), im.ReadDouble()));
            }
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(ServerName);
            om.Write(Description);
            om.Write(Intro);
            om.Write(Online);
            om.Write((byte)Levels.Count);
            foreach (var data in Levels)
            {
                om.Write(data.Creator.Username);
                om.Write(data.Creator.UUID.ToString());
                om.Write(data.Name);
                om.Write(data.UUID.ToString());
                om.Write(data.Description);
                om.Write((byte)data.Online);
                om.Write((short)data.Plays);
                om.Write(data.Rating);
            }
        }

        #endregion
    }
}
