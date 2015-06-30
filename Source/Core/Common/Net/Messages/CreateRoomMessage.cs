using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    public class CreateRoomMessage : IMessage
    {
        public const int MaxNameLength = 24, MaxDescriptionLength = 80, MaxDescriptionLines = 2;

        public string Name { get; set; }

        public string Description { get; set; }

        public MessageContext Context { get; set; }

        public MessageTypes MessageType
        {
            get { return MessageTypes.CreateRoom; }
        }

        public CreateRoomMessage(NetIncomingMessage im)
        {
            this.Decode(im);
        }

        public CreateRoomMessage(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }
        public void Decode(NetIncomingMessage im)
        {
            this.Name = im.ReadString();
            this.Description = im.ReadString();
        }
        public void Encode(NetOutgoingMessage om)
        {
            om.Write(this.Name);
            om.Write(this.Description);
        }
    }
}
