using System.Diagnostics;
using System.Net;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Net;
using Bricklayer.Core.Common.Net.Messages;
using Lidgren.Network;

namespace Bricklayer.Core.Client.Net.Messages.GameServer
{
    internal class NetworkManager
    {
        /// <summary>
        /// The underlying Lidgren NetClient.
        /// </summary>
        public NetClient NetClient { get; private set; }

        /// <summary>
        /// The message handler to handle incoming messages.
        /// </summary>
        public MessageHandler Handler { get; private set; }

        /// <summary>
        /// Configuration options for Lidgren messages.
        /// </summary>
        public NetPeerConfiguration Config { get; private set; }

        /// <summary>
        /// Contains the 2 keys used for authentication
        /// </summary>
        public Token TokenKeys { get; private set; }

        internal Client Client { get; }

        public NetworkManager(Client client)
        {
            Client = client;
        }

        private bool isDisposed;

        private static readonly NetDeliveryMethod deliveryMethod = NetDeliveryMethod.ReliableOrdered;//Message delivery method

        /// <summary>
        /// Initialization logic on app startup
        /// </summary>
        public void Init()
        {
            TokenKeys = new Token();

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

            //Listen for init response from auth server containing token keys
            Client.Events.Network.Auth.Init.AddHandler(args =>
            {
                TokenKeys.Username = args.Username;
                TokenKeys.UID = args.DatabaseId;
                TokenKeys.PrivateKey = args.PrivateKey;
                TokenKeys.PublicKey = args.PublicKey;
            });

            // Listen for verification result from the auth server
            Client.Events.Network.Auth.Verified.AddHandler(async args =>
            {
                if (args.Verified)
                {
                    Debug.WriteLine("Session verification Successful");
                    await ConnectToServer("127.0.0.1", Globals.Values.DefaultServerPort, "Test", TokenKeys.UID, TokenKeys.PublicKey); // Start connection process with game server once it gets session verification from the auth server
                }
                else
                    Debug.WriteLine("Session verification failed");
            });

            // Listen for when user is fully connected to game server
            Client.Events.Network.Game.Connect.AddHandler(args =>
            {
                Debug.WriteLine("Now connected to a server!");
            });
            // If user was disconnected from the server
            Client.Events.Network.Game.Disconnect.AddHandler(args =>
            {
                Debug.WriteLine("Disconnected or connection failed.");
            });

        }

        /// <summary>
        /// Sends login information to the authentication server.
        /// </summary>
        public void ConnectToAuth(string username, string password)
        {
            SendUnconnected(new AuthLoginMessage(Constants.Version, username, password));
        }

        public void SendSessionRequest(string username, string host, int port)
        {
            SendUnconnected(new SessionMessage(username, TokenKeys.UID, TokenKeys.PrivateKey, IPAddress.Parse(host), port));
        }

        /// <summary>
        /// Connects to the given game server.
        /// </summary>
        public async Task ConnectToServer(string host, int port, string username, int id, string publicKey)
        {
            await Task.Factory.StartNew(() =>
            {
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
        /// Creates a NetOutgoingMessage from the interal client object.
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
        /// Sends and encodes an IMessage to the connected game server.
        /// </summary>
        /// <param name="gameMessage">IMessage to write ID and send.</param>
        public void Send(IMessage gameMessage)
        {
            var message = EncodeMessage(gameMessage); //Write packet ID and encode
            NetClient.SendMessage(message, deliveryMethod); //Send
        }

        /// <summary>
        /// Sends an unconnected message to the auth server.
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
            return NetClient?.ConnectionStatus ?? NetConnectionStatus.None;
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
                    Disconnect();
                isDisposed = true;
            }
        }
    }
}
