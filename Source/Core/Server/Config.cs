using Bricklayer.Core.Common;

namespace Bricklayer.Core.Server
{
    /// <summary>
    /// Represents a configuration for the server.
    /// </summary>
    public sealed class Config
    {
        /// <summary>
        /// The configuration for database settings. By default, SQLite is used (bundled with Bricklayer)
        /// </summary>
        public DatabaseConfig Database;

        /// <summary>
        /// The configuration for the server settings, such as port number and max connections.
        /// </summary>
        public ServerConfig Server;

        public static Config GenerateDefaultConfig()
        {
            return new Config
            {
                Server = ServerConfig.GenerateDefaultConfig(),
                Database = DatabaseConfig.GenerateDefaultConfig()
            };
        }
    }

    /// <summary>
    /// Represents the database specific configuration elements.
    /// </summary>
    public sealed class DatabaseConfig
    {
        /// <summary>
        /// The DB connection string used to connect to the DB.
        /// </summary>
        /// <example>
        /// SQLite: Data Source=c:\mydb.db;Version=3;
        /// MySQL: Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;
        /// See http://www.connectionstrings.com for the appropriate connection string for your database provider.
        /// </example>
        public string Connection;

        /// <summary>
        /// The database provider/system used.
        /// </summary>
        public string Provider;

        public static DatabaseConfig GenerateDefaultConfig()
        {
            return new DatabaseConfig
            {
                // Bricklayer uses SQLite by default
                Provider = "System.Data.SQLite",
                Connection = "Data Source = database.sqlite; Version = 3"
            };
        }
    }

    /// <summary>
    /// Represents the server specific configuration elements
    /// </summary>
    public sealed class ServerConfig
    {
        /// <summary>
        /// The server address to use for authentication.
        /// </summary>
        public string AuthServerAddress;

        /// <summary>
        /// The server port for authentication.
        /// </summary>
        public int AuthServerPort;

        /// <summary>
        /// The "Message of the day/Description", to be shown in the server list.
        /// </summary>
        public string Decription;

        /// <summary>
        /// The extended MOTD, possibly showing news, stats, etc., displayed in the lobby.
        /// </summary>
        public string Intro;

        /// <summary>
        /// The maximum allowed players allowed to connect to the server. (not per level)
        /// </summary>
        public int MaxPlayers;

        /// <summary>
        /// The name of the server.
        /// </summary>
        public string Name;

        /// <summary>
        /// The port the server should run on.
        /// </summary>
        public int Port;

        public static ServerConfig GenerateDefaultConfig()
        {
            return new ServerConfig
            {
                Port = Globals.Values.DefaultServerPort,
                AuthServerAddress = Globals.Values.DefaultAuthAddress,
                AuthServerPort = Globals.Values.DefaultAuthPort,
                MaxPlayers = 64,
                Name = "Bricklayer Server",
                Decription =
                    "A Bricklayer Server running on the default configuration.\nEdit this message in the config file.",
                Intro =
                    "[color:Gold]Welcome to our $Name![/color] [color:DodgerBlue]We currently have $Online player(s) in $Levels level(s).[/color]\n\nExtended information about your server can go here!"
            };
        }
    }
}