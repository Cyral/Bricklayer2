using System;
using System.Threading.Tasks;
using Pyratron.Frameworks.LogConsole;

namespace Bricklayer.Core.Server.Components
{
    /// <summary>
    /// Represents a component required for the server.
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
        protected internal virtual LogType LogType { get; }

        /// <summary>
        /// The server object controlling this component.
        /// </summary>
        protected Server Server { get; }

        internal ServerComponent(Server server)
        {
            Server = server;
        }

        /// <summary>
        /// Performs initialization login for the component
        /// </summary>
        #pragma warning disable 1998 // Ignore warning, as overriden Init methods may include asynchonous code.
        public virtual async Task Init()
        #pragma warning restore 1998
        {
            Initialized = true;
            Logger.Log(LogType, "Intialized.");
        }
    }
}