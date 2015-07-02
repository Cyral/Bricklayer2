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
        /// Indicates if the user is not signed in.
        /// </summary>
        public bool IsGuest { get; private set; }

        /// <summary>
        /// The numerical ID of the user, for keeping track of connections.
        /// </summary>
        public short ID { get; private set; }

        /// <summary>
        /// The user's underlying network connection.
        /// </summary>
        public NetConnection Connection { get; private set; }

        public User(string username, NetConnection connection, short id)
        {
            Username = username;
            Connection = connection;
            ID = id;
            UUID = string.Empty;
        }
    }
}
