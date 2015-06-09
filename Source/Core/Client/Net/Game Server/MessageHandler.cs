using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using Bricklayer.Core.Common.Net;
using Bricklayer.Core.Common.Net.Messages;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace Bricklayer.Core.Client.Net.Messages.GameServer
{
    public class MessageHandler
    {
        #region Properties

        private Thread MessageThread { get; set; }

        #endregion

        public MessageHandler(NetworkManager networkManager)
        {
            this.networkManager = networkManager;
        }

        public void Start()
        {
            MessageThread = new Thread(ProcessNetworkMessages)
            {
                Name = "Message Handler"
            };
            MessageThread.SetApartmentState(ApartmentState.STA);
            MessageThread.Start();
        }

        /// <summary>
        /// The process network messages such as player's joining, moving, etc
        /// </summary>
        private void ProcessNetworkMessages()
        {
            NetIncomingMessage im; //Holder for the incoming message

            while (networkManager.Client.Status == NetPeerStatus.Running)
            {
                if (networkManager.Client != null)
                {
                    //Block thread until next message
                    networkManager.Client.MessageReceivedEvent.WaitOne();

                    while ((im = networkManager.ReadMessage()) != null)
                    {
                        switch (im.MessageType)
                        {
                            case NetIncomingMessageType.ConnectionLatencyUpdated:
                                OnLatencyUpdated(im.ReadSingle());
                                break;
                            case NetIncomingMessageType.VerboseDebugMessage:
                            case NetIncomingMessageType.DebugMessage:
                            case NetIncomingMessageType.WarningMessage:
                            case NetIncomingMessageType.ErrorMessage:
                                Debug.WriteLine(im.ToString());
                                break;
                            case NetIncomingMessageType.StatusChanged:
                                {
                                    var status = (NetConnectionStatus)im.ReadByte();
                                    switch (status)
                                    {
                                        case NetConnectionStatus.None:
                                            {
                                                OnDisconnect("Error connecting to the server.");
                                                break;
                                            }
                                        //When connected to the server
                                        case NetConnectionStatus.Connected:
                                            {
                                                //Must read the first byte of the hail message, which should correspond to the byte of the Init type
                                                im.SenderConnection.RemoteHailMessage.ReadByte(); //Throw it away
                                                var msg = new InitMessage(im.SenderConnection.RemoteHailMessage, MessageContext.Client);
                                                OnConnect();
                                                break;
                                            }
                                        //When disconnected from the server
                                        case NetConnectionStatus.Disconnected:
                                            {
                                                var reason = im.ReadString();
                                                OnDisconnect(reason);
                                                break;
                                            }
                                        case NetConnectionStatus.RespondedAwaitingApproval:
                                            {
                                                im.SenderConnection.Approve(networkManager.CreateMessage());
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case NetIncomingMessageType.Data:
                                {
                                    HandleDataMessage(im);
                                    break;
                                }
                        }
                        networkManager.Recycle(im);
                    }
                }
            }
        }

        /// <summary>
        /// Handles a data message (The bulk of all messages received, containing player movements, block places, etc)
        /// </summary>
        private void HandleDataMessage(NetIncomingMessage im)
        {
            if (im == null) throw new ArgumentNullException("im");

            var messageType = (MessageTypes)im.ReadByte(); //Find the type of data message sent
//            switch (messageType)
//            {
//                case MessageTypes.Chat:
//                    {
//                        var msg = new ChatMessage(im, MessageContext.Client);
//                        if (!msg.System)
//                            OnChat(new ChatLine(App.Session.Users.FirstOrDefault(user => user.ID == msg.UserID), msg.Text,
//                                DateTime.Now));
//#if !MONO
//                        else
//                            OnChat(new ChatLine(msg.Color, msg.Text,
//                                DateTime.Now));
//#endif
//                        break;
//                    }
//            }
        }

        #region Events

        //Event Arguments & Handlers

        #region Delegates

        public delegate void LatencyUpdatedEventHandler(object sender, float ping);

        public delegate void ConnectEventHandler(object sender);

        public delegate void DisconnectEventHandler(object sender, string reason);


        #endregion

        //Public Events
        public event LatencyUpdatedEventHandler LatencyUpdated;
        public event ConnectEventHandler Connect;
        public event DisconnectEventHandler Disconnect;

        //Private Callers
        private void OnLatencyUpdated(float ping) => LatencyUpdated?.Invoke(this, ping);

        private void OnConnect()
            => Connect?.Invoke(this);

        private void OnDisconnect(string reason = "") => Disconnect?.Invoke(this, reason);

        #endregion //Events

        #region Fields

        private readonly NetworkManager networkManager;

        private bool recievedInit = false; //Have we recieved the init message yet?

        #endregion //Fields
    }
}
