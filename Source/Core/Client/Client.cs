using System;
using System.Linq;
using Bricklayer.Core.Client.Components;
using Bricklayer.Core.Client.Interface;
using Bricklayer.Core.Common.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoForce.Controls;
using EventArgs = System.EventArgs;
using Level = Bricklayer.Core.Client.World.Level;

namespace Bricklayer.Core.Client
{
    /// <summary>
    /// The main class of the Bricklayer client.
    /// </summary>
    public class Client : Game
    {
        /// <summary>
        /// Manages and handles all game assets and content.
        /// </summary>
        public new ContentComponent Content { get; private set; }

        public EventManager Events { get; }

        /// <summary>
        /// The game's graphics manager.
        /// </summary>
        public GraphicsDeviceManager Graphics { get; }

        /// <summary>
        /// Manages and handles game input from the mouse and keyboard.
        /// </summary>
        public InputHandler Input { get; private set; }

        /// <summary>
        /// Manages and handles file operations.
        /// </summary>
        public IOComponent IO { get; set; }

        /// <summary>
        /// The PluginComponent for loading and managing plugins.
        /// </summary>
        public PluginComponent Plugins { get; set; }

        /// <summary>
        /// The game's sprite batch for drawing content.
        /// </summary>
        internal SpriteBatch SpriteBatch { get; private set; }

        /// <summary>
        /// The current state of the game. (Login, server list, in game, etc.)
        /// </summary>
        public GameState State
        {
            get { return state; }
            internal set
            {
                if (State != value)
                {
                    Events.Game.StateChanged.Invoke(new EventManager.GameEvents.GameStateEventArgs(State, value));
                    state = value;
                }
            }
        }

        /// <summary>
        /// The MonoForce UI manager.
        /// </summary>
        public Manager UI { get; }

        /// <summary>
        /// The current level.
        /// </summary>
        public Level Level { get; internal set; }

        /// <summary>
        /// The main window, which is the root of all UI controls.
        /// </summary>
        public new MainWindow Window { get; private set; }

        /// <summary>
        /// Handles receiving and sending of game server network messages
        /// </summary>
        internal NetworkManager Network { get; private set; }

        private RenderTarget2D backgroundTarget, foregroundTarget, lightingTarget;
        private GameState state;

        internal Client()
        {
            Events = new EventManager();
            Graphics = new GraphicsDeviceManager(this);
            IsFixedTimeStep = false;
            Graphics.SynchronizeWithVerticalRetrace = false;

            AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;

            // Create the manager for MonoForce UI
            UI = new Manager(this, "Bricklayer")
            {
                TargetFrames = 10000,
                AutoCreateRenderTarget = true,
                LogUnhandledExceptions = false,
                ShowSoftwareCursor = true
            };
        }

        private void LogUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // TODO: Save to a file (not important until later)
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values. (Delta time)</param>
        protected override void Draw(GameTime gameTime)
        {
            UI.BeginDraw(gameTime);
            GraphicsDevice.Clear(Color.Black);

            if (State == GameState.Game)
            {
                GraphicsDevice.SetRenderTarget(backgroundTarget);
                GraphicsDevice.Clear(Color.Transparent);

                // Draw level background.
                SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                // Draw plugins before anything is drawn.
                foreach (var plugin in Plugins.Plugins.Where(x => x.IsEnabled))
                    plugin.Draw(DrawPass.Before, SpriteBatch, gameTime);
                Level?.DrawBackground(SpriteBatch, gameTime);

                // Draw plugins after the background layer has been drawn.
                foreach (var plugin in Plugins.Plugins.Where(x => x.IsEnabled))
                    plugin.Draw(DrawPass.Background, SpriteBatch, gameTime);
                SpriteBatch.End();

                // Draw level foreground.
                GraphicsDevice.SetRenderTarget(foregroundTarget);
                GraphicsDevice.Clear(Color.Transparent);
                SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Level?.DrawForeground(SpriteBatch, gameTime);

                // Draw plugins after the foreground layer has been drawn.
                foreach (var plugin in Plugins.Plugins.Where(x => x.IsEnabled))
                    plugin.Draw(DrawPass.Foreground, SpriteBatch, gameTime);
                SpriteBatch.End();


                // Draw lighting and shadows.
                GraphicsDevice.SetRenderTarget(lightingTarget);
                GraphicsDevice.Clear(Color.Transparent);


                SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                Content.Effects["blur"].CurrentTechnique.Passes[0].Apply();
                Content.Effects["blur"].Parameters["Resolution"]?.SetValue(
                    new Vector2(GraphicsDevice.PresentationParameters.BackBufferWidth,
                        GraphicsDevice.PresentationParameters.BackBufferHeight));

                SpriteBatch.Draw(foregroundTarget,
                    new Rectangle(0, 2, GraphicsDevice.PresentationParameters.BackBufferWidth,
                        GraphicsDevice.PresentationParameters.BackBufferHeight), Color.White);
                //SpriteBatch.Draw(Content["gui.background"], new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight), Color.White);
                SpriteBatch.End();

                GraphicsDevice.SetRenderTarget(null);
                SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                SpriteBatch.Draw(backgroundTarget,
                    new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth,
                        GraphicsDevice.PresentationParameters.BackBufferHeight), Color.White);
                SpriteBatch.Draw(lightingTarget,
                    new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth,
                        GraphicsDevice.PresentationParameters.BackBufferHeight), Color.White);
                SpriteBatch.Draw(foregroundTarget,
                    new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth,
                        GraphicsDevice.PresentationParameters.BackBufferHeight), Color.White);
                SpriteBatch.End();
            }

            // Draw plugins after everthing is drawn.
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            foreach (var plugin in Plugins.Plugins.Where(x => x.IsEnabled))
                plugin.Draw(DrawPass.After, SpriteBatch, gameTime);
            SpriteBatch.End();

            UI.EndDraw();

            base.Draw(gameTime);
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

            SpriteBatch = new SpriteBatch(Graphics.GraphicsDevice);
            Input = new InputHandler();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override async void LoadContent()
        {
            // Initialize components.
            IO = new IOComponent(this);
            Content = new ContentComponent(this);
            Plugins = new PluginComponent(this);
            Network = new NetworkManager(this);

            await IO.Init();
            await Network.Init();

            // Set resolution.
            if (IO.Config.Client.Resolution.X == 0 && IO.Config.Client.Resolution.Y == 0)
            {
                Graphics.PreferredBackBufferWidth = (int) (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width*.9);
                Graphics.PreferredBackBufferHeight =
                    (int) (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height*.8);
                Graphics.ApplyChanges();
                base.Window.Position =
                    new Point(
                        (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - Graphics.PreferredBackBufferWidth)/
                        2,
                        (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - Graphics.PreferredBackBufferHeight)/
                        2);
            }
            else
            {
                Graphics.PreferredBackBufferWidth = IO.Config.Client.Resolution.X;
                Graphics.PreferredBackBufferHeight = IO.Config.Client.Resolution.Y;
                Graphics.ApplyChanges();
            }

            await Content.Init();

            // Initialize MonoForce after loading skins.
            UI.Initialize();
            SpriteBatch = UI.Renderer.SpriteBatch; // Set the spritebatch to the Neoforce managed one

            // Create render targets.
            backgroundTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight);
            foregroundTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight);
            lightingTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight);

            // Create the main window for all content to be added to.
            Window = new MainWindow(UI, this);
            Window.Init();
            UI.Add(Window);
            Window.SendToBack();

            // Unlike the server, the clientside plugins must be loaded last.
            Events.Game.PluginStatusChanged.AddHandler((args =>
            {
                if (!args.Enabled) return;
                // Set any block textures that need to be set.
                foreach (var block in BlockType.Blocks.Where(x => x.IsRenderable && x.Texture == null))
                {
                    var str = "blocks." + (block.Pack != null ? block.Pack + "." : string.Empty) + block.Name;
                    block.Texture = Content[str];
                }
            }));
            await Plugins.Init();
            base.LoadContent();
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
        protected override async void Update(GameTime gameTime)
        {
            UI.Update(gameTime);

            foreach (var plugin in Plugins.Plugins)
                plugin.Update(gameTime);

            if (State == GameState.Game)
            {
                Level?.Update(gameTime);
            }

            base.Update(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            Network.Disconnect("Exited game.");
            base.OnExiting(sender, args);
        }
    }
}