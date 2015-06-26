using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Server.Data
{
    /// <summary>
    /// Server data such as name and host. To be used for saving/loading servers from a config file.
    /// </summary>
    public class ServerSaveData
    {
        /// <summary>
        /// The name of the server.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The host of the server. (IP or hostname)
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// The port of the server.
        /// </summary>
        public int Port { get; private set; }

        public ServerSaveData(string name, string host, int port)
        {
            Name = name;
            Host = host;
            Port = port;
        }

        /// <summary>
        /// Returns a formatted string that shows the hostname and the port, if applicable.
        /// </summary>
        public string GetHostString()
        {
            return Port == 0 ? Host : Host + ":" + Port;
        }
    }
}
