using System;
using System.Text;

namespace Bricklayer.Core.Server
{
    /// <summary>
    /// Logs messages to the console with many options, such as colors, prefixes, and built in string.Format arguments.
    /// </summary>
    public static class Logger
    {
        internal static string GetTimestamp()
        {
            return DateTime.Now.ToString("[HH:mm:ss] ");
        }

        internal static Server Server { get; set; }

        #region Console Helpers

        /// <summary>
        /// Writes text to the console on a new line.
        /// </summary>
        public static void WriteLine(string text)
        {
            LogType.Normal.WriteText(text);
        }

        /// <summary>
        /// Writes text to the console on a new line, with string.Format args.
        /// </summary>
        public static void WriteLine(string text, params object[] objs)
        {
            WriteLine(string.Format(text, objs));
        }

        /// <summary>
        /// Writes text to the console on a new line, with a specified color.
        /// </summary>
        public static void WriteLine(ConsoleColor color, string text)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.White;

            Server.WriteHeader();
            Server.IO?.LogMessage(text);
        }

        /// <summary>
        /// Writes text to the console on a new line, with a specified color, and string.Format args.
        /// </summary>
        public static void WriteLine(ConsoleColor color, string text, params object[] objs)
        {
            WriteLine(color, string.Format(text, objs));
        }

        /// <summary>
        /// Writes text to the console on a new line, using the specified log prefix.
        /// </summary>
        public static void WriteLine(LogType type, string text)
        {
            type.WriteText(text);
        }


        /// <summary>
        /// Writes text to the console on a new line, using the specified log prefix, and a color for the rest of the text.
        /// </summary>
        public static void WriteLine(LogType type, ConsoleColor color, string text)
        {
            type.WriteText(text, color);
        }

        /// <summary>
        /// Writes text to the console on a new line, using the specified log prefix, and string.Format args.
        /// </summary>
        public static void WriteLine(LogType type, string text, params object[] objs)
        {
            Console.CursorLeft = 0;
            WriteLine(type, string.Format(text, objs));
        }

        /// <summary>
        /// Writes text to the console on a new line, using the specified log prefix, and a color for the rest of the text.
        /// </summary>
        public static void WriteLine(LogType type, ConsoleColor color, string text, params object[] objs)
        {
            Console.CursorLeft = 0;
            WriteLine(type, color, string.Format(text, objs));
        }

        /// <summary>
        /// Writes text to the console.
        /// </summary>
        public static void Write(string text)
        {
            Console.Write(text);
        }

        /// <summary>
        /// Writes text to the console with specified color.
        /// </summary>
        public static void Write(ConsoleColor color, string text)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Writes text to the console.
        /// </summary>
        public static void Write(string text, params string[] objs)
        {
            Write(string.Format(text, objs));
        }

        /// <summary>
        /// Writes text to the console.
        /// </summary>
        public static void Write(ConsoleColor color, string text, params string[] objs)
        {
            Write(color, string.Format(text, objs));
        }

        /// <summary>
        /// Writes a line break/new line to the console.
        /// </summary>
        public static void WriteBreak()
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.BufferWidth - 1) + Environment.NewLine);
        }

        #endregion
    }

    /// <summary>
    /// Messages shown in the console with a specified color and prefix.
    /// </summary>
    public class LogType
    {
        private static readonly StringBuilder sb;
        private static readonly string spacer = ": ";

        /// <summary>
        /// Used for messages from the plugin manager, as well as messages from any plugins.
        /// </summary>
        public static LogType Plugin { get; private set; }

        /// <summary>
        /// Used for error messages. All messages from any log types are written to disk, so the error is retrievable if the server
        /// crashes.
        /// </summary>
        internal static LogType Error { get; private set; }

        /// <summary>
        /// Used for IOComponent notifications, such as reading from the disk.
        /// </summary>
        internal static LogType IO { get; private set; }

        /// <summary>
        /// Used for network notifications.
        /// </summary>
        internal static LogType Net { get; private set; }

        /// <summary>
        /// Used for standard console messages, without a prefix.
        /// </summary>
        internal static LogType Normal { get; private set; }

        /// <summary>
        /// Used for messages from the database manager.
        /// </summary>
        internal static LogType Database { get; private set; }

        static LogType()
        {
            sb = new StringBuilder();

            //Types
            Normal = new LogType("Server", ConsoleColor.White);
            IO = new LogType("IO", ConsoleColor.Cyan);
            Net = new LogType("Net", ConsoleColor.Magenta);
            Error = new LogType("Error", ConsoleColor.Red);
            Plugin = new LogType("Plugin", ConsoleColor.Green);
            Database = new LogType("Database", ConsoleColor.Yellow);
        }

        #region Instance

        public string Prefix { get; }
        private ConsoleColor Color { get; }

        private LogType(string prefix, ConsoleColor color)
        {
            Prefix = prefix;
            Color = color;
        }

        /// <summary>
        /// Write text to the console using the prefix
        /// </summary>
        public void WriteText(string text)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Gray;
            var time = Logger.GetTimestamp();
            Console.Write(time);
            Console.ForegroundColor = Color;
            var info = sb.Clear().Append(Prefix).Append(spacer).ToString();
            Console.Write(info);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text);

            var extra = sb.Clear().Append(time).Append(info).Append(text).ToString().Length;
            if (Console.WindowWidth - extra - 1 > 0)
            {
                Console.CursorLeft = extra;
                Console.WriteLine(new string(' ', Math.Max(1, Console.WindowWidth - extra - 1)));
            }

            Logger.Server.WriteHeader();
            Logger.Server.WriteCommandCursor();

            if (Logger.Server.IO != null) Logger.Server.IO.LogMessage(this, text);
        }

        /// <summary>
        /// Write text to the console using the prefix and a color for the rest of the text
        /// </summary>
        public void WriteText(string text, ConsoleColor color)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            var time = Logger.GetTimestamp();
            Console.Write(time);
            Console.ForegroundColor = Color;
            var info = sb.Clear().Append(Prefix).Append(spacer).ToString();
            Console.Write(info);
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = ConsoleColor.White;

            var extra = sb.Clear().Append(time).Append(info).Append(text).ToString().Length;
            Console.CursorLeft = extra;
            Console.WriteLine(new string(' ', Math.Max(1, Console.WindowWidth - extra - 1)));

            Logger.Server.WriteHeader();
            Logger.Server.WriteCommandCursor();

            if (Logger.Server.IO != null) Logger.Server.IO.LogMessage(this, text);
        }

        #endregion
    }
}