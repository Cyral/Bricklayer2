using System;
using System.Threading;
using Bricklayer.Core.Common.Net;
using Bricklayer.Core.Common.Net.Messages;
using Bricklayer.Core.Server.Components;
using Lidgren.Network;

namespace Bricklayer.Core.Server.Net
{
    /// <summary>
    /// Handles processing of incoming messages from the connected game server.
    /// </summary>
    public class MessageHandler
    {
        private NetworkComponent NetManager => Server.Net;
        private Server Server { get; }
        private Thread networkThread;

        public MessageHandler(Server server)
        {
            Server = server;
        }

        /// <summary>
        /// The process network messages such as users joining, moving, etc
        /// </summary>
        public void ProcessNetworkMessages()
        {
            while (NetManager.NetServer.Status == NetPeerStatus.Running)
            {
                //If an issue occurs with this (again), see the discussion at:
                //https://groups.google.com/forum/#!topic/lidgren-network-gen3/EN3vaykwBWM
                NetManager.NetServer.MessageReceivedEvent.WaitOne();
                try
                {
                    NetIncomingMessage inc; //Used to store messages

                    while ((inc = NetManager.ReadMessage()) != null)
                    {
                        switch (inc.MessageType)
                        {
                            case NetIncomingMessageType.Error:
                            case NetIncomingMessageType.DebugMessage:
                            case NetIncomingMessageType.ErrorMessage:
                            case NetIncomingMessageType.VerboseDebugMessage:
                            case NetIncomingMessageType.WarningMessage:
                                Logger.WriteLine(LogType.Error, "MessageHandler Loop: " + inc.ReadString());
                                break;
                            //ConnectionApproval messages are sent when a client would like to connect to the server
                            case NetIncomingMessageType.ConnectionApproval:
                            {
                                //If client does not send username information (Purposely trying to use a modded client?)
                                if (inc.LengthBytes == 0)
                                {
                                    inc.SenderConnection?.Deny("Invalid Hail Message");
                                    break;
                                }

                                var type =
                                    (MessageTypes)Enum.Parse(typeof (MessageTypes), inc.ReadByte().ToString());
                                //Find message type
                                switch (type)
                                {
                                    // The connection should come with a public key to verify the client's session
                                    case MessageTypes.PublicKey:
                                    {
                                        var msg = new PublicKeyMessage(inc, MessageContext.Client);

                                        Server.Events.Network.UserLoginRequested.Invoke(
                                            new EventManager.NetEvents.LoginRequestEventArgs(msg.Username, msg.UUID,
                                                msg.PublicKey, inc.SenderConnection));

                                        break;
                                    }
                                }
                                break;
                            }
                            //Data messages are all messages manually sent from client
                            //These are the bulk of the messages, used for Sender movement, block placing, etc
                            case NetIncomingMessageType.Data:
                            {
                                ProcessDataMessage(inc);
                                break;
                            }
                            //StatusChanged messages occur when a client connects, disconnects, is approved, etc
                            //NOTE: Disconnecting and Disconnected are not instant unless client is shutdown with Disconnect()
                            case NetIncomingMessageType.StatusChanged:
                            {
                                var sender = Server.PlayerFromRUI(inc.SenderConnection.RemoteUniqueIdentifier, true);
                                //When a player's connection is finalized
                                if (inc.SenderConnection != null &&
                                    inc.SenderConnection.Status == NetConnectionStatus.Connected)
                                {
                                    if (sender != null)
                                        Server.Events.Network.UserConnected.Invoke(
                                            new EventManager.NetEvents.ConnectionEventArgs(sender));
                                }
                                //When a client disconnects
                                else if (inc.SenderConnection != null &&
                                         (inc.SenderConnection.Status == NetConnectionStatus.Disconnected ||
                                          inc.SenderConnection.Status == NetConnectionStatus.Disconnecting))
                                {
                                    if (sender != null)
                                        Server.Events.Network.UserDisconnected.Invoke(
                                            new EventManager.NetEvents.DisconnectionEventArgs(sender,
                                                inc.ReadString()));
                                }
                                break;
                            }
                            //Listen to unconnected data
                            case NetIncomingMessageType.UnconnectedData:
                            {
                                var type = (MessageTypes)Enum.Parse(typeof (MessageTypes), inc.ReadByte().ToString());

                                //Handle messages from the auth server differently then ones from players (ping requests)
                                if (Equals(inc.SenderEndPoint, NetManager.AuthEndpoint))
                                {
                                    switch (type)
                                    {
                                        //When the auth server confirms if a session is valid or not.
                                        case MessageTypes.ValidSession:
                                        {
                                            var msg = new ValidSessionMessage(inc, MessageContext.Server);
                                            Server.Events.Network.SessionValidated.Invoke(
                                                new EventManager.NetEvents.SessionEventArgs(
                                                    msg.Username, msg.UUID, msg.Valid));

                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    switch (type)
                                    {
                                        //When the auth server confirms if a session is valid or not.
                                        case MessageTypes.ServerInfo:
                                        {
                                            // ReSharper disable once UnusedVariable
                                            var msg = new ServerInfoMessage(inc, MessageContext.Server);
                                            Server.Events.Network.InfoRequested.Invoke(
                                                new EventManager.NetEvents.RequestInfoEventArgs(
                                                    inc.SenderEndPoint));
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                        NetManager.Recycle(inc);
                    }
                }
                catch (Exception e)
                {
                    Logger.WriteLine(LogType.Error, e.ToString());
                }
            }
        }

        /// <summary>
        /// Start the message handler on a new thread
        /// </summary>
        public void Start()
        {
            networkThread = new Thread(ProcessNetworkMessages)
            {
                Name = "Message Handler",
                Priority = ThreadPriority.AboveNormal
            };
            networkThread.Start();
        }

        /// <summary>
        /// Handles all actions for recieving data messages (Such as movement, block placing, etc)
        /// </summary>
        /// <param name="inc">The incoming message</param>
        private void ProcessDataMessage(NetIncomingMessage inc)
        {
            //The type of message being recieved
            var type = (MessageTypes)Enum.Parse(typeof (MessageTypes), inc.ReadByte().ToString());
            //The user who sent the message
            var sender = Server.PlayerFromRUI(inc.SenderConnection.RemoteUniqueIdentifier);
            if (sender != null)
            {
                switch (type)
                {
                    //If the user requests a message to be sent, send it back
                    case MessageTypes.Request:
                    {
                        var msg = new RequestMessage(inc, MessageContext.Server);
                        Server.Events.Network.MessageRequested.Invoke(
                            new EventManager.NetEvents.RequestMessageEventArgs(msg.Type, sender));
                        break;
                    }
                    case MessageTypes.CreateLevel:
                    {
                        var msg = new CreateLevelMessage(inc, MessageContext.Server);
                        Server.Events.Network.CreateLevelMessageRecieved.Invoke(
                            new EventManager.NetEvents.CreateLevelEventArgs(sender, msg.Name, msg.Description));
                        break;
                    }
                }
            }
        }
    }
}