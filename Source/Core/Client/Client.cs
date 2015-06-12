using System.Diagnostics;
using System.Net;
using Bricklayer.Client.Interface;
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

        public EventManager Events { get; private set; }

        /// <summary>
        /// Handles receiving and sending of game server network messages
        /// </summary>
        internal NetworkManager Network { get; private set; }

        /// <summary>
        /// Manages and handles game input from the mouse and keyboard.
        /// </summary>
        public InputHandler Input { get; private set; }

        /// <summary>
        /// Manages and handles file operations.
        /// </summary>
        public IO IO { get; set; }

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
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected async override void LoadContent()
        {
            base.LoadContent();

            IO = new IO();
            await IO.LoadConfig();
            IO.CheckFiles();

            Network = new NetworkManager(this);
            Network.Init();

            Content = new ContentManager();
            TextureLoader = new TextureLoader(Graphics.GraphicsDevice);
            Content.LoadTextures(this);

            Graphics.PreferredBackBufferWidth = IO.Config.Client.Resolution.X;
            Graphics.PreferredBackBufferHeight = IO.Config.Client.Resolution.Y;
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
                Network.SendSessionRequest("71.3.34.68", Globals.Values.DefaultServerPort);
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
