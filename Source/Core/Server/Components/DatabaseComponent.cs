using System;
using System.Data.Common;
using System.Data.SQLite;
using System.Net.Sockets;
using System.Threading.Tasks;
using Bricklayer.Core.Server;
using Bricklayer.Core.Server.Components;

namespace Pyratron.Bricklayer.Auth.Components
{
    /// <summary>
    /// Handles user authentication and other database functions
    /// </summary>
    public class DatabaseComponent : ServerComponent
    {
        protected override LogType LogType => LogType.Database;

        private string connectionString;

        private string setupQuery =
            "CREATE TABLE IF NOT EXISTS `Levels` (`GUID` GUID,`Name` TEXT,`Description` TEXT,PRIMARY KEY(GUID));";
        private string getRoomsQuery = "SELECT `guid`, `name`, `description` FROM `Levels`";
        private DbProviderFactory providerFactory;

        public DatabaseComponent(Server server) : base(server)
        {

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

            base.Init();
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