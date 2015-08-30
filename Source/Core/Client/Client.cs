using System;
using System.Linq;
using Bricklayer.Core.Client.Components;
using Bricklayer.Core.Client.Interface;
using Bricklayer.Core.Common.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        public NetworkManager Network { get; private set; }

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

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values. (Delta time)</param>
        protected override void Draw(GameTime gameTime)
        {
            if (UI == null || GraphicsDevice == null) return;
            UI.BeginDraw(gameTime);
            GraphicsDevice.Clear(Color.Black);

            if (State == GameState.Game && Level != null)
            {
                GraphicsDevice.SetRenderTarget(backgroundTarget);
                GraphicsDevice.Clear(Color.Transparent);

                // Draw level background.
                SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null,
                    null, Level.Camera.GetViewMatrix(Vector2.One));
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
                SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null,
                    null, Level.Camera.GetViewMatrix(Vector2.One));
                Level?.DrawForeground(SpriteBatch, gameTime);

                // Draw plugins after the foreground layer has been drawn.
                foreach (var plugin in Plugins.Plugins.Where(x => x.IsEnabled))
                    plugin.Draw(DrawPass.Foreground, SpriteBatch, gameTime);
                SpriteBatch.End();


                // Draw lighting and shadows.
                GraphicsDevice.SetRenderTarget(lightingTarget);
                GraphicsDevice.Clear(Color.Transparent);

                SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null,
                    null, Level.Camera.GetViewMatrix(Vector2.One));
                Content.Effects["blur"].CurrentTechnique.Passes[0].Apply();
                Content.Effects["blur"].Parameters["Resolution"]?.SetValue(
                    new Vector2(GraphicsDevice.PresentationParameters.BackBufferWidth,
                        GraphicsDevice.PresentationParameters.BackBufferHeight));

                SpriteBatch.Draw(foregroundTarget,
                    new Rectangle(0, 2, GraphicsDevice.PresentationParameters.BackBufferWidth,
                        GraphicsDevice.PresentationParameters.BackBufferHeight), Color.White);
                SpriteBatch.End();

                GraphicsDevice.SetRenderTarget(null);
                SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                SpriteBatch.Draw(backgroundTarget, Window.ControlRect, Color.White);
                SpriteBatch.Draw(lightingTarget, Window.ControlRect, Color.White);
                SpriteBatch.Draw(foregroundTarget, Window.ControlRect, Color.White);
                SpriteBatch.End();
            }

            // Draw plugins after everthing is drawn.
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            foreach (var plugin in Plugins.Plugins.Where(x => x.IsEnabled))
                plugin.Draw(DrawPass.After, SpriteBatch, gameTime);
            SpriteBatch.End();
            UI.EndDraw();

            // Draw parts of the screen manager (fade image) over everything else.
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Window.Draw(SpriteBatch, gameTime);
            SpriteBatch.End();

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

            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Extensions.Setup(GraphicsDevice);
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

            // Set block textures and create texture atlas when content is loaded.
            Events.Game.ContentLoaded.AddHandler((args => { SetBlockTexturesAndAtlas(); }));

            // Unlike the server, the clientside plugins must be loaded last.
            await Plugins.Init();
            Events.Game.ContentLoaded.Invoke(new EventManager.GameEvents.ContentLoadEventArgs(Content));

            // Set block textures and create texture atlas when plugins are reloaded.
            Events.Game.PluginStatusChanged.AddHandler((args =>
            {
                if (args.Enabled)
                {
                    SetBlockTexturesAndAtlas();
                    Window.Invalidate();
                }
            }));
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
        protected override void Update(GameTime gameTime)
        {
            UI.Update(gameTime);

            // Update plugin logic.
            foreach (var plugin in Plugins.Plugins)
                plugin.Update(gameTime);

            if (State == GameState.Game)
            {
                // Update level.
                Level?.Update(gameTime);
            }

            // Reload content.
            if (Input.WasKeyPressed(Keys.F3))
            {
#pragma warning disable 4014
                Content.Init();
#pragma warning restore 4014
                Plugins.ReloadContent();
                Events.Game.ContentLoaded.Invoke(new EventManager.GameEvents.ContentLoadEventArgs(Content));
            }

            base.Update(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            Network.Disconnect("Exited game.");
            Plugins.Unload();
            base.OnExiting(sender, args);
        }

        private void LogUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // TODO: Save to a file (not important until later)
        }

        /// <summary>
        /// Whenever new blocks are added, call this method to set their texture and create a texture atlas from the blocks.
        /// </summary>
        private void SetBlockTexturesAndAtlas()
        {
            var items = BlockType.Blocks.Where(x => x.IsRenderable).ToArray();
            // Create a texture atlas for blocks.
            if (items.Length > 1)
            {
                Content.Atlases["blocks"] = TextureAtlas.CreateAtlas(GraphicsDevice,
                    Content.Textures.Where(x => x.Key.StartsWith("blocks")).Select(x => x.Value).ToList());
            }

            foreach (var block in items)
            {
                var str = "blocks\\" + (block.Pack != null ? block.Pack + "\\" : string.Empty) + block.Name;
                block.Image = Content[str];

                //Set the block's average color based on the average color of the tile.
                block.Color = block.Image.Texture.AverageColor(block.SourceRect);
            }
            foreach (var block in BlockType.Blocks.Where(x => !x.IsRenderable))
                block.Color = Color.Black;
        }
    }
}