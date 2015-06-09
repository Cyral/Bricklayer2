using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Net;
using Lidgren.Network;
using Bricklayer.Core.Common.Net.Messages;
using System.Net;

namespace Bricklayer.Core.Server.Net
{
    /// <summary>
    /// Handles processing of incoming messages
    /// </summary>
    public class MessageHandler
    {
        #region Properties

        public NetworkManager NetManager
        {
            get { return Server.Net; }
        }

        #endregion

        /// <summary>
        /// Stored pending user sessions
        /// </summary>
        internal Dictionary<string, NetConnection> PendingSessions = new Dictionary<string, NetConnection>();

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
        /// The process network messages such as users joining, moving, etc
        /// </summary>
        public async void ProcessNetworkMessages()
        {
            //Used to store messages
            NetIncomingMessage inc;

            while (Server.Net.NetServer.Status == NetPeerStatus.Running)
            {
                //If an issue occurs with this (again), see the discussion at:
                //https://groups.google.com/forum/#!topic/lidgren-network-gen3/EN3vaykwBWM
                Server.Net.NetServer.MessageReceivedEvent.WaitOne();
                try
                {
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
                                    if (inc.LengthBytes == 0)
                                    //If client does not send username information (Purposely trying to use a modded client?)
                                    {
                                        inc.SenderConnection?.Deny("Invalid Hail Message");
                                        break;
                                    }

                                    var type =
                                        (MessageTypes)Enum.Parse(typeof(MessageTypes), inc.ReadByte().ToString());
                                    //Find message type
                                    switch (type)
                                    {

                                        case MessageTypes.PublicKey: // The connection should come with a public key to verify the client's session
                                        {
                                                var msg = new PublicKeyMessage(inc, MessageContext.Client);
                                                Logger.WriteLine(LogType.Net, msg.Username + " requesting to join. Verifying session..");
                                                PendingSessions.Add(msg.Username, inc.SenderConnection);
                                                var message = NetManager.EncodeMessage(new PublicKeyMessage(msg.Username, msg.PublicKey)); //Write packet ID and encode
                                                IPEndPoint receiver = new IPEndPoint(NetUtility.Resolve(Globals.Strings.AuthServerAddress), Globals.Strings.AuthServerPort); // Auth Server info
                                                NetManager.NetServer.SendUnconnectedMessage(message, receiver); //Send public key to auth server to verify if session is valid

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
                                    //Log.WriteLine(LogType.Status, "Status: {0} changed: {1}", name, inc.SenderConnection.Status.ToString());

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
                            case NetIncomingMessageType.UnconnectedData:
                            {
                                    if (inc.SenderEndPoint.Address.ToString() == Globals.Strings.AuthServerAddress && inc.SenderEndPoint.Port == Globals.Strings.AuthServerPort)
                                    {
                                        var type = (MessageTypes)Enum.Parse(typeof(MessageTypes), inc.ReadByte().ToString());

                                        switch (type)
                                        {
                                            case MessageTypes.ValidSession:
                                                {
                                                    var msg = new ValidSessionMessage(inc, MessageContext.Client);
                                                    if (msg.Valid)
                                                    {
                                                        PendingSessions[msg.Username].Approve(NetManager.EncodeMessage(new InitMessage()));
                                                        PendingSessions.Remove(msg.Username);
                                                        Logger.WriteLine(LogType.Net, "Allowing connection to for " + msg.Username);
                                                    }
                                                    else
                                                    {
                                                        PendingSessions[msg.Username].Deny("Invalid or expired session");
                                                        PendingSessions.Remove(msg.Username);
                                                        Logger.WriteLine(LogType.Net, "Invalid or expired session. Disallowing connection to for " + msg.Username);
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
        /// Handles all actions for recieving data messages (Such as movement, block placing, etc)
        /// </summary>
        /// <param name="inc">The incoming message</param>
        private async void ProcessDataMessage(NetIncomingMessage inc)
        {

            var type = (MessageTypes)Enum.Parse(typeof(MessageTypes), inc.ReadByte().ToString());
            switch (type)
            { 
            }
        }

        //private async Task HandleRequestMessage(NetIncomingMessage inc, User sender, Room room, MessageTypes type)
        //{
        //    switch (type)
        //    {
        //       
        //    }
        //}

        #region Fields

        private Thread networkThread;

        public Dictionary<short, User> Trackers = new Dictionary<short, User>();
        //Store tracker entities so they can be removed if player disconnects

        #endregion
    }
}
