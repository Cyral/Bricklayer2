using System;
using Bricklayer.Core.Common;

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

            public class GameStateEventArgs : EventArgs
            {
                public GameState NewState { get; private set; }
                public GameState OldState { get; private set; }

                public GameStateEventArgs(GameState newState, GameState oldState)
                {
                    NewState = newState;
                    OldState = oldState;
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

                public class InitEventArgs : EventArgs
                {
                    public int DatabaseId { get; private set; }
                    public string PrivateKey { get; private set; }
                    public string PublicKey { get; private set; }

                    public InitEventArgs(int databaseId, string privateKey, string publicKey)
                    {
                        DatabaseId = databaseId;
                        PrivateKey = privateKey;
                        PublicKey = publicKey;
                    }
                }

                public class FailedLoginEventArgs : EventArgs
                {
                    public string ErrorMessage { get; private set; }

                    public FailedLoginEventArgs(string errorMessage)
                    {
                        ErrorMessage = errorMessage;
                    }
                }

                public class VerifiedEventArgs : EventArgs
                {
                    public bool Verified { get; private set; }

                    public VerifiedEventArgs(bool verified)
                    {
                        Verified = verified;
                    }
                }

                #endregion

                //Events represent a collection of event handlers.
                //(Note: These are not standard .NET events, see the Event class)

                #region Events

                /// <summary>
                /// When client recieves Init message from Auth server
                /// </summary>
                public Event<InitEventArgs> Init { get; } =
                    new Event<InitEventArgs>();

                /// <summary>
                /// When client recieves a FailedLogin message from the Auth server
                /// </summary>
                public Event<FailedLoginEventArgs> FailedLogin { get; } =
                    new Event<FailedLoginEventArgs>();

                /// <summary>
                /// When client recieves verification of session from the Auth server
                /// </summary>
                public Event<VerifiedEventArgs> Verified { get; } =
                    new Event<VerifiedEventArgs>();


                #endregion
            }

            /// <summary>
            /// Events related to networking from the game server.
            /// </summary>
            public sealed class GameServerEvents
            {
                //Arguments define what values are passed to the event handler(s).

                #region Arguments

                public class InitEventArgs : EventArgs
                {
                    public InitEventArgs()
                    {

                    }
                }

                public class DisconnectEventArgs : EventArgs
                {
                    public string Reason { get; private set; }

                    public DisconnectEventArgs(string reason)
                    {
                        Reason = reason;
                    }
                }

                public class ConnectEventArgs : EventArgs
                {
                    public ConnectEventArgs()
                    {

                    }
                }

                public class LatencyUpdatedEventArgs : EventArgs
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
                /// When client recieves Init message from the game server.
                /// </summary>
                public Event<InitEventArgs> Init { get; } = new Event<InitEventArgs>();

                /// <summary>
                /// When the client is disconnected from the game server.
                /// </summary>
                public Event<DisconnectEventArgs> Disconnect { get; } = new Event<DisconnectEventArgs>();

                /// <summary>
                /// When the client is fully connected to a game server
                /// </summary>
                public Event<ConnectEventArgs> Connect { get; } = new Event<ConnectEventArgs>();

                /// <summary>
                /// When the connection latency (ping) is updated after a successful ping/pong message.
                /// </summary>
                public Event<LatencyUpdatedEventArgs> LatencyUpdated { get; } = new Event<LatencyUpdatedEventArgs>(); 

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
