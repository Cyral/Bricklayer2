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
        public PluginComponent Plugins { get; internal set; }

        private string clear, input;
        private int historyIndex;
        private List<string> inputHistory;
        private Timer saveTimer;
        private bool showHeader;
        private DateTime start;

        internal async Task Start()
        {
            Logger.Server = this;
            Events = new EventManager();
            Players = new List<Player>();
            Levels = new List<Level>();

            // Setup server
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            start = DateTime.Now;
            input = string.Empty;
            clear = new string(' ', Console.WindowWidth);
            var stopwatch = Stopwatch.StartNew();
            Logger.WriteLine($"{Constants.Strings.ServerTitle}");
            Logger.WriteLine($"Server is starting now, on {DateTime.Now.ToString("U", new CultureInfo("en-US"))}");

            // Initialize Properties
            Commands = CommandParser.CreateNew().UsePrefix(string.Empty).OnError(OnParseError);
            RegisterCommands();

            // Initialize Components
            IO = new IOComponent(this);
            Plugins = new PluginComponent(this);
            Database = new DatabaseComponent(this);
            Net = new NetworkComponent(this);

            await IO.Init();
            await Plugins.Init();
            await Database.Init();
            await Net.Init();

            // Create save timer
            saveTimer = new Timer(IO.Config.Server.AutoSaveTime*1000*60);
            saveTimer.Elapsed += async (sender, args) => await SaveAll();
            saveTimer.Start();

            stopwatch.Stop();
            Logger.WriteBreak();
            Logger.WriteLine("Ready. ({0}s) Type /help for commands.", Math.Round(stopwatch.Elapsed.TotalSeconds, 2));
            Logger.WriteBreak();

            WriteHeader();

            inputHistory = new List<string>();

            while (true) // Parse commands now that messaging has been handed off to another thread
            {
                input = string.Empty;
                WriteCommandCursor();

                // Read input and parse commands
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Backspace)
                    {
                        if (input.Length - 1 >= 0)
                        {
                            input = input.Substring(0, input.Length - 1);
                            Console.CursorLeft = 2 + input.Length;
                            Console.Write(" \b");
                        }
                        continue;
                    }
                    if (key.Key == ConsoleKey.Enter)
                    {
                        // Enter press will parse the command.
                        Console.WriteLine();
                        break;
                    }
                    // Arrow keys will scroll through command history.
                    switch (key.Key)
                    {
                        case ConsoleKey.UpArrow:
                            if (historyIndex > 0 && inputHistory.Count > 0)
                            {
                                historyIndex--;
                                Console.CursorLeft = 2;
                                if (historyIndex >= inputHistory.Count - 1)
                                    Console.Write(inputHistory[historyIndex] + new string(' ', input.Length));
                                else if (historyIndex < inputHistory.Count - 1 &&
                                         inputHistory[historyIndex + 1].Length + 1 - inputHistory[historyIndex].Length <=
                                         0)
                                    Console.Write(inputHistory[historyIndex]);
                                else
                                    Console.Write(inputHistory[historyIndex] +
                                                  new string(' ', Math.Max(0, Math.Max(input.Length, 
                                                          inputHistory[historyIndex + 1].Length + 1 -
                                                          inputHistory[historyIndex].Length))));
                                Console.CursorLeft = inputHistory[historyIndex].Length + 2;
                                input = inputHistory[historyIndex];
                            }
                            continue;
                        case ConsoleKey.DownArrow:
                            if (historyIndex < inputHistory.Count && inputHistory.Count > 0)
                            {
                                historyIndex++;
                                Console.CursorLeft = 2;
                                if (historyIndex == inputHistory.Count)
                                {
                                    Console.Write(new string(' ',
                                        Math.Max(0, Math.Max(input.Length, inputHistory[historyIndex - 1].Length))));
                                    input = string.Empty;
                                    Console.CursorLeft = 2;
                                    continue;
                                }
                                if (inputHistory[historyIndex - 1].Length + 1 - inputHistory[historyIndex].Length <= 0)
                                    Console.Write(inputHistory[historyIndex]);
                                else
                                    Console.Write(inputHistory[historyIndex] +
                                                  new string(' ',
                                                      Math.Max(0, Math.Max(input.Length,
                                                          inputHistory[historyIndex - 1].Length + 1 -
                                                          inputHistory[historyIndex].Length))));
                                Console.CursorLeft = inputHistory[historyIndex].Length + 2;
                                input = inputHistory[historyIndex];
                            }
                            continue;
                        case ConsoleKey.LeftArrow:
                            if (Console.CursorLeft > 2)
                                Console.CursorLeft--;
                            continue;
                        case ConsoleKey.RightArrow:
                            if (Console.CursorLeft < Math.Min(2 + input.Length, Console.BufferWidth - 1))
                                Console.CursorLeft++;
                            continue;
                    }
                    input += key.KeyChar;
                    Console.Write(key.KeyChar);
                }

                if (!string.IsNullOrWhiteSpace(input))
                {
                    inputHistory.Add(input.Trim());
                    Commands.Parse(input.Trim());

                    // Set history index and remove old entries.
                    historyIndex = inputHistory.Count;
                    if (inputHistory.Count > 50)
                        inputHistory.RemoveAt(0);
                }

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
            var level = Levels.FirstOrDefault(x => x.UUID == uuid);
            if (level == null)
            {
                level = await IO.LoadLevel(uuid);
                Levels.Add(level);
            }

            await RemovePlayerFromLevels(sender);
            level.Players.Remove(sender);
            level.Players.Add(sender);

            sender.Level = level;

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
            var level = new Level(this, sender, name, Guid.NewGuid(), description, 0, 2.5);
            await RemovePlayerFromLevels(sender);
            level.Players.Add(sender);

            // Add the level to the database.
            await Database.CreateLevel(level);
            // Save initial level.
            await IO.SaveLevel(level);

            Levels.Add(level);
            sender.Level = level;

            return level;
        }

        /// <summary>
        /// Removes a player from any level they are currently in. (Does not send network message.)
        /// </summary>
        internal async Task RemovePlayerFromLevels(Player sender)
        {
            if (sender.Level != null)
            {
                sender.Level.Players.Remove(sender);
                if (sender.Level.Players.Count == 0)
                {
                    await CloseLevel(sender.Level.UUID); // Close level if nobody is in it.
                }
            }
        }

        private async Task CloseLevel(Guid uuid)
        {
            var level = Levels.FirstOrDefault(r => r.UUID == uuid);
            if (level != null && level.Online == 0)
            {
                await IO.SaveLevel(level);
                Logger.WriteLine(LogType.Normal, $"{level.Name} closed.");
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
                                x.Identifier.Equals(args.FromName("plugin-identifier-or-name"),
                                    StringComparison.OrdinalIgnoreCase)
                                ||
                                x.Name.Equals(args.FromName("plugin-identifier-or-name"),
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
                                $"Plugin with identifier \"{args.FromName("plugin-identifier-or-name")}\" already {status}.");
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(
                            $"Plugin with identifier \"{args.FromName("plugin-identifier-or-name")}\" not found.");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                })
                .AddArgument(Argument
                    .Create("status")
                    .AddOption(Argument.Create("enable"))
                    .AddOption(Argument.Create("disable")))
                .AddArgument(Argument.Create("plugin-identifier-or-name")));

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
        /// Save all levels.
        /// </summary>
        internal async Task SaveAll()
        {
            if (Levels.Count > 0)
            {
                Logger.WriteLine("Saving all levels. ({0})", Levels.Count);
                foreach (var level in Levels)
                    await IO.SaveLevel(level);
            }
        }

        /// <summary>
        /// Gracefully exits, saving all everything and broadcasting an exit message.
        /// </summary>
        private async void SafeExit()
        {
            Logger.WriteLine("\nSERVER SAFE EXIT:");
            Logger.WriteLine("Network disconnected.");
            Net.Shutdown("The server has shut down. This may be a quick restart, or regular maintenance");
            await SaveAll();
            IO.LogMessage($"SERVER EXIT: The server has gracefully exited on {DateTime.Now.ToString("U")}\n");
            Environment.Exit(0);
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
                $"Sent: {(Net == null ? 0 : Math.Round(Net.NetServer.Statistics.SentBytes/1024d/1024, 1))}MB | Recieved: {(Net == null ? 0 : Math.Round(Net.NetServer.Statistics.ReceivedBytes/1024d/1024, 1))}MB | Uptime: {(DateTime.Now - start).ToString("d\\:hh\\:mm")}";

            WriteCenteredText(stats);
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

        /// <summary>
        /// Helper method to write centered text.
        /// </summary>
        private static void WriteCenteredText(string message)
        {
            if (Console.WindowWidth - message.Length > 0)
            {
                Console.Write(new string(' ', (Console.WindowWidth - message.Length)/2));
                Console.Write(message);
                Console.WriteLine(new string(' ', (Console.WindowWidth - message.Length + 1)/2));
            }
            else
                Console.Write(message);
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