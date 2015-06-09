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
         
        internal EventManager()
        {
            Game = new GameEvents();
        }
    }
}
