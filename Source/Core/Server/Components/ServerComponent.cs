using System;
using System.Threading.Tasks;

namespace Bricklayer.Core.Server.Components
{
    /// <summary>
    /// Represents a static component required for the server.
    /// </summary>
    public abstract class ServerComponent
    {
        /// <summary>
        /// Determines if the component is initialized.
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        /// The type of log message associated with this component.
        /// </summary>
        protected virtual LogType LogType { get; private set; }

        /// <summary>
        /// The server object controlling this component.
        /// </summary>
        protected Server Server { get; set; }

        internal ServerComponent(Server server)
        {
            Server = server;
        }

        /// <summary>
        /// Performs initialization login for the component
        /// </summary>
#pragma warning disable 1998 //Ignore warning, as overriden Init methods may include asynchonous code.
        public virtual async Task Init()
        #pragma warning restore 1998
        {
            Initialized = true;
            Logger.WriteLine(LogType ?? LogType.Normal, "Intialized.");
        }

        /// <summary>
        /// Logs a message using the component's log type.
        /// </summary>
        protected virtual void Log(string message)
        {
            Logger.WriteLine(LogType ?? LogType.Normal, message);
        }

        /// <summary>
        /// Logs a message with parameters using string.Format.
        /// </summary>
        protected virtual void Log(string message, params object[] args)
        {
            Logger.WriteLine(LogType ?? LogType.Normal, string.Format(message, args));
        }

        /// <summary>
        /// Logs a message with a color.
        /// </summary>
        protected virtual void Log(string message, ConsoleColor color)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Logger.WriteLine(LogType ?? LogType.Normal, color, message);
        }

        /// <summary>
        /// Logs a message with a color and parameters using string.Format.
        /// </summary>
        protected virtual void Log(string message, ConsoleColor color, params object[] args)
        {
            Logger.WriteLine(LogType ?? LogType.Normal, color, string.Format(message, args));
        }
    }
}