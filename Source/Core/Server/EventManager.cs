using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Common;
using Lidgren.Network;

namespace Bricklayer.Core.Server
{
    /// <summary>
    /// Server events.
    /// </summary>
    public class EventManager
    {
        /// <summary>
        /// Events related to player connections
        /// </summary>
        public ConnectionEvents Connection { get; }

        /// <summary>
        /// Events related to the main game client.
        /// </summary>
        public sealed class ConnectionEvents
        {

            //Arguments define what values are passed to the event handler(s).
            #region Arguments
            public class PreLoginEventArgs : EventArgs
            {
                public string Username { get; private set; }
                public int Id { get; private set; }
                public string PublicKey { get; private set; }
                public NetConnection Connection { get; private set; }

                public PreLoginEventArgs(string username, int id, string publicKey, NetConnection connection)
                {
                    Username = username;
                    Id = id;
                    PublicKey = publicKey;
                    Connection = connection;
                }
            }

            public class ConnectionEventArgs : EventArgs
            {
                public string Username { get; private set; }

                public ConnectionEventArgs(string username)
                {
                    Username = username;
                }
            }

            public class DisconnectionEventArgs : EventArgs
            {
                public string Username { get; private set; }
                public string Reason { get; private set; }

                public DisconnectionEventArgs(string username, string reason)
                {
                    Username = username;
                    Reason = reason;
                }
            }


            public class ValidSessionEventArgs : EventArgs
            {
                public string Username { get; private set; }
                public int Id { get; private set; }

                public ValidSessionEventArgs(string username, int id )
                {
                    Username = username;
                    Id = id;
                }
            }

            public class InvalidSessionEventArgs : EventArgs
            {
                public string Username { get; private set; }
                public int Id { get; private set; }

                public InvalidSessionEventArgs(string username, int id)
                {
                    Username = username;
                    Id = id;
                }
            }
            #endregion

            //Events represent a collection of event handlers.
            //(Note: These are not standard .NET events, see the Event class)
            #region Events

            /// <summary>
            /// When a player requests to join with a public key
            /// </summary>
            public Event<PreLoginEventArgs> PreLogin { get; } = new Event<PreLoginEventArgs>();

            /// <summary>
            /// When a player fully connects 
            /// </summary>
            public Event<ConnectionEventArgs> Connection { get; } = new Event<ConnectionEventArgs>();

            /// <summary>
            /// When a player disconnects 
            /// </summary>
            public Event<DisconnectionEventArgs> Disconnection { get; } = new Event<DisconnectionEventArgs>();

            /// <summary>
            /// When Auth server sends back validation of user session
            /// </summary>
            public Event<ValidSessionEventArgs> Valid { get; } = new Event<ValidSessionEventArgs>();

            /// <summary>
            /// When Auth server sends back invalidation of user session
            /// </summary>
            public Event<InvalidSessionEventArgs> Invalid { get; } = new Event<InvalidSessionEventArgs>();
            #endregion
        }

        internal EventManager()
        {
            Connection = new ConnectionEvents();
        }
    }
}
