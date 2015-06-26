using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Server.Data
{
    /// <summary>
    /// Server data such as name and host. To be used for saving/loading servers from a config file
    /// </summary>
    public class ServerSaveData
    {
        public string Name { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }

        public ServerSaveData(string name, string ip, int port)
        {
            Name = name;
            IP = ip;
            Port = port;
        }
        /// <summary>
        /// Returns a string created from an IP and port (If it exists), such as localhost:0000
        /// </summary>
        public string GetHostString()
        {
            return Port == 0 ? IP : IP + ":" + Port;
        }
    }
}
