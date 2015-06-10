using System.Net;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Net;
using Bricklayer.Core.Common.Net.Messages;
using Lidgren.Network;

namespace Bricklayer.Core.Client.Net.Messages.GameServer
{
    public class NetworkManager
    {
        /// <summary>
        /// The underlying Lidgren NetClient.
        /// </summary>
        public NetClient NetClient { get; private set; }

        /// <summary>
        /// The message handler
        /// </summary>
        public MessageHandler Handler { get; private set; }

        /// <summary>
        /// The host server to connect to
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Configuration options for Lidgren messages
        /// </summary>
        public NetPeerConfiguration Config { get; private set; }

        /// <summary>
        /// The port of the server to connect to
        /// </summary>
        public int Port { get; set; }


        internal Client Client { get; set; }


        public NetworkManager(Client client)
        {
            Client = client;
        }

        private bool isDisposed;

        private const NetDeliveryMethod deliveryMethod = NetDeliveryMethod.ReliableOrdered;//Message delivery method

        /// <summary>
        /// Initialization logic on app startup
        /// </summary>
        public void Init()
        {
            // Create new instance of configs. Parameter is "application Id". It has to be same on client and server.
            Config = new NetPeerConfiguration(Globals.Strings.NetworkID);

            //Create message handler (So events are initialized, but don't start it)
            Handler = new MessageHandler(this);

            // Create new client, with previously created configs
            NetClient = new NetClient(Config);
            NetClient.Start();

            Handler.Start();

            // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
            Config.EnableMessageType(NetIncomingMessageType.ConnectionApproval
                                     | NetIncomingMessageType.Data
                                     | NetIncomingMessageType.ConnectionLatencyUpdated
                                     | NetIncomingMessageType.StatusChanged
                                     | NetIncomingMessageType.UnconnectedData);
            // ReSharper enable BitwiseOperatorOnEnumWithoutFlags
        }

        /// <summary>
        /// Connects to the given game server.
        /// </summary>
        public async Task Connect(string host, int port, string username, int id, string publicKey)
        {
            await Task.Factory.StartNew(() =>
            {
                Host = host;
                Port = port;

                // Create new client, with previously created configs
                NetClient = new NetClient(Config);
                NetClient.Start();

                Handler.Start();

                Join(host, port, username, id, publicKey);
            });
        }

        /// <summary>
        /// Sends a message once connected to join a server officially.
        /// </summary>
        private void Join(string host, int port, string username, int id, string publicKey)
        {
            NetClient.Connect(host, port,
                EncodeMessage(new PublicKeyMessage(username, id, publicKey)));
        }


        /// <summary>
        /// Creates a NetOutgoingMessage from the interal Client object.
        /// </summary>
        /// <returns>A new NetOutgoingMessage.</returns>
        public NetOutgoingMessage CreateMessage()
        {
            return NetClient.CreateMessage();
        }

        /// <summary>
        /// Reads the latest message in the queue.
        /// </summary>
        /// <returns>The latest NetIncomingMessage, ready for processing, null if no message waiting.</returns>
        public NetIncomingMessage ReadMessage()
        {
            return NetClient.ReadMessage();
        }

        /// <summary>
        /// Sends and encodes an IMessage to the server.
        /// </summary>
        /// <param name="gameMessage">IMessage to write ID and send.</param>
        public void Send(IMessage gameMessage)
        {
            var message = EncodeMessage(gameMessage); //Write packet ID and encode
            NetClient.SendMessage(message, deliveryMethod); //Send
        }

        /// <summary>
        /// Sends and encodes an IMessage to the auth server.
        /// </summary>
        /// <param name="gameMessage">IMessage to write ID and send.</param>
        public void SendUnconnected(IMessage gameMessage)
        {
            var message = EncodeMessage(gameMessage); //Write packet ID and encode
            var receiver = new IPEndPoint(NetUtility.Resolve(Globals.Values.DefaultAuthAddress), Globals.Values.DefaultAuthPort); // Auth Server info
            NetClient.SendUnconnectedMessage(message, receiver); //Send
        }

        /// <summary>
        /// Writes an IMessage's packet ID and encodes it's data into a NetOutgoingMessage.
        /// </summary>
        public NetOutgoingMessage EncodeMessage(IMessage gameMessage)
        {
            gameMessage.Context = MessageContext.Client;
            var message = NetClient.CreateMessage();
            //Write packet type ID
            message.Write((byte)gameMessage.MessageType);
            gameMessage.Encode(message);
            return message;
        }

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        /// <param name="reason">Reason to tell the server for disconnecting.</param>
        public void Disconnect(string reason = "Disconnected.")
        {
            if (NetClient != null) NetClient.Disconnect(reason);
        }

        /// <summary>
        /// Gets the connection status from the internal client
        /// </summary>
        public NetConnectionStatus GetConnectionStatus()
        {
            return NetClient != null ? NetClient.ConnectionStatus : NetConnectionStatus.None;
        }

        /// <summary>
        /// Gets the server connection's Statistics from the internal client
        /// </summary>
        public NetConnectionStatistics GetConnectionStats()
        {
            return NetClient.ServerConnection.Statistics;
        }

        /// <summary>
        /// Gets the client status from the internal client
        /// </summary>
        /// <returns></returns>
        public NetPeerStatus GetPeerStatus()
        {
            return NetClient.Status;
        }

        /// <summary>
        /// Gets the client statistics from the internal client
        /// </summary>
        /// <returns></returns>
        public NetPeerStatistics GetPeerStats()
        {
            return NetClient.Statistics;
        }

        /// <summary>
        /// Recycles a message after processing by reusing it, reducing GC load
        /// </summary>
        /// <param name="im">Message to recylce</param>
        public void Recycle(NetIncomingMessage im)
        {
            NetClient.Recycle(im);
        }

        /// <summary>
        /// Disposes the Network
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes the connection and disconnects from the server
        /// </summary>
        /// <param name="disposing">Disconnect?</param>
        private void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    Disconnect();
                }
                isDisposed = true;
            }
        }
    }
}
