namespace Bricklayer.Core.Common.Data
{
    /// <summary>
    /// Server data such as name and host. To be used for saving/loading servers from a config file.
    /// </summary>
    public class ServerData
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

        public ServerData(string name, string host, int port)
        {
            Name = name;
            Host = host;
            Port = port == 0 ? Globals.Values.DefaultServerPort : port;
        }

        /// <summary>
        /// Returns a formatted string that shows the hostname and the port, if applicable.
        /// </summary>
        public string GetHostString()
        {
            return Port == 0 || Port == Globals.Values.DefaultServerPort ? Host : $"{Host}:{Port}";
        }
    }
}
