using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Entity;
using Bricklayer.Core.Common.Net;
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
                public string UUID { get; private set; }
                public string PublicKey { get; private set; }
                public NetConnection Connection { get; private set; }

                public PreLoginEventArgs(string username, string uuid, string publicKey, NetConnection connection)
                {
                    Username = username;
                    UUID = uuid;
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
                public string UUID { get; private set; }

                public ValidSessionEventArgs(string username, string uuid)
                {
                    Username = username;
                    UUID = uuid;
                }
            }

            public class InvalidSessionEventArgs : EventArgs
            {
                public string Username { get; private set; }
                public string UUID { get; private set; }

                public InvalidSessionEventArgs(string username, string uuid)
                {
                    Username = username;
                    UUID = uuid;
                }
            }


            public class RequestInfoEventArgs : EventArgs
            {
                public IPEndPoint Host { get; private set; }

                public RequestInfoEventArgs(IPEndPoint host)
                {
                    Host = host;
                }
            }

            public class RequestMessageEventArgs : EventArgs
            {
                /// <summary>
                /// The type of message requested.
                /// </summary>
                public MessageTypes Type { get; private set; }

                /// <summary>
                /// The user who requested the message.
                /// </summary>
                public Player Sender { get; set; }

                public RequestMessageEventArgs(MessageTypes type, Player sender)
                {
                    Type = type;
                    Sender = sender;
                }

            }
            #endregion

            //Events represent a collection of event handlers.
            //(Note: These are not standard .NET events, see the Event class)
            #region Events

            /// <summary>
            /// When a player requests to join with a public key.
            /// </summary>
            public Event<PreLoginEventArgs> PreLogin { get; } = new Event<PreLoginEventArgs>();

            /// <summary>
            /// When a player fully connects.
            /// </summary>
            public Event<ConnectionEventArgs> Connection { get; } = new Event<ConnectionEventArgs>();

            /// <summary>
            /// When a player disconnects.
            /// </summary>
            public Event<DisconnectionEventArgs> Disconnection { get; } = new Event<DisconnectionEventArgs>();

            /// <summary>
            /// When Auth server sends back validation of user session.
            /// </summary>
            public Event<ValidSessionEventArgs> Valid { get; } = new Event<ValidSessionEventArgs>();

            /// <summary>
            /// When Auth server sends back invalidation of user session.
            /// </summary>
            public Event<InvalidSessionEventArgs> Invalid { get; } = new Event<InvalidSessionEventArgs>();

            /// <summary>
            /// When client pings the server for information for the serverlist.
            /// </summary>
            public Event<RequestInfoEventArgs> RequestInfo { get; } = new Event<RequestInfoEventArgs>();

            /// <summary>
            /// When the client request the server sends a specific message back.
            /// </summary>
            public Event<RequestMessageEventArgs> RequestMessage { get; } = new Event<RequestMessageEventArgs>(); 
            #endregion
        }

        internal EventManager()
        {
            Connection = new ConnectionEvents();
        }
    }
}
