using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Common;

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
        /// Events related to the main game client.
        /// </summary>
        public sealed class GameEvents
        {
            //Delegates define the arguments/structure of each event.
            #region Delegates

            #endregion

            //Arguments define what values are passed to the event handler(s).
            #region Arguments

            #endregion

            //Events represent a collection of event handlers.
            //(Note: These are not standard .NET events, see the Event class)
            #region Events

            #endregion
        }

        internal EventManager()
        {
            Game = new GameEvents();
        }
    }
}
