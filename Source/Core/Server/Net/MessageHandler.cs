using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Bricklayer.Core.Common;
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
        private Server Server { get; set; }

        /// <summary>
        /// Stored pending user sessions
        /// </summary>
        private readonly Dictionary<int, NetConnection> pendingSessions = new Dictionary<int, NetConnection>();

        /// <summary>
        /// The process network messages such as users joining, moving, etc
        /// </summary>
        public async void ProcessNetworkMessages()
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
                                    case MessageTypes.PublicKey:
                                        // The connection should come with a public key to verify the client's session
                                    {
                                        var msg = new PublicKeyMessage(inc, MessageContext.Client);
                                        Logger.WriteLine(LogType.Net,
                                            msg.Username + " requesting to join. Verifying public key with auth server.");
                                        pendingSessions.Add(msg.ID, inc.SenderConnection);
                                        var message = NetManager.EncodeMessage(msg);

                                        var receiver =
                                            new IPEndPoint(
                                                NetUtility.Resolve(Server.IO.Config.Server.AuthServerAddress),
                                                Server.IO.Config.Server.AuthServerPort); // Auth Server info
                                        //Send public key to auth server to verify if session is valid
                                        NetManager.NetServer.SendUnconnectedMessage(message, receiver);
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
                                //The lines below can get spammy, used to show when statuses change
                                //Logger.WriteLine(LogType.Net, "Status: {0} changed: {1}", "",inc.SenderConnection.Status.ToString());

                                //When a Users connection is finalized
                                if (inc.SenderConnection != null &&
                                    inc.SenderConnection.Status == NetConnectionStatus.Connected)
                                {
                                    //Log message

                                    break;
                                }
                                //When a client disconnects
                                if (inc.SenderConnection != null &&
                                    (inc.SenderConnection.Status == NetConnectionStatus.Disconnected ||
                                     inc.SenderConnection.Status == NetConnectionStatus.Disconnecting))
                                {
                                }
                                break;
                            }
                            //Listen to data from the auth server.
                            case NetIncomingMessageType.UnconnectedData:
                            {
                                    if (Equals(inc.SenderEndPoint.Address, Dns.GetHostEntry(Globals.Values.DefaultAuthAddress).AddressList[0]) && inc.SenderEndPoint.Port == Globals.Values.DefaultAuthPort)           
                                    {
                                    var type =
                                        (MessageTypes)Enum.Parse(typeof (MessageTypes), inc.ReadByte().ToString());

                                    switch (type)
                                    {
                                        //When the auth server confirms if a session is valid or not.
                                        case MessageTypes.ValidSession:
                                        {
                                            var msg = new ValidSessionMessage(inc, MessageContext.Server);
                                            if (msg.Valid)
                                            {
                                                pendingSessions[msg.ID].Approve(
                                                    NetManager.EncodeMessage(new InitMessage()));
                                                pendingSessions.Remove(msg.ID);
                                                Logger.WriteLine(LogType.Net,
                                                    $"Session valid for '{msg.ID}'. (Allowed)");
                                            }
                                            else
                                            {
                                                pendingSessions[msg.ID].Deny("Invalid or expired session.");
                                                pendingSessions.Remove(msg.ID);
                                                Logger.WriteLine(LogType.Net,
                                                    $"Session invalid for '{msg.ID}'. (Denied)");
                                            }

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
                Name = "Auth Message Handler",
                Priority = ThreadPriority.AboveNormal
            };
            networkThread.Start();
        }

        /// <summary>
        /// Handles all actions for recieving data messages (Such as movement, block placing, etc)
        /// </summary>
        /// <param name="inc">The incoming message</param>
        private async void ProcessDataMessage(NetIncomingMessage inc)
        {
            var type = (MessageTypes)Enum.Parse(typeof (MessageTypes), inc.ReadByte().ToString());
            switch (type)
            {
              
            }
        }

        private Thread networkThread;

        public MessageHandler(Server server)
        {
            Server = server;
        }
    }
}