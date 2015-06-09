using System;
using System.Collections.Generic;
using Lidgren.Network;

namespace Bricklayer.Core.Common
{
    /// <summary>
    /// Represents a user connected to the server or client
    /// </summary>
    public class User : UserData
    {
        /// <summary>
        /// Represents an anonymous user
        /// </summary>
        public bool IsGuest { get; set; }

        /// <summary>
        /// The numerical ID of the user, for keeping track of connections
        /// </summary>
        public short ID { get; set; }

        /// <summary>
        /// The user's underlying Lidgren connection
        /// </summary>
        public NetConnection Connection { get; set; }

        public User(string username, NetConnection connection, short id)
        {
            Username = username;
            Connection = connection;
            ID = id;
            DatabaseID = -1;
        }
    }
}
