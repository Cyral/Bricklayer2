using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Client
{
    /// <summary>
    /// Represents a state of the game
    /// </summary>
    public enum GameState
    {
        Login,
        Servers,
        Lobby,
        Game,
    }
}
