using System.Collections.Generic;
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
        public string ServerName { get; private set; }
        public string Description { get; private set; }
        public string Intro { get; private set; }
        public int Online { get; private set; }
        public List<LevelData> Levels { get; }
        public PluginMessages PluginMessages { get; private set; } 

        public InitMessage(NetIncomingMessage im, MessageContext context)
        {
            Levels = new List<LevelData>();
            Context = context;
            Decode(im);
        }

        public InitMessage(string serverName, string description, string intro, int online, List<LevelData> levels, PluginMessages pluginMessages)
        {
            ServerName = serverName;
            Description = description;
            Intro = intro;
            Levels = levels;
            Online = (byte)online;
            PluginMessages = pluginMessages;
        }


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
                Levels.Add(new LevelData(im)); // The level class will handle decoding the message.
            }

            if (!im.ReadBoolean()) return;

            int pmLength = im.ReadByte();
            var pluginMessages = new List<string>();
            for (var i = 0; i < pmLength; i++)
            {
                pluginMessages.Add(im.ReadString());
            }
            PluginMessages = new PluginMessages(pluginMessages);
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
                data.Encode(om);
            }

            if (PluginMessages?.MessageTypes.Count > 0)
            { 
                om.Write(true);
                om.Write((byte) PluginMessages.MessageTypes.Count);
                foreach (var data in PluginMessages.MessageTypes)
                {
                    om.Write(data);
                }
            }
            else
            {
                om.Write(false);
            }
        }
    }
}
