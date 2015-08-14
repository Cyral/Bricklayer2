using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Data;

namespace Bricklayer.Core.Server.Components
{
    /// <summary>
    /// Handles user authentication and other database functions
    /// </summary>
    public class DatabaseComponent : ServerComponent
    {
        protected override LogType LogType => LogType.Database;
        private string connectionString;
        private DbProviderFactory providerFactory;

        public DatabaseComponent(Server server) : base(server) {}

        /// <summary>
        /// Adds the specified named parameters to the command.
        /// </summary>
        public void AddParamaters(DbCommand command, Dictionary<string, string> parameters)
        {
            foreach (var param in parameters)
            {
                var comParam = command.CreateParameter();
                comParam.ParameterName = "@" + param.Key;
                comParam.Value = param.Value;
                command.Parameters.Add(comParam);
            }
        }

        /// <summary>
        /// Gets a list of all levels in the database.
        /// </summary>
        public async Task<List<LevelData>> GetAllLevels()
        {
            var levels = new List<LevelData>();
            var command = providerFactory.CreateCommand();
            if (command == null)
                return levels;
            // Select the level data (name, description, plays, etc.), and find the name of the creator
            command.CommandText =
                "SELECT Level.GUID, Level.Name, Level.Description, Level.Plays, Level.Creator, Player.Username FROM Levels Level JOIN Players Player ON Player.GUID = Level.Creator";
            // Query the database and add all resulting levels to the level list
            await PerformQuery(connectionString, command, reader =>
            {
                if (!reader.HasRows)
                    return;

                while (reader.Read())
                {
                    LevelData briefing;
                    // If level is already open
                    if (Server.Levels.FirstOrDefault(x => x.UUID == reader.GetGuid(0)) != null)
                        briefing = Server.Levels.FirstOrDefault(x => x.UUID == reader.GetGuid(0));
                    else // Create new LevelData other wise
                        briefing = new LevelData(new PlayerData(reader.GetString(5), reader.GetGuid(4)), reader.GetString(1),
                            reader.GetGuid(0), reader.GetString(2), 0,
                            reader.GetInt32(3), 3.5d);

                    levels.Add(briefing);
                }
            });

            return levels;
        }

        /// <summary>
        /// Gets level data for the specified level.
        /// </summary>
        public async Task<LevelData> GetLevelData(Guid uuid)
        {
            LevelData data = null;
            var command = providerFactory.CreateCommand();
            if (command == null)
                return data;
            // Select the level data (name, description, plays, etc.), and find the name of the creator
            command.CommandText =
                "SELECT Level.GUID, Level.Name, Level.Description, Level.Plays, Level.Creator, Player.Username FROM Levels Level JOIN Players Player ON Player.GUID = Level.Creator WHERE Level.Guid = @uuid";
            AddParamaters(command, new Dictionary<string, string>
            {
                {"uuid", uuid.ToString("N")}
            });
            await PerformQuery(connectionString, command, reader =>
            {
                if (reader.Read())
                {
                    data = new LevelData(new PlayerData(reader.GetString(5), reader.GetGuid(4)), reader.GetString(1),
                        reader.GetGuid(0), reader.GetString(2), 0,
                        reader.GetInt32(3), 3.5d);
                }
            });

            return data;
        }

        /// <summary>
        /// Gets a player's username from their GUID and returns a PlayerData object.
        /// </summary>
        public async Task<PlayerData> GetPlayerData(Guid uuid)
        {
            var command = providerFactory.CreateCommand();
            PlayerData player = null;
            if (command != null)
            {
                command.CommandText = "SELECT username FROM Players WHERE guid = @uuid";
                AddParamaters(command, new Dictionary<string, string> {{"uuid", uuid.ToString("N")}});
                await PerformQuery(connectionString, command, reader =>
                {
                    if (reader.Read())
                        player = new PlayerData(reader.GetString(0), uuid);
                });
            }
            if (player != null)
                return player;
            throw new KeyNotFoundException($"Player with UUID of '{uuid}' was not found in the database.");
        }

        public override async Task Init()
        {
            if (!Server.IO.Initialized)
                throw new InvalidOperationException("The IO component must be initialized first.");

            Log($"Using provider {Server.IO.Config.Database.Provider}");

            // Create provider from provider string.
            connectionString = Server.IO.Config.Database.Connection;
            providerFactory = DbProviderFactories.GetFactory(Server.IO.Config.Database.Provider);

            if (!(await TestConnection(connectionString)))
                Log(
                    "Could not connect to database. Database services will be non functional.");

            // Create initial tables if they don't exist
            var initialCommand = providerFactory.CreateCommand();
            if (initialCommand != null)
            {
                initialCommand.CommandText =
                    "CREATE TABLE IF NOT EXISTS Levels (GUID GUID PRIMARY KEY,Name TEXT,Description TEXT,Plays INTEGER, Creator GUID);" +
                    "CREATE TABLE IF NOT EXISTS Players (Username Text UNIQUE,GUID GUID PRIMARY KEY)";
                await PerformOperation(connectionString, initialCommand);
            }

            // When a (new) user connects, add them to the database.
            Server.Events.Network.UserConnected.AddHandler(async args =>
            {
                var insertCommand = providerFactory.CreateCommand();
                if (insertCommand != null)
                {
                    insertCommand.CommandText =
                        "INSERT OR IGNORE INTO Players (Username, GUID) VALUES (@username, @uuid);";
                    AddParamaters(insertCommand,
                        new Dictionary<string, string>
                        {
                            {"username", args.Player.Username},
                            {"uuid", args.Player.UUID.ToString("N")}
                        });
                    await PerformOperation(connectionString, insertCommand);
                }
            }, EventPriority.InternalFinal);

            await base.Init();
        }

        internal async Task CreateLevel(LevelData data)
        {
            var command = providerFactory.CreateCommand();
            if (command != null)
            {
                command.CommandText =
                    "INSERT INTO Levels (GUID,Name,Description, Plays, Creator) VALUES(@guid, @name, @desc, 0, @creator)";
                AddParamaters(command, new Dictionary<string, string>
                {
                    {"guid", data.UUID.ToString("N")},
                    {"name", data.Name},
                    {"desc", data.Description},
                    {"creator", data.Creator.UUID.ToString("N")}
                });
                await PerformOperation(connectionString, command);
            }
        }

        /// <summary>
        /// Performs an async operation on the specified database that does not return any results.
        /// </summary>
        private async Task PerformOperation(string connection, DbCommand command)
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    using (var con = providerFactory.CreateConnection())
                    {
                        // Open the connection and connect to database
                        if (con == null)
                            return;

                        con.ConnectionString = connection;
                        con.Open();

                        // Execute command
                        command.Connection = con;
                        command.ExecuteNonQuery();
                        con.Close();
                    }
                }
                catch (Exception ex) // Catch any errors connecting to database
                {
                    if (ex is DbException || ex is SocketException)
                        Logger.WriteLine(LogType.Error, ex.ToString());
#if DEBUG
                    throw;
#endif
                }
                finally // Make sure the connection is closed
                {
                    command.Dispose();
                }
            });
        }

        /// <summary>
        /// Performs an async query on the specified database that returns a result to be handled as an action with a DbDataReader
        /// </summary>
        private async Task PerformQuery(string connection, DbCommand command, Action<DbDataReader> action)
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    // Use a DbProviderFactory to create the connection so that different providers can be used
                    // eg, MySql or SQLite
                    using (var con = providerFactory.CreateConnection())
                    {
                        // Connect
                        if (con == null)
                            return;

                        con.ConnectionString = connection;
                        con.Open();
                        command.Connection = con;

                        // Perform command and handle the result
                        using (var reader = command.ExecuteReader())
                            action(reader); // Let caller handle logic
                        con.Close();
                    }
                }
                catch (Exception ex) // Catch any errors connecting to database
                {
                    if (ex is DbException || ex is SocketException)
                        Logger.WriteLine(LogType.Error, ex.ToString());
#if DEBUG
                    throw;
#endif
                }
                finally // Make sure the connection is closed
                {
                    command.Dispose();
                }
            });
        }

        /// <summary>
        /// Checks if the server is able to connect to the database specified in the connection string
        /// </summary>
        private async Task<bool> TestConnection(string conn)
        {
            return await Task.Factory.StartNew(() =>
            {
                try
                {
                    using (var con = providerFactory.CreateConnection())
                    {
                        // I'm sorry Resharper, I'm afraid I can't do that
                        // ReSharper disable once InvertIf
                        if (con != null)
                        {
                            con.ConnectionString = conn;
                            Log("Testing connection to database \"{0}\"", con.Database);
                            con.Open();
                        }
                    }
                    Log("Success", ConsoleColor.Green);
                    return true;
                }
                catch (Exception)
                {
                    Log("Failure", ConsoleColor.Red);
                    return false;
                }
            });
        }
    }
}