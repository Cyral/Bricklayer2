using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Data
{
    /// <summary>
    /// Player information not associated with a map or game.
    /// </summary>
    public class PlayerData
    {
        public PlayerData(string name, short id, bool isGuest)
        {
            Username = name;
        }

        /// <summary>
        /// Indicates if the user is not signed in.
        /// </summary>
        public bool IsGuest { get; private set; }

        /// <summary>
        /// The numerical ID of the user, for keeping track of connections.
        /// </summary>
        public short ID { get; private set; }

        /// <summary>
        /// The name of the player.
        /// </summary>
        public string Username { get; set; }
    }
}
