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
            public LevelEvents Level { get; }

            internal GameEvents()
            {
                Level = new LevelEvents();
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
                    public Level Level { get; private set; }

                    public CreateLevelEventArgs(Level level)
                    {
                        Level = level;
                    }
                }

                public class BlockPlacedEventArgs : BricklayerEventArgs
                {
                    /// <summary>
                    /// The type of block placed.
                    /// </summary>
                    public BlockType Type { get; private set; }

                    /// <summary>
                    /// Previous type of block.
                    /// </summary>
                    public BlockType OldType { get; private set; }

                    /// <summary>
                    /// X grid coordinate.
                    /// </summary>
                    public int X { get; private set; }

                    /// <summary>
                    /// Y grid coordinate.
                    /// </summary>
                    public int Y { get; private set; }

                    public Layer Layer { get; private set; }

                    /// <summary>
                    /// Z grid coordinate. (Layer)
                    /// </summary>
                    public int Z => (int)Layer;

                    /// <summary>
                    /// Player who placed block. Null if placed by plugin or game.
                    /// </summary>
                    public Player Sender { get; private set; }

                    public Level Level { get; }

                    public BlockPlacedEventArgs(Player sender, int x, int y, int z, BlockType newType, BlockType oldType) : this(x, y, z, newType, oldType)
                    {
                        Sender = sender;
                        Level = sender.Level;
                    }

                    public BlockPlacedEventArgs(Level level, int x, int y, int z, BlockType newType, BlockType oldType) : this(x, y, z, newType, oldType)
                    {
                        Level = level;
                    }

                    private BlockPlacedEventArgs(int x, int y, int z, BlockType newType, BlockType oldType)
                    {
                        X = x;
                        Y = y;
                        Layer = (Layer)z;
                        Type = newType;
                        OldType = oldType;
                    }

                    public BlockPlacedEventArgs(Player sender, int x, int y, Layer layer, BlockType newType, BlockType oldType)
                        : this(sender, x, y, (int)layer, newType, oldType)
                    { }
                }

                /// <summary>
                /// When a new level is created.
                /// </summary>
                /// <remarks>
                /// For when a new level is requested to be made, see the corresponding Network event.
                /// </remarks>
                public Event<CreateLevelEventArgs> LevelCreated { get; } = new Event<CreateLevelEventArgs>();

                /// <summary>
                /// When a block is placed.
                /// </summary>
                public Event<BlockPlacedEventArgs> BlockPlaced { get; } = new Event<BlockPlacedEventArgs>();
            }

            #endregion

            #region Arguments

            public class PluginStatusEventArgs : BricklayerEventArgs
            {
                public ServerPlugin Plugin { get; private set; }

                public bool Enabled => Plugin.IsEnabled;

                public PluginStatusEventArgs(ServerPlugin plugin)
                {
                    Plugin = plugin;
                }
            }

            #endregion

            #region Events 

            /// <summary>
            /// When a plugin is loaded, enabled, or disabled.
            /// </summary>
            public Event<PluginStatusEventArgs> PluginStatusChanged { get; } = new Event<PluginStatusEventArgs>();

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
                public byte[] PublicKey { get; private set; }
                public string Username { get; private set; }
                public Guid UUID { get; private set; }

                public LoginRequestEventArgs(string username, Guid uuid, byte[] publicKey, NetConnection connection)
                {
                    Username = username;
                    UUID = uuid;
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

                public SessionEventArgs(string username, Guid uuid, bool valid)
                {
                    Username = username;
                    UUID = uuid;
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

            public class RatingEventArgs : BricklayerEventArgs
            {
                public Guid Level { get; private set; }
                public int Rating { get; private set; }
                public Player Sender { get; private set; }

                public RatingEventArgs(Guid level, Player sender, int rating)
                {
                    Level = level;
                    Sender = sender;
                    Rating = rating;
                }
            }

            public class BlockPlacedEventArgs : BricklayerEventArgs
            {
                /// <summary>
                /// The type of block placed.
                /// </summary>
                public BlockType Type { get; private set; }

                /// <summary>
                /// X grid coordinate.
                /// </summary>
                public int X { get; private set; }

                /// <summary>
                /// Y grid coordinate.
                /// </summary>
                public int Y { get; private set; }

                public Layer Layer { get; private set; }

                /// <summary>
                /// Z grid coordinate. (Layer)
                /// </summary>
                public int Z => (int)Layer;

                /// <summary>
                /// Player who placed block. Null if placed by plugin or game.
                /// </summary>
                public Player Sender { get; private set; }

                public Level Level => Sender.Level;

                public BlockPlacedEventArgs(Player sender, int x, int y, int z, BlockType type)
                {
                    Sender = sender;
                    X = x;
                    Y = y;
                    Layer = (Layer)z;
                    Type = type;
                }

                public BlockPlacedEventArgs(Player sender, int x, int y, Layer layer, BlockType type)
                    : this(sender, x, y, (int)layer, type)
                { }
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
            /// When the server receives a chat message.
            /// </summary>
            public Event<ChatEventArgs> ChatMessageReceived { get; } = new Event<ChatEventArgs>();

            /// <summary>
            /// When the server receives a block place message.
            /// </summary>
            public Event<BlockPlacedEventArgs> BlockPlaceMessageReceived { get; } = new Event<BlockPlacedEventArgs>();

            /// <summary>
            /// When the server receives a rating for a level from a player
            /// </summary>
            public Event<RatingEventArgs> RatingMessageReceived { get; } = new Event<RatingEventArgs>();

            #endregion
        }

        #endregion
    }
}