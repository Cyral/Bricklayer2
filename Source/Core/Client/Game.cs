using Bricklayer.Client.Interface;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoForce.Controls;

namespace Bricklayer.Core.Client
{
    /// <summary>
    /// The main class of the Bricklayer client.
    /// </summary>
    internal class Game : Microsoft.Xna.Framework.Game
    {
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

        public Game()
        {
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
        }

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