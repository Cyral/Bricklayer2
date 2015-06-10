using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using Bricklayer.Core.Common;
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

            while (networkManager.NetClient.Status == NetPeerStatus.Running)
            {
                if (networkManager.NetClient != null)
               {
                    //Block thread until next message
                    networkManager.NetClient.MessageReceivedEvent.WaitOne();

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
                                                networkManager.Client.Events.Network.Game.Disconnected.Invoke(
                                                new EventManager.NetEvents.GameServerEvents.DisconnectedEventArgs("Error connecting to the server."));
                                                break;
                                            }
                                        //When connected to the server
                                        case NetConnectionStatus.Connected:
                                            {
                                                //Must read the first byte of the hail message, which should correspond to the byte of the Init type
                                                im.SenderConnection.RemoteHailMessage.ReadByte(); //Throw it away
                                                var msg = new InitMessage(im.SenderConnection.RemoteHailMessage, MessageContext.Client);
                                                networkManager.Client.Events.Network.Game.Connected.Invoke(
                                               new EventManager.NetEvents.GameServerEvents.ConnectedEventArgs());
                                                break;
                                            }
                                        //When disconnected from the server
                                        case NetConnectionStatus.Disconnected:
                                            {
                                                var reason = im.ReadString();
                                                networkManager.Client.Events.Network.Game.Disconnected.Invoke(
                                               new EventManager.NetEvents.GameServerEvents.DisconnectedEventArgs(reason));
                                                break;
                                            }
                                        case NetConnectionStatus.RespondedAwaitingApproval:
                                            {
                                                im.SenderConnection.Approve();
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
                            case NetIncomingMessageType.UnconnectedData:
                                {
                                    if (im.SenderEndPoint.Address.ToString() == Globals.Values.DefaultAuthAddress && im.SenderEndPoint.Port == Globals.Values.DefaultAuthPort)
                                        HandleUnconnectedMessage(im);
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
        private void HandleUnconnectedMessage(NetIncomingMessage im)
        {
            if (im == null) throw new ArgumentNullException(nameof(im));

            if (im.SenderEndPoint.Address.ToString() == Globals.Values.DefaultAuthAddress
                && im.SenderEndPoint.Port == Globals.Values.DefaultAuthPort) // Check if incoming data is from real auth server
            {
                var messageType = (MessageTypes)im.ReadByte(); //Find the type of data message sent
                switch (messageType)
                {
                    case MessageTypes.AuthInit:
                        {
                            var msg = new AuthInitMessage(im, MessageContext.Client);
                            networkManager.Client.Events.Network.Auth.Init.Invoke(
                                new EventManager.NetEvents.AuthServerEvents.InitEventArgs(msg.UID, msg.PrivateKey, msg.PublicKey));
                            break;
                        }
                    case MessageTypes.FailedLogin:
                        {
                            var msg = new FailedLoginMessage(im, MessageContext.Client);
                            networkManager.Client.Events.Network.Auth.FailedLogin.Invoke(
                               new EventManager.NetEvents.AuthServerEvents.FailedLoginEventArgs(msg.ErrorMessage));
                            break;
                        }
                    case MessageTypes.Verified:
                        {
                            var msg = new VerifiedMessage(im, MessageContext.Client);
                            networkManager.Client.Events.Network.Auth.Verified.Invoke(
                                new EventManager.NetEvents.AuthServerEvents.VerifiedEventArgs(msg.Verified));
                            break;
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

            var messageType = (MessageTypes)im.ReadByte(); 
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
