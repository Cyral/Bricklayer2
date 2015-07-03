using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Common;

namespace Bricklayer.Core.Server
{
    /// <summary>
    /// The base of all server plugins.
    /// </summary>
    public abstract class ServerPlugin : Plugin
    {
        /// <summary>
        /// The server host.
        /// </summary>
        public Server Server { get; set; }

        /// <summary>
        /// Creates an instance of the plugin.
        /// </summary>
        public ServerPlugin(Server host)
        {
            Server = host;
        }
    }
}
