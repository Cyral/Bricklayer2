using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Bricklayer.Core.Client
{
    /// <summary>
    /// Represents a configuration for the server.
    /// </summary>
    public sealed class Config
    {
        /// <summary>
        /// The configuration for the client settings, such as username, smiley color, etc.
        /// </summary>
        public ClientConfig Client;

        public static Config GenerateDefaultConfig()
        {
            return new Config
            {
                Client = ClientConfig.GenerateDefaultConfig(),
            };
        }
    }

    /// <summary>
    /// Represents the server specific configuration elements
    /// </summary>
    public sealed class ClientConfig
    {
        /// <summary>
        /// The resolution, in pixels, of the game window
        /// </summary>
        public Microsoft.Xna.Framework.Point Resolution;

        /// <summary>
        /// The current username to be used.
        /// </summary>
        public string Username;

        /// <summary>
        /// The password of the saved login.
        /// </summary>
        public string Password;

        /// <summary>
        /// Indicates if the login is remembered.
        /// </summary>
        public bool RememberMe;

        /// <summary>
        /// The hue, 0-360, that the player should be tinted
        /// </summary>
        public int Color;

        /// <summary>
        /// The server address to use for authentication.
        /// </summary>
        public string AuthServerAddress;

        /// <summary>
        /// The server port for authentication.
        /// </summary>
        public int AuthServerPort;

        public static ClientConfig GenerateDefaultConfig()
        {
            return new ClientConfig
            {
                RememberMe = true,
                Password = string.Empty,
                Resolution = new Point(0, 0), // Empty on default, which tells the client to go fullscreen (Windowed)
                Username = "Guest",
                Color = 40,
                AuthServerAddress = Common.Globals.Values.DefaultAuthAddress,
                AuthServerPort = Common.Globals.Values.DefaultAuthPort,
            };
        }
    }
}
