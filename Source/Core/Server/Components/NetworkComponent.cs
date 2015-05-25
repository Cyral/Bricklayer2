#region Usings

using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Net;
using Bricklayer.Core.Server.Net;
using Lidgren.Network;

#endregion

namespace Bricklayer.Core.Server.Components
{
    /// <summary>
    /// Handles network functions for the server, such as port-forwarding, sending messages, etc.
    /// Send: Sends a message to a user.
    /// Broadcast: Sends a message to each user in a room.
    /// Global: Sends a message to each user on the server.
    /// </summary>
    public class NetworkComponent : ServerComponent
    {
        #region Properties

        /// <summary>
        /// The underlying Lidgren server object
        /// </summary>
        internal NetServer NetServer { get; set; }

        /// <summary>
        /// The message loop for handling messages
        /// </summary>
        public MessageHandler Messages { get; set; }

        /// <summary>
        /// The server configuration
        /// </summary>
        internal NetPeerConfiguration Config { get; set; }

        /// <summary>
        /// Indicates if the server has shut down.
        /// </summary>
        public bool IsShutdown { get; set; }

        protected override LogType LogType => LogType.Net;

        #endregion

        #region Fields

        private const NetDeliveryMethod deliveryMethod = NetDeliveryMethod.ReliableOrdered; //Message delivery method
        private bool isDisposed; //Is the instance disposed?

        #endregion

        #region Methods

        public override async Task Init()
        {
            if (!Server.IO.Initialized)
                throw new InvalidOperationException("The IO component must be initialized first.");

            var result = Start(Server.IO.Config.Server.Port, Server.IO.Config.Server.MaxPlayers);
            if (!result)
            {
                Log("Aborting startup.");
                Environment.Exit(0);
            }

            await base.Init();
        }

        /// <summary>
        /// Starts the server connection, returns false if error
        /// </summary>
        /// <param name="port">Port to run on</param>
        /// <param name="maxconnections">Maximum clients connectable</param>
        public bool Start(int port, int maxconnections)
        {
            //Set up config
            Config = new NetPeerConfiguration(Globals.Strings.NetworkID)
            {
                Port = port,
                MaximumConnections = maxconnections
            };

            Config.EnableMessageType(NetIncomingMessageType.ConnectionApproval
                                     | NetIncomingMessageType.ErrorMessage
                                     | NetIncomingMessageType.WarningMessage
                                     | NetIncomingMessageType.Data
                                     | NetIncomingMessageType.StatusChanged);

            //Start Lidgren server
            NetServer = new NetServer(Config);
            try
            {
                NetServer.Start();
            }
            catch (SocketException ex)
            {
                Logger.WriteLine(LogType.Error, ex.Message);
                Server.IO.LogMessage(string.Format("SERVER EXIT: The server has exited because of an error on {0}",
                    DateTime.Now.ToString("U")));
                return false;
            }
            Log("Lidgren NetServer started. Port: {0}, Max. Connections: {1}", Config.Port.ToString(),
                Config.MaximumConnections.ToString());

            //Start message handler
            Messages = new MessageHandler();
            //Messages.Start();
            Log("Message handler started.");

            return true; //No error
        }

        /// <summary>
        /// Creates a NetOutgoingMessage from the interal Server object
        /// </summary>
        /// <returns>A new NetOutgoingMessage</returns>
        public NetOutgoingMessage CreateMessage()
        {
            return NetServer.CreateMessage();
        }

        /// <summary>
        /// Reads the latest message in the queue
        /// </summary>
        /// <returns>The latest message, ready for processing, null if no message waiting</returns>
        public NetIncomingMessage ReadMessage()
        {
            return NetServer.ReadMessage();
        }

        /// <summary>
        /// Encodes and sends a message to a specified NetConnection recipient
        /// </summary>
        /// <param name="gameMessage">IMessage to send</param>
        /// <param name="recipient">Client to send to</param>
        public void Send(IMessage gameMessage, NetConnection recipient)
        {
            if (recipient != null)
                NetServer.SendMessage(EncodeMessage(gameMessage), recipient, deliveryMethod);
        }

        /// <summary>
        /// Sends a message to all Users connected to the server
        /// </summary>
        /// <param name="gameMessage">IMessage to send</param>
        public void Global(IMessage gameMessage)
        {
            NetServer.SendToAll(EncodeMessage(gameMessage), deliveryMethod); //Send
        }

        /// <summary>
        /// Encodes a message with a packet ID so the client can identify what kind of message it is
        /// </summary>
        /// <param name="gameMessage">A message to encode</param>
        /// <returns>An encoded message as a NetOutgoingMessage</returns>
        public NetOutgoingMessage EncodeMessage(IMessage gameMessage)
        {
            gameMessage.Context = MessageContext.Server;
            var message = NetServer.CreateMessage();
            //Write packet type ID
            message.Write((byte)gameMessage.MessageType);
            gameMessage.Encode(message);
            return message;
        }

        /// <summary>
        /// Shuts down the server and disconnects clients
        /// </summary>
        /// <param name="reason">Reason for shutting down</param>
        public void Shutdown(string reason = "Auth server is shutting down.")
        {
            IsShutdown = true;
            NetServer.Shutdown(reason);
        }

        /// <summary>
        /// Recycles a message after processing by reusing it, reducing GC load
        /// </summary>
        /// <param name="im">Message to recylce</param>
        public void Recycle(NetIncomingMessage im)
        {
            if (im != null)
                NetServer.Recycle(im);
        }

        /// <summary>
        /// Disposes the NetworkComponent
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes the connection and shuts down the server
        /// </summary>
        /// <param name="disposing">Disconnect?</param>
        private void Dispose(bool disposing)
        {
            if (isDisposed) return;
            if (disposing)
                Shutdown();
            isDisposed = true;
        }

        #endregion
    }
}