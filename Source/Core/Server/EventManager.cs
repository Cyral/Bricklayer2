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
        /// Events related to player connections and networking.
        /// </summary>
        public NetEvents Network { get; }

        /// <summary>
        /// Events related to the main game client.
        /// </summary>
        public sealed class NetEvents
        {

            //Arguments define what values are passed to the event handler(s).
            #region Arguments
            public class PreLoginEventArgs : BricklayerEventArgs
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

            public class ConnectionEventArgs : BricklayerEventArgs
            {
                public string Username { get; private set; }

                public ConnectionEventArgs(string username)
                {
                    Username = username;
                }
            }

            public class DisconnectionEventArgs : BricklayerEventArgs
            {
                public string Username { get; private set; }
                public string Reason { get; private set; }

                public DisconnectionEventArgs(string username, string reason)
                {
                    Username = username;
                    Reason = reason;
                }
            }


            public class SessionEventArgs : BricklayerEventArgs
            {
                public string Username { get; private set; }
                public string UUID { get; private set; }
                public bool Valid { get; set; }

                public SessionEventArgs(string username, string uuid, bool valid)
                {
                    Username = username;
                    UUID = uuid;
                    Valid = valid;
                }
            }

            public class RequestInfoEventArgs : BricklayerEventArgs
            {
                public IPEndPoint Host { get; private set; }

                public RequestInfoEventArgs(IPEndPoint host)
                {
                    Host = host;
                }
            }

            public class RequestMessageEventArgs : BricklayerEventArgs
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
            public Event<PreLoginEventArgs> UserLoginRequested { get; } = new Event<PreLoginEventArgs>();

            /// <summary>
            /// When a player fully connects.
            /// </summary>
            public Event<ConnectionEventArgs> UserConnected { get; } = new Event<ConnectionEventArgs>();

            /// <summary>
            /// When a player disconnects.
            /// </summary>
            public Event<DisconnectionEventArgs> UserDisconnected { get; } = new Event<DisconnectionEventArgs>();

            /// <summary>
            /// When Auth server sends back a response on whether the user's session is valid or not. (See the Valid argument)
            /// </summary>
            public Event<SessionEventArgs> SessionValidated { get; } = new Event<SessionEventArgs>();

            /// <summary>
            /// When client pings the server for information for the serverlist.
            /// </summary>
            public Event<RequestInfoEventArgs> InfoRequested { get; } = new Event<RequestInfoEventArgs>();

            /// <summary>
            /// When the client request the server sends a specific message back.
            /// </summary>
            public Event<RequestMessageEventArgs> MessageRequested { get; } = new Event<RequestMessageEventArgs>(); 
            #endregion
        }

        internal EventManager()
        {
            Network = new NetEvents();
        }
    }
}
