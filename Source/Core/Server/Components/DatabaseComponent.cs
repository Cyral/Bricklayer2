using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Net.Sockets;
using System.Threading.Tasks;
using Bricklayer.Core.Server.Data;

namespace Bricklayer.Core.Server.Components
{
    /// <summary>
    /// Handles user authentication and other database functions
    /// </summary>
    public class DatabaseComponent : ServerComponent
    {
        protected override LogType LogType => LogType.Database;

        private readonly string
            setupQuery =
                "CREATE TABLE IF NOT EXISTS `Levels` (`GUID` GUID,`Name` TEXT,`Description` TEXT,`Plays` INTEGER,PRIMARY KEY(GUID));",
            getRoomsQuery = "SELECT `guid`, `name`, `description`, `plays` FROM `Levels`",
            createRoomQuery =
                "INSERT INTO `Levels`(`GUID`,`Name`,`Description`, `Plays`) VALUES (@guid, @name, @desc, 0);";

        private string connectionString;
        private DbProviderFactory providerFactory;

        public DatabaseComponent(Server server) : base(server)
        {
        }

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
        /// Finds all briefings favorited by the specified user
        /// </summary>
        public async Task<List<LobbySaveData>> GetAllRooms()
        {
            var briefings = new List<LobbySaveData>();
            var command = providerFactory.CreateCommand();
            if (command != null)
            {
                command.CommandText = getRoomsQuery;
                //Query the database and add all resulting briefings to the briefing list
                await PerformQuery(connectionString, command, reader =>
                {
                    while (reader.Read())
                    {
                        // Create and add each briefing to a list
                        var briefing = new LobbySaveData(reader.GetString(1), 0, reader.GetString(2), 10,
                            reader.GetInt32(3), 3.5d);
                        briefings.Add(briefing);
                    }
                });
            }

            return briefings;
        }

        public override async Task Init()
        {
            if (!Server.IO.Initialized)
                throw new InvalidOperationException("The IO component must be initialized first.");

            Log($"Using provider {Server.IO.Config.Database.Provider}");

            //Create provider from provider string.
            connectionString = Server.IO.Config.Database.Connection;
            providerFactory = DbProviderFactories.GetFactory(Server.IO.Config.Database.Provider);

            if (!(await TestConnection(connectionString)))
                Log(
                    "Could not connect to database. Database services will be non functional.");

            //Create initial tables
            var initialCommand = providerFactory.CreateCommand();
            if (initialCommand != null)
            {
                initialCommand.CommandText = setupQuery;
                await PerformOperation(connectionString, initialCommand);
            }

            //Create temporary rooms for testing
            var rooms = new List<LobbySaveData>
            {
                new LobbySaveData("DogeBall", 0, "A game of dodge ball, but the ball being a doge head. Wow!", 6,
                    23, 4),
                new LobbySaveData("Terrain", 1,
                    "Beatiful terrain environment builds for your eyes to look at! Enjoy :)", 3, 20, 5),
                new LobbySaveData("pls r8 5", 2, "pls r8 5. thats al i evr wanted in life.", 0, 7, 1)
            };

            foreach (var room in rooms)
              CreateRoom(room);

            base.Init();
        }

        internal async void CreateRoom(LobbySaveData data)
        {
            var command = providerFactory.CreateCommand();
            if (command != null)
            {
                command.CommandText = createRoomQuery;
                AddParamaters(command, new Dictionary<string, string>
                {
                    {"guid", Guid.NewGuid().ToString()},
                    {"name", data.Name},
                    {"desc", data.Description}
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
                        //Open the connection and connect to database
                        if (con != null)
                        {
                            con.ConnectionString = connection;
                            con.Open();

                            //Execute command
                            command.Connection = con;
                            command.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }
                catch (Exception ex) //Catch any errors connecting to database
                {
                    if (ex is DbException || ex is SocketException)
                        Logger.WriteLine(LogType.Error, ex.ToString());
#if DEBUG
                    throw;
#endif
                }
                finally //Make sure the connection is closed
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
                    //Use a DbProviderFactory to create the connection so that different providers can be used
                    //eg, MySql or SQLite
                    using (var con = providerFactory.CreateConnection())
                    {
                        //Connect
                        if (con != null)
                        {
                            con.ConnectionString = connection;
                            con.Open();
                            command.Connection = con;

                            //Perform command and handle the result
                            using (var reader = command.ExecuteReader())
                            {
                                action(reader); //Let caller handle logic
                            }
                            con.Close();
                        }
                    }
                }
                catch (Exception ex) //Catch any errors connecting to database
                {
                    if (ex is DbException || ex is SocketException)
                        Logger.WriteLine(LogType.Error, ex.ToString());
#if DEBUG
                    throw;
#endif
                }
                finally //Make sure the connection is closed
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