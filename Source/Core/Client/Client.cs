using System.Diagnostics;
using System.Net;
using Bricklayer.Client.Interface;
using Bricklayer.Core.Client.Net.Messages.AuthServer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoForce.Controls;
using Bricklayer.Core.Client.Net.Messages.GameServer;
using Bricklayer.Core.Common;
using Bricklayer.Core.Common.Net.Messages;
using Microsoft.Xna.Framework.Input;

namespace Bricklayer.Core.Client
{
    /// <summary>
    /// The main class of the Bricklayer client.
    /// </summary>
    public class Client : Game
    {
        /// <summary>
        /// The game's graphics manager.
        /// </summary>
        public GraphicsDeviceManager Graphics { get; private set; }

        /// <summary>
        /// The game's sprite batch for drawing content.
        /// </summary>
        public SpriteBatch SpriteBatch { get; private set; }

        /// <summary>
        /// The main window, which is the root of all UI controls.
        /// </summary>
        public MainWindow Window { get; private set; }

        /// <summary>
        /// The MonoForce UI manager.
        /// </summary>
        public Manager UIManager { get; private set; }

        /// <summary>
        /// Texture loader for loading texture assets.
        /// </summary>
        internal TextureLoader TextureLoader { get; private set; }

        /// <summary>
        /// Manages and handles all game assets and content.
        /// </summary>
        public new ContentManager Content { get; private set; }

        /// <summary>
        /// Handles receiving and sending of auth server network messages
        /// </summary>
        public AuthNetworkManager AuthNetwork { get; private set; }


        public EventManager Events { get; private set; }

        /// <summary>
        /// Handles receiving and sending of game server network messages
        /// </summary>
        public NetworkManager Network { get; private set; }

        /// <summary>
        /// Contains the 2 keys used for authentication
        /// </summary>
        internal Token TokenKeys { get; private set; }

        /// <summary>
        /// Manages and handles game input from the mouse and keyboard.
        /// </summary>
        public InputHandler Input { get; private set; }

        /// <summary>
        /// The current state of the game. (Login, server list, in game, etc.)
        /// </summary>
        public GameState State
        {
            get { return state; }
            set
            {
                Events.Game.StateChanged.Invoke(new EventManager.GameEvents.GameStateEventArgs(State, value));
                state = value; 
            }
        }
        private GameState state;

        public Client()
        {
            Events = new EventManager();
            Graphics = new GraphicsDeviceManager(this) { SynchronizeWithVerticalRetrace = false };
            IsFixedTimeStep = false;

            //Create the manager for MonoForce UI
            UIManager = new Manager(this, "Bricklayer")
            {
                AutoCreateRenderTarget = true,
                LogUnhandledExceptions = false,
                ShowSoftwareCursor = true
            };
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
           
            Input = new InputHandler();

            AuthNetwork = new AuthNetworkManager(this);
            AuthNetwork.Init();

            Network = new NetworkManager(this);
            Network.Init();

            TokenKeys = new Token();

            // Listen for init response from auth server containing token keys
            Events.Network.Auth.Init.AddHandler(args =>
            {
                 TokenKeys.UID = args.DatabaseId;
                TokenKeys.PrivateKey = args.PrivateKey;
                TokenKeys.PublicKey = args.PublicKey;
                Debug.WriteLine("Recieved Tokens:\nPrivate Key: " + args.PrivateKey + "\nPublic Key: " + args.PublicKey);
            });

            // Listen for failed login response from auth server
            Events.Network.Auth.FailedLogin.AddHandler(args =>
            {
                Debug.WriteLine("Failed to login. Error Message: " + args.ErrorMessage);
            });

            // Listen for verification result from the auth server
            Events.Network.Auth.Verified.AddHandler(args =>
            {
                if (args.Verified)
                {
                    Debug.WriteLine("Session verification Successful");
                    Connect(); // Start connection process with game server once it gets session verification from the auth server
                }
                else
                    Debug.WriteLine("Session verification failed");
            });

            // Listen for when user is fully connected to game server
            Events.Network.Game.Connect.AddHandler(args =>
            {
                Debug.WriteLine("Now connected to a server!");
            });
            // If user was disconnected from the server
            Events.Network.Game.Disconnect.AddHandler(args =>
            {
                Debug.WriteLine("Disconnected or connection failed.");
            });

            // Connect to Auth Server. Tempoary testing method for the auth server. Will be removed
            ConnectToAuth();
        }

        // These three methods are here for testing reasons for the Auth system. They will be removed
        public async void ConnectToAuth()
        {
            await AuthNetwork.SendDetails("Test", "test");
        }

        public void SendSessionRequest()
        {
            AuthNetwork.Send(new SessionMessage("Test", TokenKeys.UID, TokenKeys.PrivateKey, IPAddress.Parse("71.3.34.68"), Globals.Values.DefaultServerPort));
        }

        public async void Connect()
        {
            await Network.Connect("127.0.0.1", Globals.Values.DefaultServerPort, "Test", TokenKeys.UID, TokenKeys.PublicKey);
        }
        //


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            IO.CheckFiles();
            Content = new ContentManager();
            TextureLoader = new TextureLoader(Graphics.GraphicsDevice);
            Content.LoadTextures(this);

            Graphics.PreferredBackBufferWidth = 1024;
            Graphics.PreferredBackBufferHeight = 768;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            Graphics.ApplyChanges();

            //Initialize MonoForce after loading skins.
            UIManager.Initialize();
            SpriteBatch = UIManager.Renderer.SpriteBatch; //Set the spritebatch to the Neoforce managed one


            //Create the main window for all content to be added to.
            Window = new MainWindow(UIManager, this);
            Window.Init();
            UIManager.Add(Window);
            Window.SendToBack();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values. (Delta time)</param>
        protected override void Update(GameTime gameTime)
        {
            UIManager.Update(gameTime);
            Input.Update();
            base.Update(gameTime);

            if (Input.IsKeyPressed(Keys.Space))
            {
                SendSessionRequest();
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values. (Delta time)</param>
        protected override void Draw(GameTime gameTime)
        {
            UIManager.BeginDraw(gameTime);
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here

            UIManager.EndDraw();
            base.Draw(gameTime);
        }
    }
}
