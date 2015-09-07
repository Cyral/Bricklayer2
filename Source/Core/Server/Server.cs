using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Bricklayer.Core.Common.Entity;
using Bricklayer.Core.Server.Components;
using Bricklayer.Core.Server.World;
using Pyratron.Frameworks.Commands.Parser;
using Pyratron.Frameworks.LogConsole;

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
        public CommandParser Commands { get; private set; }

        /// <summary>
        /// The DatabaseComponent for handling database operations.
        /// </summary>
        public DatabaseComponent Database { get; private set; }

        /// <summary>
        /// Manages and lists all server events.
        /// </summary>
        public EventManager Events { get; private set; }

        /// <summary>
        /// IOComponent handles disk operations.
        /// </summary>
        public IOComponent IO { get; private set; }

        /// <summary>
        /// List of levels currently open.
        /// </summary>
        public List<Level> Levels { get; private set; }

        /// <summary>
        /// The NetworkComponent for handling recieving, sending, etc.
        /// </summary>
        public NetworkComponent Net { get; private set; }

        /// <summary>
        /// List of users online the server.
        /// </summary>
        public List<Player> Players { get; private set; }

        /// <summary>
        /// The PluginComponent for loading and managing plugins.
        /// </summary>
        public PluginComponent Plugins { get; private set; }

        private Timer saveTimer;
        private DateTime start;

        internal async Task Start()
        {
            Events = new EventManager();
            Players = new List<Player>();
            Levels = new List<Level>();

            // Setup server
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            start = DateTime.Now;

            var stopwatch = Stopwatch.StartNew();
            Logger.Log($"{Constants.Strings.ServerTitle}");

            // Initialize Properties
            Commands = CommandParser.CreateNew().UsePrefix(string.Empty).OnError(OnParseError);
            RegisterCommands();

            // Initialize Components
            IO = new IOComponent(this);
            Plugins = new PluginComponent(this);
            Database = new DatabaseComponent(this);
            Net = new NetworkComponent(this);

            Logger.InputEntered += input => Commands.Parse(input);
            Logger.MessageLogged +=
                (message, level, type, time, fullmessage) => Logger.LogToFile(fullmessage);
            Logger.Log($"Server is starting now, on {DateTime.Now.ToString("U", new CultureInfo("en-US"))}");


            await IO.Init();
            await Plugins.Init();
            await Database.Init();
            await Net.Init();

            // Create save timer
            saveTimer = new Timer(IO.Config.Server.AutoSaveTime * 1000 * 60);
            saveTimer.Elapsed += async (sender, args) => await SaveAll();
            saveTimer.Start();

            stopwatch.Stop();
            Logger.WriteBreak();
            Logger.Log("Ready. ({0}s) Type \"help\" for commands.", Math.Round(stopwatch.Elapsed.TotalSeconds, 2));
            Logger.WriteBreak();

            Logger.Wait();
        }


        /// <summary>
        /// Makes a player join the level with the specified UUID. If the level is not open, it will be loaded.
        /// </summary>
        /// <remarks>
        /// It is up to the method caller to send a message to the sender with the level data.
        /// </remarks>
        public async Task<Level> JoinLevel(Player sender, Guid uuid)
        {
            var level = Levels.FirstOrDefault(x => x.UUID == uuid);
            if (level == null)
            {
                level = await IO.LoadLevel(uuid);
                Levels.Add(level);
            }

            RemovePlayerFromLevels(sender);
            level.Players.Remove(sender);
            level.Players.Add(sender);

            await Database.AddPlay(level.UUID);
            level.Plays++;


            sender.Level = level;
            Events.Game.Level.PlayerJoined.Invoke(new EventManager.GameEvents.LevelEvents.PlayerJoinEventArgs(sender,
                sender.Level));

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
            var level = new Level(this, sender, name, Guid.NewGuid(), description, 0, 5);
            RemovePlayerFromLevels(sender);
            level.Players.Add(sender);

            // Add the level to the database.
            await Database.CreateLevel(level);
            // Save initial level.
            await IO.SaveLevel(level);

            Levels.Add(level);
            sender.Level = level;
            Events.Game.Level.PlayerJoined.Invoke(new EventManager.GameEvents.LevelEvents.PlayerJoinEventArgs(sender,
                sender.Level));

            return level;
        }

        /// <summary>
        /// Removes a player from any level they are currently in.
        /// </summary>
        internal void RemovePlayerFromLevels(Player sender)
        {
            if (sender.Level != null)
            {
                Events.Game.Level.PlayerLeft.Invoke(new EventManager.GameEvents.LevelEvents.PlayerLeaveEventArgs(
                    sender, sender.Level));
            }
        }

        internal async Task CloseLevel(Guid uuid)
        {
            var level = Levels.FirstOrDefault(r => r.UUID == uuid);
            if (level != null && level.Online == 0)
            {
                await IO.SaveLevel(level);
                Logger.Log(NetworkComponent.LevelLogType, $"{level.Name} closed.");
                Levels.Remove(level);
            }
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
                    {
                        Console.Write(command.Name + ": ");

                        if (!string.IsNullOrEmpty(command.Description))
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.Write(command.Description + " ");
                        }

                        Console.Write("(Usage: ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(command.GenerateUsage());
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine(")");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }));

            Commands.AddCommand(Command
                .Create("Stats")
                .AddAlias("stats", "data")
                .SetDescription("Shows statistics")
                .SetAction(delegate { WriteStats(); }));

            Commands.AddCommand(Command
                .Create("Save All")
                .AddAlias("save", "saveall")
                .SetDescription("Saves all levels.")
                .SetAction(async delegate { await SaveAll(); }));

            Commands.AddCommand(Command
                .Create("Plugin List")
                .AddAlias("plugins", "pl")
                .SetDescription("Lists all plugins.")
                .SetAction(delegate
                {
                    if (!Plugins.Initialized) return;
                    Console.WriteLine("Installed plugins list:");
                    var plugins = Plugins.Plugins.OrderBy(x => !x.IsEnabled).ThenBy(x => x.Name);
                    foreach (var plugin in plugins)
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = plugin.IsEnabled ? ConsoleColor.Green : ConsoleColor.Red;
                        Console.Write(plugin.IsEnabled ? " Enabled " : " Disabled");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(" " + plugin.Name);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine(" " + plugin.Identifier + " - v" + plugin.Version + " ");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }));

            Commands.AddCommand(Command
                .Create("Plugin Status")
                .AddAlias("plugin")
                .SetDescription(
                    "Enables or disables a plugin. It is recommended to restart after enabling or disabling a plugin.")
                .SetAction((args, data) =>
                {
                    if (!Plugins.Initialized) return;
                    // Find plugin.
                    var plugin =
                        Plugins.Plugins.FirstOrDefault(
                            x =>
                                x.Identifier.Equals(args.FromName("identifier_or_name"),
                                    StringComparison.OrdinalIgnoreCase)
                                ||
                                x.Name.Equals(args.FromName("identifier_or_name"),
                                    StringComparison.OrdinalIgnoreCase));
                    if (plugin != null)
                    {
                        var enabled = args.FromName("status").Equals("enable", StringComparison.OrdinalIgnoreCase);
                        var status = enabled ? "enabled" : "disabled";
                        if (plugin.IsEnabled != enabled) // If not already in the state.
                        {
                            try
                            {
                                if (enabled)
                                    Plugins.EnablePlugin(plugin);
                                else
                                    Plugins.DisablePlugin(plugin);
                            }
                            catch (Exception e)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(e.Message);
                                Console.ForegroundColor = ConsoleColor.White;
                                return;
                            }

                            Console.WriteLine($"Plugin \"{plugin.Name}\" {status}.");

                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine(
                                "It is recommended to restart the server for all changes to take effect. Plugins may be left in an unstable state.");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else
                        {
                            Console.WriteLine(
                                $"Plugin with identifier \"{args.FromName("identifier_or_name")}\" already {status}.");
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(
                            $"Plugin with identifier \"{args.FromName("identifier_or_name")}\" not found.");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                })
                .AddArgument(Argument
                    .Create("status")
                    .AddOption(Argument.Create("enable"))
                    .AddOption(Argument.Create("disable")))
                .AddArgument(Argument.Create("identifier_or_name")));

            Commands.AddCommand(Command
                .Create("Exit")
                .AddAlias("exit")
                .SetDescription("Gracefully exits, saving all everything and broadcasting an exit message.")
                .SetAction(delegate { SafeExit(); }));

            Commands.AddCommand(Command
                .Create("Reload")
                .AddAlias("reload", "rm")
                .SetDescription("Reloads config")
                .SetAction(async delegate { await IO.LoadServerConfig(); }));
        }

        /// <summary>
        /// Save all levels.
        /// </summary>
        internal async Task SaveAll()
        {
            if (Levels.Count > 0)
            {
                Logger.Log("Saving all levels. ({0})", Levels.Count);
                foreach (var level in Levels)
                    await IO.SaveLevel(level);
            }
        }

        /// <summary>
        /// Gracefully exits, saving all everything and broadcasting an exit message.
        /// </summary>
        private async void SafeExit()
        {
            Logger.Warn("\nServer is exiting.");
            Logger.Log("Network disconnected.");
            Net.Shutdown("The server has shut down. This may be a quick restart, or regular maintenance.");
            await SaveAll();
            Logger.Log($"Server Exit: The server has gracefully exited on {DateTime.Now.ToString("U")}\n");
            Logger.FlushLog();
            Environment.Exit(0);
        }

        /// <summary>
        /// Writes statistics to the console, such as the bandwidth used.
        /// </summary>
        private void WriteStats()
        {
            var stats =
                $"Sent: {(Net == null ? 0 : Math.Round(Net.NetServer.Statistics.SentBytes/1024d/1024, 1))}MB | Recieved:" +
                $" {(Net == null ? 0 : Math.Round(Net.NetServer.Statistics.ReceivedBytes/1024d/1024, 1))}MB | Uptime:" +
                $" {(DateTime.Now - start).ToString("d\\:hh\\:mm")}";

            Logger.WriteCenteredText(stats);
        }

        /// <summary>
        /// Finds a Sender from a remote unique identifier
        /// </summary>
        /// <param name="remoteUniqueIdentifier">The RUI to find</param>
        /// <param name="ignoreError">If true and the sender is not found, null will be returned instead of throwing an error.</param>
        public Player PlayerFromRUI(long remoteUniqueIdentifier, bool ignoreError = false)
        {
            var found =
                Players.FirstOrDefault(user => user.Connection.RemoteUniqueIdentifier == remoteUniqueIdentifier);
            if (found != null) return found;
            if (ignoreError) return null;
            throw new KeyNotFoundException($"Could not find user from RemoteUniqueIdentifier: {remoteUniqueIdentifier}");
        }

        private static void OnParseError(object sender, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        #endregion
    }
}