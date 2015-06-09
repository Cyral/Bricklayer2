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
    internal class Game : Microsoft.Xna.Framework.Game
    {
        private static GameState state;

        /// <summary>
        /// The game's graphics manager.
        /// </summary>
        public static GraphicsDeviceManager Graphics { get; private set; }

        /// <summary>
        /// The game's sprite batch for drawing content.
        /// </summary>
        public static SpriteBatch SpriteBatch { get; private set; }

        /// <summary>
        /// The main window, which is the root of all UI controls.
        /// </summary>
        public static MainWindow Window { get; private set; }

        /// <summary>
        /// The MonoForce UI manager.
        /// </summary>
        public static Manager UIManager { get; private set; }

        /// <summary>
        /// Texture loader for loading texture assets.
        /// </summary>
        public static TextureLoader TextureLoader { get; private set; }

        /// <summary>
        /// Manages and handles all game assets and content.
        /// </summary>
        public static new ContentManager Content { get; private set; }

        /// <summary>
        /// Handles receiving and sending of auth server network messages
        /// </summary>
        public static AuthNetworkManager AuthNetwork { get; private set; }


        public static EventManager Events { get; private set; }

        /// <summary>
        /// Handles receiving and sending of game server network messages
        /// </summary>
        public static NetworkManager Network { get; private set; }

        /// <summary>
        /// Contains the 2 keys used for authentication
        /// </summary>
        internal static Token TokenKeys { get; set; }

        KeyboardState oldKeyboardState,
        currentKeyboardState;

        public GameState State
        {
            get { return state; }
            set
            {
                Events.Game.StateChanged.Invoke(new EventManager.GameEvents.GameStateEventArgs(State, value));
                state = value; 
            }
        }

        public Game()
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

            currentKeyboardState = new KeyboardState();

            AuthNetwork = new AuthNetworkManager();
            AuthNetwork.Init();

            Network = new NetworkManager();
            Network.Init();

            TokenKeys = new Token();

            // Listen for init response from auth server containing token keys
            AuthNetwork.Handler.Init += (sender, id, privateKey, publicKey) =>
            {
                TokenKeys.UID = id;
                TokenKeys.PrivateKey = privateKey;
                TokenKeys.PublicKey = publicKey;
                Debug.WriteLine("Recieved Tokens:\nPrivate Key: " + privateKey + "\nPublic Key: " + publicKey);
            };

            // Listen for failed login response from auth server
            AuthNetwork.Handler.FailedLogin += (sender, errorMessage) =>
            {
                Debug.WriteLine("Failed to login. Error Message: " + errorMessage);
            };

            // Listen for verification result from the auth server
            AuthNetwork.Handler.Verified += (sender, verified) =>
            {
                if (verified)
                {
                    Debug.WriteLine("Session verification Successful");
                    Connect(); // Start connection process with game server once it gets session verification from the auth server
                }
                else
                    Debug.WriteLine("Session verification failed");
            };

            // Listen for when user is fully connected to game server
            Network.Handler.Connect += (sender) =>
            {
                Debug.WriteLine("Now connected to server!");
            };

            // If user was disconnected from the server
            Network.Handler.Disconnect += (sender, reason) =>
            {
                Debug.WriteLine("Connection to game server failed: " + reason);
            };

            // Connect to Auth Server. Tempoary testing method for the auth server. Will be removed
            ConnectToAuth();
        }

        // These three methods are here for testing reasons for the Auth system. They will be removed
        public async void ConnectToAuth()
        {
            await AuthNetwork.SendDetails("pugmatt", "$2y$10$4Vk/vuxSZQeXQdwpwQ14s.qYzShVXZX/33GlmelkCmEbq3v/XPfB6");
        }

        public void SendSessionRequest()
        {
            AuthNetwork.Send(new SessionMessage("pugmatt", TokenKeys.UID, TokenKeys.PrivateKey, IPAddress.Parse("127.0.0.1"), Globals.Values.DefaultServerPort));
        }

        public async void Connect()
        {
            await Network.Connect("127.0.0.1", Globals.Values.DefaultServerPort, "pugmatt", TokenKeys.UID, TokenKeys.PublicKey);
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
            Content.LoadTextures();

            Graphics.PreferredBackBufferWidth = 1024;
            Graphics.PreferredBackBufferHeight = 768;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            Graphics.ApplyChanges();

            //Initialize MonoForce after loading skins.
            UIManager.Initialize();
            SpriteBatch = UIManager.Renderer.SpriteBatch; //Set the spritebatch to the Neoforce managed one


            //Create the main window for all content to be added to.
            Window = new MainWindow(UIManager);
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
            base.Update(gameTime);

            oldKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            if ((currentKeyboardState.IsKeyUp(Keys.Space)) && (oldKeyboardState.IsKeyDown(Keys.Space)))
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
