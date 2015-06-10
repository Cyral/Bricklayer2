using System.Net;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using Lidgren.Network;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Net;
using Bricklayer.Core.Common.Net.Messages;

namespace Bricklayer.Core.Client.Net.Messages.AuthServer
{
    public class AuthNetworkManager
    {
        /// <summary>
        /// The underlying Lidgren NetClient.
        /// </summary>
        public NetClient NetClient { get; private set; }

        /// <summary>
        /// The message handler
        /// </summary>
        public AuthMessageHandler Handler { get; private set; }

        internal Client Client { get; set; }

        /// <summary>
        /// Configuration options for Lidgren messages
        /// </summary>
        public NetPeerConfiguration Config { get; private set; }

        private bool isDisposed;

        private const NetDeliveryMethod deliveryMethod = NetDeliveryMethod.ReliableOrdered;//Message delivery method

        public AuthNetworkManager(Client client)
        {
            Client = client;
        }


        /// <summary>
        /// Initialization logic on app startup
        /// </summary>
        public void Init()
        {
            // Create new instance of configs. Parameter is "application Id". It has to be same on client and server.
            Config = new NetPeerConfiguration(Globals.Strings.AuthNetworkID);

            //Create message handler (So events are initialized, but don't start it)
            Handler = new AuthMessageHandler(this);

            // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
            Config.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            // ReSharper enable BitwiseOperatorOnEnumWithoutFlags

        }

        /// <summary>
        /// Send Login details to Auth Server
        /// </summary>
        public async Task SendDetails(string username, string password)
        {
            await Task.Factory.StartNew(() =>
            {

                // Create new client, with previously created configs
                NetClient = new NetClient(Config);
                NetClient.Start();

                Handler.Start();

                Send(new AuthLoginMessage(Constants.Version, username, password));
            });
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
        /// Sends and encodes an IMessage to the auth server.
        /// </summary>
        /// <param name="gameMessage">IMessage to write ID and send.</param>
        public void Send(IMessage gameMessage)
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

    }
}
