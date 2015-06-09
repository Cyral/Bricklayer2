﻿using System;
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
            //Delegates define the arguments/structure of each event.
            #region Delegates

            public delegate void GameStateEventHandler(GameState newState, GameState oldState);
            #endregion

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
            public Event<GameStateEventHandler, GameStateEventArgs> StateChanged { get; } = new Event<GameStateEventHandler, GameStateEventArgs>();
            #endregion
        }

        /// <summary>
        /// Events related to the network
        /// </summary>
        public sealed class NetEvents
        {
            /// <summary>
            /// Events related to the Auth Server
            /// </summary>
            internal sealed class AuthEvents
            {
                //Delegates define the arguments/structure of each event.

                #region Delegates

                public delegate void InitEventHandler(int databaseID, string privateKey, string publicKey);
                public delegate void FailedLoginEventHandler(string errorMessage);
                public delegate void VerifiedEventHandler(bool verified);

                #endregion

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
                public Event<InitEventHandler, InitEventArgs> Init { get; } =
                    new Event<InitEventHandler, InitEventArgs>();

                /// <summary>
                /// When client recieves a FailedLogin message from the Auth server
                /// </summary>
                public Event<FailedLoginEventHandler, FailedLoginEventArgs> FailedLogin { get; } =
                    new Event<FailedLoginEventHandler, FailedLoginEventArgs>();

                /// <summary>
                /// When client recieves verification of session from the Auth server
                /// </summary>
                public Event<VerifiedEventHandler, VerifiedEventArgs> Verified { get; } =
                    new Event<VerifiedEventHandler, VerifiedEventArgs>();


                #endregion
            }

            /// <summary>
            /// Events related to the Game Server
            /// </summary>
            public sealed class GameServerEvents
            {
                //Delegates define the arguments/structure of each event.

                #region Delegates

                public delegate void InitEventHandler();

                #endregion

                //Arguments define what values are passed to the event handler(s).

                #region Arguments

                public class InitEventArgs : EventArgs
                {

                    public InitEventArgs()
                    {

                    }
                }

                #endregion

                //Events represent a collection of event handlers.
                //(Note: These are not standard .NET events, see the Event class)

                #region Events

                /// <summary>
                /// When client recieves Init message from Auth server
                /// </summary>
                public Event<InitEventHandler, InitEventArgs> Init { get; } =
                    new Event<InitEventHandler, InitEventArgs>();

                #endregion
            }
        }

        internal EventManager()
        {
            Game = new GameEvents();
            Network = new NetEvents();
        }
    }
}