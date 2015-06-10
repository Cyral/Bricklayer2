using System;
using System.Threading;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Net;
using Bricklayer.Core.Common.Net.Messages;
using Lidgren.Network;

namespace Bricklayer.Core.Client.Net.Messages.AuthServer
{
    public class AuthMessageHandler
    {
        #region Properties

        private Thread MessageThread { get; set; }

        #endregion

        public AuthMessageHandler(AuthNetworkManager networkManager)
        {
            this.networkManager = networkManager;
        }

        public void Start()
        {
            MessageThread = new Thread(ProcessNetworkMessages)
            {
                Name = "Auth Message Handler"
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

        private readonly AuthNetworkManager networkManager;
    }
}
