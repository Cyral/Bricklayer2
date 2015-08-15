using System;
using System.Diagnostics;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Data
{
    /// <summary>
    /// Player information not associated with a map or game.
    /// </summary>
    public class PlayerData
    {
        public PlayerData(string name, Guid uuid, bool isGuest = false)
        {
            Username = name;
            UUID = uuid;
            IsGuest = isGuest;
        }

        /// <summary>
        /// Indicates if the user is not signed in.
        /// </summary>
        public bool IsGuest { get; private set; }

        /// <summary>
        /// The user's unique identifier.
        /// </summary>
        public Guid UUID { get; private set; }

        /// <summary>
        /// The name of the player.
        /// </summary>
        public string Username { get; set; }

        internal PlayerData(NetIncomingMessage im)
        {
            IsGuest = im.ReadBoolean();
            UUID = Guid.Parse(im.ReadString());
            Username = im.ReadString();
        }

        internal virtual void Encode(NetOutgoingMessage om)
        {
            om.Write(IsGuest);
            om.Write(UUID.ToString());
            om.Write(Username);
        }

    }
}
