using System;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Data
{
    /// <summary>
    /// Basic data for a level.
    /// </summary>
    /// <remarks>
    /// This is the data stored in the database and displayed in the lobby window.
    /// </remarks>
    public class LevelData
    {
        /// <summary>
        /// The name of this level.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The description of this level.
        /// </summary>
        public string Description { get; protected set; }

        /// <summary>
        /// The number of players currently online this level.
        /// </summary>
        public virtual int Online { get; protected set; }
        
        /// <summary>
        /// The rating, from 0 to 5, of this level.
        /// </summary>
        public int Rating { get; protected set; }

        /// <summary>
        /// The number of times this level has been played. (Non unique)
        /// </summary>
        public int Plays { get; set; }

        /// <summary>
        /// The unique ID of this level.
        /// </summary>
        public Guid UUID { get; protected set; }

        /// <summary>
        /// The creator of this level
        /// </summary>
        public PlayerData Creator { get; protected set; }

        public LevelData(PlayerData creator, string name, Guid uuid, string description, int players, int plays, int rating)
        {
            Creator = creator;
            Name = name;
            UUID = uuid;
            Description = description;
            Online = players;
            Rating = rating;
            Plays = plays;
        }

        internal LevelData(NetIncomingMessage im)
        {
            Creator = new PlayerData(im.ReadString(), Guid.Parse(im.ReadString()));
            Name = im.ReadString();
            UUID = Guid.Parse(im.ReadString());
            Description = im.ReadString();
            Online = im.ReadByte();
            Plays = im.ReadInt16();
            Rating = im.ReadInt32();
        }


        /// <summary>
        /// Writes the relevant level data to a messgae.
        /// </summary>
        internal virtual void Encode(NetOutgoingMessage om)
        {
            om.Write(Creator.Username);
            om.Write(Creator.UUID.ToString());
            om.Write(Name);
            om.Write(UUID.ToString());
            om.Write(Description);
            om.Write((byte)Online);
            om.Write((short)Plays);
            om.Write(Rating);
        }
    }
}
