using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Bricklayer.Core.Common.Entity;
using Bricklayer.Core.Common.World;
using Bricklayer.Core.Server.World;
using Bricklayer.Core.Server.Components;
using Pyratron.Frameworks.Commands.Parser;
using Level = Bricklayer.Core.Server.World.Level;

namespace Bricklayer.Core.Server
{
    /// <summary>
    /// The main server class handles console commands and input, as well as containing references to the core components.
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Command parser for commmands ran in the console or by users.
        /// </summary>
        public CommandParser Commands { get; set; }

        /// <summary>
        /// The DatabaseComponent for handling database operations.
        /// </summary>
        public DatabaseComponent Database { get; set; }

        /// <summary>
        /// Manages and lists all server events.
        /// </summary>
        public EventManager Events { get; private set; }

        /// <summary>
        /// IOComponent handles disk operations.
        /// </summary>
        public IOComponent IO { get; set; }

        /// <summary>
        /// List of levels currently open.
        /// </summary>
        public List<Level> Levels { get; private set; }

        /// <summary>
        /// The NetworkComponent for handling recieving, sending, etc.
        /// </summary>
        public NetworkComponent Net { get; set; }

        /// <summary>
        /// List of users online the server.
        /// </summary>
        public List<Player> Players { get; private set; }

        /// <summary>
        /// The PluginComponent for loading and managing plugins.
        /// </summary>
        public PluginComponent Plugins { get; set; }

        private string clear, input;
        private bool showHeader;
        private DateTime start;

        internal async Task Start()
        {
            Logger.Server = this;
            Events = new EventManager();
            Players = new List<Player>();
            Levels = new List<Level>();

            //Setup server
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            start = DateTime.Now;
            input = string.Empty;
            clear = new string(' ', Console.WindowWidth);
            var stopwatch = Stopwatch.StartNew();
            Logger.WriteLine($"{Constants.Strings.ServerTitle}");
            Logger.WriteLine($"Server is starting now, on {DateTime.Now.ToString("U", new CultureInfo("en-US"))}");

            //Initialize Properties
            Commands = CommandParser.CreateNew().UsePrefix(string.Empty).OnError(OnParseError);
            RegisterCommands();

            //Initialize Components
            IO = new IOComponent(this);
            Plugins = new PluginComponent(this);
            Database = new DatabaseComponent(this);
            Net = new NetworkComponent(this);

            await IO.Init();
            await Plugins.Init();
            await Database.Init();
            await Net.Init();
            stopwatch.Stop();
            Logger.WriteBreak();
            Logger.WriteLine("Ready. ({0}s) Type /help for commands.",
                Math.Round(stopwatch.Elapsed.TotalSeconds, 2));
            Logger.WriteBreak();

            WriteHeader();

            while (true) //Parse commands now that messaging has been handed off to another thread
            {
                input = string.Empty;
                WriteCommandCursor();
                //Read input and parse command
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Backspace)
                    {
                        if (input.Length - 1 >= 0)
                        {
                            input = input.Substring(0, input.Length - 1);
                            Console.CursorLeft = 3 + input.Length - 1;
                            Console.Write(' ');
                            Console.CursorLeft = 3 + input.Length - 1;
                        }
                        continue;
                    }
                    if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine("");
                        break;
                    }
                    input += key.KeyChar;
                    Console.Write(key.KeyChar);
                }


                Commands.Parse(input.Trim());

                WriteHeader();
            }
            // ReSharper disable once FunctionNeverReturns
        }


        /// <summary>
        /// Makes a player join the level with the specified UUID. If the level is not open, it will be loaded.
        /// </summary>
        /// <remarks>
        /// It is up to the method caller to send a message to the sender with the level data.
        /// </remarks>
        public async Task<Level> JoinLevel(Player sender, Guid uuid)
        {
            //TODO: Implement level loading/saving from disk
            var level = new Level(sender, "Test", Guid.NewGuid(), "Test", 0, 2.5);
            level.Players.Add(sender);

            Levels.Add(level);

            return level;
        }

        /// <summary>
        /// Creates a level with the specified name and description, and the sender as the owner.
        /// </summary>
        /// <remarks>
        /// It is up to the method caller to send a message to the sender with the level data.
        /// </remarks>
        public async Task<Level> CreateLevel(Player sender, string name, string description)
        {
            var level = new Level(sender, name, Guid.NewGuid(), description, 0, 2.5);
            level.Players.Add(sender);

            //Add the level to the database
            await Database.CreateLevel(level);

            Levels.Add(level);

            return level;
        }

        #region Console Stuff

        /// <summary>
        /// Register command parser commands.
        /// </summary>
        private void RegisterCommands()
        {
            Commands.AddCommand(Command
                .Create("Command Help")
                .AddAlias("help", "list", "commands")
                .SetDescription("Lists commands")
                .SetAction(delegate
                {
                    foreach (var command in Commands.Commands)
                        Console.WriteLine(command.ShowHelp());
                }));

            Commands.AddCommand(Command
                .Create("Toggle header")
                .AddAlias("header", "toggle")
                .SetDescription("Toggles the statistics header.")
                .SetAction(delegate
                {
                    showHeader = !showHeader;
                    Console.WriteLine("Stats header {0}", showHeader ? "Enabled" : "Disabled");
                }));

            Commands.AddCommand(Command
                .Create("Stats")
                .AddAlias("stats", "data")
                .SetDescription("Shows statistics")
                .SetAction(delegate { WriteStats(); }));

            Commands.AddCommand(Command
                .Create("Exit")
                .AddAlias("exit")
                .SetDescription("Gracefully exits, saving all everything and broadcasting an exit message.")
                .SetAction(delegate { SafeExit(); }));

            Commands.AddCommand(Command
                .Create("Reload")
                .AddAlias("reload", "rm")
                .SetDescription("Reloads config")
                .SetAction(async delegate { await IO.LoadConfig(); }));
        }

        /// <summary>
        /// Gracefully exits, saving all everything and broadcasting an exit message.
        /// </summary>
        private void SafeExit()
        {
            Logger.WriteLine("\nSERVER SAFE EXIT:");
            Logger.WriteLine("Network disconnected.");
            Net.Shutdown("The server has shut down. This may be a quick restart, or regular maintenance");
            IO.LogMessage($"SERVER EXIT: The server has gracefully exited on {DateTime.Now.ToString("U")}\n");
            Environment.Exit(0); //Peace out dudes and dudettes <3 Yay to the woo
        }

        /// <summary>
        /// Writes the command cursor "$".
        /// </summary>
        public void WriteCommandCursor()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("$ ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(input);
            Console.CursorLeft = 2 + input.Length;
        }

        /// <summary>
        /// Writes the header at the top of the console.
        /// </summary>
        public void WriteHeader()
        {
            if (!showHeader)
                return;
            if (clear.Length != Console.WindowWidth)
                clear = new string(' ', Console.WindowWidth);
            var left = Console.CursorLeft;
            var top = Console.CursorTop;
            Console.SetCursorPosition(Math.Max(0, Console.WindowLeft), Math.Max(0, Console.WindowTop));
            Console.Write(clear);
            Console.SetCursorPosition(Math.Max(0, Console.WindowLeft), Math.Max(0, Console.WindowTop));

            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.Black;

            WriteStats();

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;

            Console.SetCursorPosition(Math.Max(0, left), Math.Max(0, top));
        }

        /// <summary>
        /// Writes statistics to the console, such as the bandwidth used.
        /// </summary>
        private void WriteStats()
        {
            string stats =
                $"Sent: {(Net == null ? 0 : Math.Round(Net.NetServer.Statistics.SentBytes / 1024d / 1024, 1))}MB | Recieved: {(Net == null ? 0 : Math.Round(Net.NetServer.Statistics.ReceivedBytes / 1024d / 1024, 1))}MB | Uptime: {(DateTime.Now - start).ToString("d\\:hh\\:mm")}";

            WriteCenteredText(stats);
        }

        /// <summary>
        /// Finds a Sender from a remote unique identifier
        /// </summary>
        /// <param name="remoteUniqueIdentifier">The RUI to find</param>
        /// <param name="ignoreError">If true and the sender is not found, null will be returned instead of throwing an error.</param>
        public Player PlayerFromRUI(long remoteUniqueIdentifier, bool ignoreError = false)
        {
            Player found = null;
            foreach (var user in Players)
            {
                if (user.Connection.RemoteUniqueIdentifier == remoteUniqueIdentifier)
                    found = user;
            }
            if (found != null) return found;
            if (ignoreError) return null;
            throw new KeyNotFoundException($"Could not find user from RemoteUniqueIdentifier: {remoteUniqueIdentifier}");
        }

        /// <summary>
        /// Helper method to write centered text.
        /// </summary>
        private void WriteCenteredText(string message)
        {
            if (Console.WindowWidth - message.Length > 0)
            {
                Console.Write(new string(' ', (Console.WindowWidth - message.Length) / 2));
                Console.Write(message);
                Console.WriteLine(new string(' ', (Console.WindowWidth - message.Length + 1) / 2));
            }
            else
                Console.Write(message);
        }

        private void OnParseError(object sender, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        #endregion
    }
}