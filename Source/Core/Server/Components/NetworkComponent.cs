#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Entity;
using Bricklayer.Core.Common.Net;
using Bricklayer.Core.Common.Net.Messages;
using Bricklayer.Core.Common.World;
using Bricklayer.Core.Server.Net;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using static Bricklayer.Core.Server.EventManager;

#endregion

namespace Bricklayer.Core.Server.Components
{
    /// <summary>
    /// Handles network functions for the server, such as port-forwarding, sending messages, etc
    /// Send Function Guide:
    /// Send: Sends a message to a Sender
    /// Broadcast: Sends a message to each Sender in a level
    /// Global: Sends a message to each Sender on the server
    /// </summary>
    public class NetworkComponent : ServerComponent
    {
        private static readonly NetDeliveryMethod deliveryMethod = NetDeliveryMethod.ReliableOrdered;
            //Message delivery method

        /// <summary>
        /// The server configuration
        /// </summary>
        public NetPeerConfiguration Config { get; set; }

        /// <summary>
        /// Indicates if the server has shut down, meaning there is no need to save maps.
        /// </summary>
        public bool IsShutdown { get; set; }

        /// <summary>
        /// The message loop for handling messages
        /// </summary>
        public MessageHandler MsgHandler { get; set; }

        /// <summary>
        /// The underlying Lidgren server object
        /// </summary>
        public NetServer NetServer { get; set; }

        /// <summary>
        /// Net LogType
        /// </summary>
        protected override LogType LogType => LogType.Net;

        /// <summary>
        /// The IP of the auth server.
        /// </summary>
        internal IPEndPoint AuthEndpoint { get; private set; }

        /// <summary>
        /// Stored pending user sessions. (The user's UUID and connection)
        /// These will be validated once the auth server responds.
        /// </summary>
        private readonly Dictionary<Guid, NetConnection> pendingSessions = new Dictionary<Guid, NetConnection>();

        private bool isDisposed; //Is the instance disposed?

        public NetworkComponent(Server server) : base(server)
        {
            //When a user requests to join, verify their account is valid with the auth server.
            Server.Events.Network.UserLoginRequested.AddHandler(args =>
            {
                Logger.WriteLine(LogType.Net,
                    args.Username + " requesting to join. Verifying public key with auth server.");
                if (pendingSessions.ContainsKey(args.UUID))
                    pendingSessions.Remove(args.UUID);
                pendingSessions.Add(args.UUID, args.Connection);
                var message = EncodeMessage(new PublicKeyMessage(args.Username, args.UUID, args.PublicKey));

                //Send public key to auth server to verify if session is valid
                NetServer.SendUnconnectedMessage(message, AuthEndpoint);
            });

            //If the response from the auth server is valid, approve their connection request and finalize the connection process.
            Server.Events.Network.SessionValidated.AddHandler(async args =>
            {
                if (!pendingSessions.ContainsKey(args.UUID))
                {
                    Logger.WriteLine(LogType.Error, $"Pending session for UUID \"{args.UUID}\" not found.");
                    return;
                }
                if (args.Valid)
                {
                    Server.Players.Add(new Player(pendingSessions[args.UUID], null, new Vector2(0, 0), args.Username,
                        args.UUID, false));
                    pendingSessions[args.UUID].Approve(EncodeMessage(
                        new InitMessage(Server.IO.Config.Server.Name, Server.IO.Config.Server.Decription,
                            Server.IO.Config.Server.Intro, NetServer.ConnectionsCount,
                            await Server.Database.GetAllLevels())));
                    Logger.WriteLine(LogType.Net,
                        $"Session valid for '{args.Username}'. (Allowed)");
                }
                else
                {
                    pendingSessions[args.UUID].Deny("Invalid or expired session.");
                    Logger.WriteLine(LogType.Net,
                        $"Session invalid for '{args.Username}'. (Denied)");
                }
                pendingSessions.Remove(args.UUID);
            }, EventPriority.InternalFinal);

            Server.Events.Network.MessageRequested.AddHandler(async args =>
            {
                //When the client request an init message (refreshing the lobby)
                if (args.Type == MessageTypes.Init)
                {
                    Send(new InitMessage(Server.IO.Config.Server.Name, Server.IO.Config.Server.Decription,
                        Server.IO.Config.Server.Intro, NetServer.ConnectionsCount, await Server.Database.GetAllLevels()),
                        args.Sender);
                }
                else if (args.Type == MessageTypes.Banner)
                {
                    if (Server.IO.Banner != null)
                        Send(new BannerMessage(Server.IO.Banner), args.Sender);
                }
            });

            //When a client loads their server list and requests server information, such as description, players online, etc.
            Server.Events.Network.InfoRequested.AddHandler(
                args =>
                {
                    SendUnconnected(args.Host,
                        new ServerInfoMessage(Server.IO.Config.Server.Decription, Server.Net.NetServer.ConnectionsCount,
                            Server.IO.Config.Server.MaxPlayers));
                });

            Server.Events.Network.CreateLevelMessageRecieved.AddHandler(
                async args =>
                {
                    //Create the new level
                    var level = await Server.CreateLevel(args.Sender, args.Name, args.Description);
                    Logger.WriteLine(LogType.Normal, $"Level \"{level.Name}\" created by {level.Creator.Username}");
                    Send(new LevelDataMessage(level), args.Sender);
                    //Fire another event with the newly created level, so that plugins can access it.
                    //(As the current event is only from the network message)
                    Server.Events.Game.Levels.LevelCreated.Invoke(new GameEvents.LevelEvents.CreateLevelEventArgs(level));


                }, EventPriority.InternalFinal); //Must be the last event called as it fires another event

            Server.Events.Network.JoinLevelMessageRecieved.AddHandler(
                async args =>
                {
                    //Create the new level
                    var level = await Server.JoinLevel(args.Sender, args.UUID);
                    Logger.WriteLine(LogType.Normal, $"Level \"{level.Name}\" joined by {args.Sender.Username}");
                    Send(new LevelDataMessage(level), args.Sender);
                    SendInLevel(level, new PlayerJoinMessage(args.Sender));
                    SendInLevel(level, new ChatMessage("[color:Green]" + args.Sender.Username + " has joined.[/color]"));

                }, EventPriority.InternalFinal);

            Server.Events.Network.ChatMessageReceived.AddHandler(
                args =>
                {
                    if (args.Sender.Level != null)
                    {
                        Logger.WriteLine(LogType.Normal,
                            $"Level \"{args.Sender.Level.Name}\" - (Chat) \"{args.Sender.Username}\": \"{args.Message}\"");
                        SendInLevel(args.Sender.Level, new ChatMessage(args.Sender.Username + ": " + args.Message));
                    }
                }, EventPriority.InternalFinal);

            Server.Events.Network.UserConnected.AddHandler(args =>
            {
                Logger.WriteLine(LogType.Normal, ConsoleColor.Green, $"Player \"{args.Player.Username}\" has connected.");
            }, EventPriority.InternalFinal);

            Server.Events.Network.UserDisconnected.AddHandler(args =>
            {
                Logger.WriteLine(LogType.Normal, ConsoleColor.Red, $"Player \"{args.Player.Username}\" has disconnected.");
            }, EventPriority.InternalFinal);
        }

        public override async Task Init()
        {
            if (!Server.IO.Initialized)
                throw new InvalidOperationException("The IO component must be initialized first.");

            //Find the address of the auth server
            await
                Task.Factory.StartNew(
                    () =>
                    {
                        AuthEndpoint = new IPEndPoint(NetUtility.Resolve(Server.IO.Config.Server.AuthServerAddress),
                            Server.IO.Config.Server.AuthServerPort);
                    });

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

            // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
            Config.EnableMessageType(NetIncomingMessageType.ConnectionApproval
                                     | NetIncomingMessageType.ErrorMessage
                                     | NetIncomingMessageType.WarningMessage
                                     | NetIncomingMessageType.Data
                                     | NetIncomingMessageType.StatusChanged
                                     | NetIncomingMessageType.UnconnectedData);
            // ReSharper enable BitwiseOperatorOnEnumWithoutFlags

            //Start Lidgren server
            NetServer = new NetServer(Config);
            try
            {
                NetServer.Start();
            }
                // ReSharper disable once RedundantCatchClause
            catch (SocketException ex)
            {
                Logger.WriteLine(LogType.Error, ex.Message);
                Server.IO.LogMessage(
                    $"SERVER EXIT: The server has exited because of an error on {DateTime.Now.ToString("U")}");
                return false;
            }
            Log("Lidgren NetServer started. Port: {0}, Max. Connections: {1}", Config.Port.ToString(),
                Config.MaximumConnections.ToString());

            //Start message handler
            MsgHandler = new MessageHandler(Server);
            MsgHandler.Start();
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
        /// Encodes and sends a message to a specified Sender
        /// </summary>
        /// <param name="gameMessage">IMessage to send</param>
        /// <param name="user">Sender to send to</param>
        public void Send(IMessage gameMessage, Player user)
        {
            var con =
                NetServer.Connections.FirstOrDefault(
                    x => x.RemoteUniqueIdentifier == user.Connection.RemoteUniqueIdentifier);
            if (con != null)
                Send(gameMessage, con);
        }


        /// <summary>
        /// Sends an unconnected message to the endpoint
        /// </summary>
        /// <param name="gameMessage">IMessage to write ID and send.</param>
        public void SendUnconnected(IPEndPoint receiver, IMessage gameMessage)
        {
            var message = EncodeMessage(gameMessage); //Write packet ID and encode
            NetServer.SendUnconnectedMessage(message, receiver); //Send
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
        /// Broadcasts a message to all Players in a level, EXCEPT for the one specified
        /// </summary>
        /// <param name="gameMessage">IMessage to send</param>
        /// <param name="user">Sender NOT to send to</param>
        /// <summary>
        /// Broadcasts a message to all clients in a level, EXCEPT for the one specified
        /// </summary>
        /// <param name="gameMessage">IMessage to send</param>
        /// <param name="recipient">Client NOT to send to</param>
        /// <summary>
        /// Broadcasts a message to all Players in a level
        /// </summary>
        /// <param name="Level">Level/Level to send to</param>
        /// <param name="gameMessage">IMessage to send</param>
        public void SendInLevel(Level level, IMessage message)
        {


            //Search for recipients
            var recipients = NetServer.Connections.Where(
                x => Server.PlayerFromRUI(x.RemoteUniqueIdentifier, true) != null &&
                     Server.PlayerFromRUI(x.RemoteUniqueIdentifier, true).Level == level)
                .ToList();

            if (recipients.Count > 0) //Send to recipients found
                NetServer.SendMessage(EncodeMessage(message), recipients, deliveryMethod, 0);
        }
        /// <summary>
        /// Sends a message to all Players connected to the server
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
        public void Shutdown(string reason = "Disconnected.")
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
        /// Disposes the NetworkManager
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
    }
}
