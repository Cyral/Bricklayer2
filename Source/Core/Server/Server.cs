using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Bricklayer.Core.Server.Components;
using Bricklayer.Core.Server.Net;
using Pyratron.Frameworks.Commands.Parser;

namespace Bricklayer.Core.Server
{
    /// <summary>
    /// The main server class handles console commands and input, as well as containing references to the core components.
    /// </summary>
    public class Server
    {
        private static string clear, input;
        private static bool showHeader;
        private static DateTime start;

        /// <summary>
        /// IOComponent handles disk operations.
        /// </summary>
        public static IOComponent IO { get; set; }

        /// <summary>
        /// The NetworkComponent for handling recieving, sending, etc.
        /// </summary>
        public static NetworkManager Net { get; set; }

        /// <summary>
        /// Command parser for commmands ran in the console or by users.
        /// </summary>
        public static CommandParser Commands { get; set; }

        /// <summary>
        /// Manages and lists all server events.
        /// </summary>
        public static EventManager Events { get; private set; }

        public async Task Start()
        {
            Events = new EventManager();

            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();

            start = DateTime.Now;

            input = string.Empty;
            clear = new string(' ', Console.WindowWidth);
            var stopwatch = Stopwatch.StartNew();
            Logger.WriteLine(LogType.Server, $"{Constants.Strings.ServerTitle}");
            Logger.WriteLine(LogType.Server, $"Server is starting now, on {DateTime.Now.ToString("U")}");
            
            //Initialize Properties
            Commands = CommandParser.CreateNew().UsePrefix(string.Empty).OnError(OnParseError);
            RegisterCommands();

            //Initialize Components
            IO = new IOComponent();
            Net = new NetworkManager();

            await IO.Init();
            await Net.Init();

            stopwatch.Stop();
            Logger.WriteBreak();
            Logger.WriteLine(LogType.Server, "Ready. ({0}s) Type /help for commands.",
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

        #region Console Stuff

        /// <summary>
        /// Register command parser commands.
        /// </summary>
        private static void RegisterCommands()
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
        private static void SafeExit()
        {
            Logger.WriteLine(LogType.Server, "\nSERVER SAFE EXIT:");
            Logger.WriteLine(LogType.Server, "Network disconnected.");
            Net.Shutdown("The server has shut down. This may be a quick restart, or regular maintenance");
            IO.LogMessage($"SERVER EXIT: The server has gracefully exited on {DateTime.Now.ToString("U")}\n");
            Environment.Exit(0); //Peace out dudes and dudettes <3 Yay to the woo
        }

        /// <summary>
        /// Writes the command cursor "$".
        /// </summary>
        public static void WriteCommandCursor()
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
        public static void WriteHeader()
        {
            try
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
            catch (ArgumentOutOfRangeException e)
            {
                //Nothin, caused by window resize
            }
        }

        /// <summary>
        /// Writes statistics to the console, such as the bandwidth used.
        /// </summary>
        private static void WriteStats()
        {
            string stats =
                $"Sent: {(Net == null ? 0 : Math.Round(Net.NetServer.Statistics.SentBytes / 1024d / 1024, 1))}MB | Recieved: {(Net == null ? 0 : Math.Round(Net.NetServer.Statistics.ReceivedBytes / 1024d / 1024, 1))}MB | Uptime: {(DateTime.Now - start).ToString("d\\:hh\\:mm")}";

            WriteCenteredText(stats);
        }

        /// <summary>
        /// Helper method to write centered text.
        /// </summary>
        private static void WriteCenteredText(string message)
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

        private static void OnParseError(object sender, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        #endregion
    }
}