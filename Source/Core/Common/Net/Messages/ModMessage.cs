using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Sends mod info to client
    /// Auth Server => Client
    /// </summary>
    public class ModMessage : IMessage
    {
        public string Id { get; set; }
        public string ModName { get; set; }
        public string FileName { get; set; }

        public ModMessage(NetIncomingMessage im, MessageContext context)
        {
            Context = context;
            Decode(im);
        }

        public ModMessage(string id, string modName, string fileName)
        {
            Id = id;
            ModName = modName;
            FileName = fileName;
        }

        #region IMessage Members

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.Mod;

        public void Decode(NetIncomingMessage im)
        {
            Id = im.ReadString();
            ModName = im.ReadString();
            FileName = im.ReadString();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Id);
            om.Write(ModName);
            om.Write(FileName);
        }

        #endregion
    }
}
