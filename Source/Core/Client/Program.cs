/*
 * Bricklayer Client
 * Copyright (C) 2015 (See LICENSE file)
 * https://github.com/Pyratron/Bricklayer
 */

using System;

namespace Bricklayer.Core.Client
{
    #if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the Bricklayer Client.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Create an instance of the game and run it.
            using (var game = new Game())
                game.Run();
        }
    }
    #endif
}
