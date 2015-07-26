using System;
using System.Net;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Entity;
using Bricklayer.Core.Common.Net;
using Bricklayer.Core.Common.World;
using Lidgren.Network;

namespace Bricklayer.Core.Server
{
    /// <summary>
    /// Server events.
    /// </summary>
    public class EventManager
    {
        /// <summary>
        /// Events related to the main game.
        /// </summary>
        public GameEvents Game { get; }

        /// <summary>
        /// Events related to player connections and networking.
        /// </summary>
        public NetEvents Network { get; }

        internal EventManager()
        {
            Network = new NetEvents();
            Game = new GameEvents();
        }

        #region Nested type: Class

        /// <summary>
        /// Events related to the main game.
        /// </summary>
        public sealed class GameEvents
        {
            /// <summary>
            /// Events related to level management.
            /// </summary>
            public LevelEvents Levels { get; }

            internal GameEvents()
            {
                Levels = new LevelEvents();
            }

            #region Nested type: Class

            /// <summary>
            /// Events related to level management.
            /// </summary>
            public sealed class LevelEvents
            {
                public class CreateLevelEventArgs : BricklayerEventArgs
                {
                    /// <summary>
                    /// The level that was created.
                    /// </summary>
                    public Level Level { get; internal set; }

                    public CreateLevelEventArgs(Level level)
                    {
                        Level = level;
                    }
                }

                /// <summary>
                /// When a new level is created.
                /// </summary>
                /// <remarks>
                /// For when a new level is requested to be made, see the corresponding Network event.
                /// </remarks>
                public Event<CreateLevelEventArgs> LevelCreated { get; } = new Event<CreateLevelEventArgs>();
            }

            #endregion
        }

        #endregion

        #region Nested type: Class

        /// <summary>
        /// Events related to connections and networking.
        /// </summary>
        public sealed class NetEvents
        {
            // Arguments define what values are passed to the event handler(s).

            #region Arguments

            public class LoginRequestEventArgs : BricklayerEventArgs
            {
                /// <summary>
                /// The connection that sent the login request.
                /// </summary>
                public NetConnection Connection { get; private set; }

                /// <summary>
                /// The public key to be verified.
                /// </summary>
                public string PublicKey { get; private set; }
                public string Username { get; private set; }
                public Guid UUID { get; private set; }

                public LoginRequestEventArgs(string username, string uuid, string publicKey, NetConnection connection)
                {
                    Username = username;
                    UUID = Guid.Parse(uuid);
                    PublicKey = publicKey;
                    Connection = connection;
                }
            }

            public class ConnectionEventArgs : BricklayerEventArgs
            {
                /// <summary>
                /// The player who has connected.
                /// </summary>
                public Player Player { get; private set; }

                public ConnectionEventArgs(Player player)
                {
                    Player = player;
                }
            }

            public class DisconnectionEventArgs : BricklayerEventArgs
            {
                /// <summary>
                /// The reason for this user disconnecting.
                /// </summary>
                public string Reason { get; private set; }

                /// <summary>
                /// The player who was disconnected.
                /// </summary>
                public Player Player { get; private set; }

                public DisconnectionEventArgs(Player player, string reason)
                {
                    Player = player;
                    Reason = reason;
                }
            }


            public class SessionEventArgs : BricklayerEventArgs
            {
                public string Username { get; private set; }
                public Guid UUID { get; private set; }

                /// <summary>
                /// Indicates if the session sent is valid or not.
                /// </summary>
                public bool Valid { get; private set; }

                public SessionEventArgs(string username, string uuid, bool valid)
                {
                    Username = username;
                    UUID = Guid.Parse(uuid);
                    Valid = valid;
                }
            }

            public class RequestInfoEventArgs : BricklayerEventArgs
            {
                /// <summary>
                /// The endpoint that requested the server information.
                /// </summary>
                public IPEndPoint Host { get; private set; }

                public RequestInfoEventArgs(IPEndPoint host)
                {
                    Host = host;
                }
            }

            public class RequestMessageEventArgs : BricklayerEventArgs
            {
                /// <summary>
                /// The user who requested the message.
                /// </summary>
                public Player Sender { get; private set; }

                /// <summary>
                /// The type of message requested.
                /// </summary>
                public MessageTypes Type { get; private set; }

                public RequestMessageEventArgs(MessageTypes type, Player sender)
                {
                    Type = type;
                    Sender = sender;
                }
            }

            public class JoinLevelEventArgs : BricklayerEventArgs
            {
                public Player Sender { get; private set; }

                /// <summary>
                /// The UUID of the level requested to join.
                /// </summary>
                public Guid UUID { get; private set; }

                public JoinLevelEventArgs(Player sender, Guid uuid)
                {
                    Sender = sender;
                    UUID = uuid;
                }
            }

            public class CreateLevelEventArgs : BricklayerEventArgs
            {
                public string Description { get; private set; }
                public string Name { get; private set; }
                public Player Sender { get; private set; }

                public CreateLevelEventArgs(Player sender, string name, string description)
                {
                    Sender = sender;
                    Name = name;
                    Description = description;
                }
            }

            public class ChatEventArgs : BricklayerEventArgs
            {
                public string Message { get; private set; }
                public Player Sender { get; private set; }

                public ChatEventArgs(Player sender, string message)
                {
                    Sender = sender;
                    Message = message;
                }
            }

            #endregion

            // Events represent a collection of event handlers.
            // (Note: These are not standard .NET events, see the Event class)

            #region Events

            /// <summary>
            /// When a player requests to join with a public key.
            /// </summary>
            public Event<LoginRequestEventArgs> UserLoginRequested { get; } = new Event<LoginRequestEventArgs>();

            /// <summary>
            /// When a player fully connects. (After verifying their session is valid)
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

            /// <summary>
            /// When the server receives a request from a player to join a level.
            /// </summary>
            public Event<JoinLevelEventArgs> JoinLevelMessageRecieved { get; } = new Event<JoinLevelEventArgs>();

            /// <summary>
            /// When the server receives a request from a player to create a new level.
            /// </summary>
            public Event<CreateLevelEventArgs> CreateLevelMessageRecieved { get; } = new Event<CreateLevelEventArgs>();

            /// <summary>
            /// When the server receives a chat message
            /// </summary>
            public Event<ChatEventArgs> ChatMessageReceived { get; } = new Event<ChatEventArgs>();

            #endregion
        }

        #endregion
    }
}