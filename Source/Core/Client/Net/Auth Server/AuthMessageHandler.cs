using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using Lidgren.Network;
using Bricklayer.Core.Common.Net;
using Bricklayer.Core.Common.Net.Messages;
using Bricklayer.Core.Common;

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
                            case NetIncomingMessageType.UnconnectedData:
                                {
                                   if (im.SenderEndPoint.Address.ToString() == Globals.Strings.AuthServerAddress && im.SenderEndPoint.Port == Globals.Strings.AuthServerPort) 
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

           // if (im.SenderEndPoint.Address.ToString() == Constants.AuthServerAddress && im.SenderEndPoint.Port == Constants.AuthServerPort) // Check if incoming data is from real Auth Server
           // {
                var messageType = (MessageTypes)im.ReadByte(); //Find the type of data message sent
                switch (messageType)
                {
                    case MessageTypes.AuthInit:
                        {
                            var msg = new AuthInitMessage(im, MessageContext.Client);

                            OnInit(msg.PrivateKey, msg.PublicKey);

                            break;
                        }
                    case MessageTypes.FailedLogin:
                        {
                            var msg = new FailedLoginMessage(im, MessageContext.Client);

                            OnFailedLogin(msg.ErrorMessage);

                            break;
                        }
                    case MessageTypes.Verified:
                        {
                            var msg = new VerifiedMessage(im, MessageContext.Client);

                            OnVerified(msg.Verified);

                            break;
                        }
                }
            //}
        }

        #region Events

        //Event Arguments & Handlers

        #region Delegates

        internal delegate void InitEventHandler(object sender, string privateKey, string publicKey);
        internal delegate void FailedLoginEventHandler(object sender, string errorMessage);
        internal delegate void VerifiedEventHandler(object sender, bool verified);

        #endregion

        //Public Events

        internal event InitEventHandler Init;
        internal event FailedLoginEventHandler FailedLogin;
        internal event VerifiedEventHandler Verified;

        //Private Callers
        private void OnInit(string privateKey, string publicKey) => Init?.Invoke(this, privateKey, publicKey);
        private void OnFailedLogin(string errorMessage) => FailedLogin?.Invoke(this, errorMessage);
        private void OnVerified(bool verified) => Verified?.Invoke(this, verified);

        #endregion //Events

        #region Fields

        private readonly AuthNetworkManager networkManager;


        #endregion //Fields
    }
}
