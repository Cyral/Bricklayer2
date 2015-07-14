using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Net.Messages;

namespace Bricklayer.Core.Client
{
    /// <summary>
    /// Client events.
    /// </summary>
    public class EventManager
    {
        /// <summary>
        /// Events related to the main game.
        /// </summary>
        public GameEvents Game { get; }


        /// <summary>
        /// Events related to network
        /// </summary>
        public NetEvents Network { get; }

        /// <summary>
        /// Events related to the main game client.
        /// </summary>
        public sealed class GameEvents
        {
            //Arguments define what values are passed to the event handler(s).
            #region Arguments

            public class GameStateEventArgs : BricklayerEventArgs
            {
                public GameState NewState { get; private set; }
                public GameState OldState { get; private set; }

                public GameStateEventArgs(GameState newState, GameState oldState)
                {
                    NewState = newState;
                    OldState = oldState;
                }
            }

            public class GameScreenEventArgs : BricklayerEventArgs
            {
                public Screen NewScreen { get; private set; }
                public Screen OldScreen { get; private set; }

                public GameScreenEventArgs(Screen newScreen, Screen oldScreen)
                {
                    NewScreen = newScreen;
                    OldScreen = oldScreen;
                }
            }
            #endregion

            //Events represent a collection of event handlers.
            //(Note: These are not standard .NET events, see the Event class)

            #region Events

            /// <summary>
            /// When the state of the game is changed. (From login to lobby, server list to game screen, etc.)
            /// </summary>
            public Event<GameStateEventArgs> StateChanged { get; } = new Event<GameStateEventArgs>();

            /// <summary>
            /// When the current UI screen of the game is changed. (Example: From login to in-game)
            /// </summary>
            public Event<GameScreenEventArgs> ScreenChanged { get; } = new Event<GameScreenEventArgs>();
            #endregion
        }

        /// <summary>
        /// Events related to the network
        /// </summary>
        public sealed class NetEvents
        {
            /// <summary>
            /// Events related to the authentication server and proccess.
            /// </summary>
            internal AuthServerEvents Auth { get; }

            /// <summary>
            /// Events related to networking from the game server.
            /// </summary>
            public GameServerEvents Game { get; }


            /// <summary>
            /// Events related to the authentication server and proccess.
            /// </summary>
            internal sealed class AuthServerEvents
            {
                //Arguments define what values are passed to the event handler(s).
                #region Arguments

                public class InitEventArgs : BricklayerEventArgs
                {
                    public string UUID { get; private set; }
                    public string Username { get; private set; }
                    internal string PrivateKey { get; private set; }
                    public string PublicKey { get; private set; }

                    public InitEventArgs(string username, string uuid, string privateKey, string publicKey)
                    {
                        Username = username;
                        UUID = uuid;
                        PrivateKey = privateKey;
                        PublicKey = publicKey;
                    }
                }

                public class FailedLoginEventArgs : BricklayerEventArgs
                {
                    public string ErrorMessage { get; private set; }

                    public FailedLoginEventArgs(string errorMessage)
                    {
                        ErrorMessage = errorMessage;
                    }
                }

                public class VerifiedEventArgs : BricklayerEventArgs
                {
                    public bool Verified { get; private set; }

                    public VerifiedEventArgs(bool verified)
                    {
                        Verified = verified;
                    }
                }

                public class PluginDownloadEventArgs : BricklayerEventArgs
                {
                    public PluginDownloadMessage Message { get; private set; }

                    public PluginDownloadEventArgs(PluginDownloadMessage message)
                    {
                        Message = message;
                    }
                }
                #endregion

                //Events represent a collection of event handlers.
                //(Note: These are not standard .NET events, see the Event class)

                #region Events

                /// <summary>
                /// When client recieves Init message from Auth server, containing the user's name, UUID, and session key-pair.
                /// </summary>
                public Event<InitEventArgs> InitReceived { get; } =
                    new Event<InitEventArgs>();

                /// <summary>
                /// When client recieves a FailedLogin message from the Auth server, meaning invalid credentials.
                /// </summary>
                public Event<FailedLoginEventArgs> FailedLogin { get; } =
                    new Event<FailedLoginEventArgs>();

                /// <summary>
                /// When client recieves verification of session from the Auth server
                /// </summary>
                public Event<VerifiedEventArgs> Verified { get; } =
                    new Event<VerifiedEventArgs>();

                /// <summary>
                /// When the auth server tells the client to download a mod. (From the install button on the website)
                /// </summary>
                public Event<PluginDownloadEventArgs> PluginDownloadRequested { get; } = new Event<PluginDownloadEventArgs>();


                #endregion
            }

            /// <summary>
            /// Events related to networking from the game server.
            /// </summary>
            public sealed class GameServerEvents
            {
                //Arguments define what values are passed to the event handler(s).

                #region Arguments

                public class DisconnectEventArgs : BricklayerEventArgs
                {
                    public string Reason { get; private set; }

                    public DisconnectEventArgs(string reason)
                    {
                        Reason = reason;
                    }
                }

                public class ConnectEventArgs : BricklayerEventArgs
                {
                }

                public class InitEventArgs : BricklayerEventArgs
                {
                    public InitMessage Message { get; private set; }

                    public InitEventArgs(InitMessage message)
                    {
                        Message = message;
                    }
                }

                public class BannerEventArgs : BricklayerEventArgs
                {
                    public byte[] Banner { get; private set; }

                    public BannerEventArgs(byte[] banner)
                    {
                        Banner = banner;
                    }
                }

                public class ServerInfoEventArgs : BricklayerEventArgs
                {
                    public string Description { get; private set; }
                    public int Players { get; private set; }
                    public int MaxPlayers { get; private set; }
                    public IPEndPoint Host { get; private set; }

                    public ServerInfoEventArgs(string description, int players, int maxPlayers, IPEndPoint host)
                    {
                        Description = description;
                        Players = players;
                        MaxPlayers = maxPlayers;
                        Host = host;
                    }
                }


                public class LatencyUpdatedEventArgs : BricklayerEventArgs
                {
                    public float Ping { get; private set; }

                    public LatencyUpdatedEventArgs(float ping)
                    {
                        Ping = ping;
                    }
                }

                #endregion

                //Events represent a collection of event handlers.
                //(Note: These are not standard .NET events, see the Event class)

                #region Events

                /// <summary>
                /// When the client is disconnected from the game server.
                /// </summary>
                public Event<DisconnectEventArgs> Disconnected { get; } = new Event<DisconnectEventArgs>();

                /// <summary>
                /// When the client is fully connected to a game server.
                /// </summary>
                public Event<ConnectEventArgs> Connected { get; } = new Event<ConnectEventArgs>();

                /// <summary>
                /// When an init message is recieved, containing list of levels and server info. (On connect and when reload button is pressed)
                /// </summary>
                public Event<InitEventArgs> InitReceived { get; } = new Event<InitEventArgs>();

                /// <summary>
                /// When the connection latency (ping) is updated after a successful ping/pong message.
                /// </summary>
                public Event<LatencyUpdatedEventArgs> LatencyUpdated { get; } = new Event<LatencyUpdatedEventArgs>();

                /// <summary>
                /// When server sends player info to display on serverlist.
                /// </summary>
                public Event<ServerInfoEventArgs> ServerInfoReceived { get; } = new Event<ServerInfoEventArgs>();

                /// <summary>
                /// When server sends player lobby banner.
                /// </summary>
                public Event<BannerEventArgs> LobbyBannerReceived { get; } = new Event<BannerEventArgs>();

                #endregion
            }


            internal NetEvents()
            {
                Game = new GameServerEvents();
                Auth = new AuthServerEvents();
            }
        }

        internal EventManager()
        {
            Game = new GameEvents();
            Network = new NetEvents();
        }
    }
}
