using System.Linq;
using System.Text;
using Bricklayer.Core.Client.Interface.Controls;
using Bricklayer.Core.Common.Net.Messages;
using Bricklayer.Core.Common.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoForce.Controls;
using Level = Bricklayer.Core.Client.World.Level;

namespace Bricklayer.Core.Client.Interface.Screens
{
    /// <summary>
    /// Main screen used for the in-game/level view.
    /// </summary>
    public class GameScreen : Screen
    {
        /// <summary>
        /// The currently selected block in the user's inventory.
        /// </summary>
        public BlockType SelectedBlock
        {
            get { return selectedBlock; }
            internal set
            {
                if (SelectedBlock != value)
                {
                    Client.Events.Game.Level.SelectedBlockChanged.Invoke(
                        new EventManager.GameEvents.LevelEvents.SelectedBlockChangedEventArgs(value, selectedBlock));
                    selectedBlock = value;
                }
            }
        }

        public override GameState State => GameState.Game;

        /// <summary>
        /// The current level.
        /// </summary>
        public Level Level => Client.Level;

        /// <summary>
        /// Stats label showing FPS.
        /// </summary>
        public Label LblStats { get; private set; }

        /// <summary>
        /// List of chat messages.
        /// </summary>
        public ControlList<ChatDataControl> LstChats { get; private set; }

        /// <summary>
        /// List of players and their pings, for the tablist.
        /// </summary>
        public ControlList<PlayerListDataControl> LstPlayers { get; private set; }

        /// <summary>
        /// Inventory bar/panel with selectable blocks.
        /// </summary>
        public InventoryControl Inventory { get; private set; }

        /// <summary>
        /// Bottom status bar.
        /// </summary>
        public StatusBar SbStats { get; private set; }

        /// <summary>
        /// Textbox for entering chat messages.
        /// </summary>
        public TextBox TxtChat { get; private set; }

        private BlockType selectedBlock;

        /// <summary>
        /// Setup the game UI.
        /// </summary>
        public override void Setup(ScreenManager screenManager)
        {
            base.Setup(screenManager);

            // Status bar.
            SbStats = new StatusBar(Manager);
            SbStats.Init();
            SbStats.Bottom = Manager.ScreenHeight;
            SbStats.Left = 0;
            SbStats.Width = Manager.ScreenWidth;

            LblStats = new Label(Manager) {Top = 4, Left = 8, Width = Manager.ScreenWidth - 16};
            LblStats.Init();

            SbStats.Add(LblStats);
            AddControl(SbStats);

            // Inventory.
            Inventory = new InventoryControl(this, Manager);
            Inventory.Left = Manager.TargetWidth/2 - (Inventory.Width/2);
            Inventory.Init();
            AddControl(Inventory);
            Inventory.StayOnTop = true;

            // Chat.
            TxtChat = new TextBox(Manager);
            TxtChat.Init();
            TxtChat.Left = 8;
            TxtChat.DrawFormattedText = false;
            TxtChat.Bottom = SbStats.Top - 8;
            TxtChat.Width = (int) (Manager.TargetWidth*.4f) - 16; // Remove 16 to align due to invisible scrollbar
            TxtChat.Visible = false;
            TxtChat.Passive = true;
            TxtChat.MaxLength = ChatMessage.MaxChatLength;
            AddControl(TxtChat);

            LstChats = new ControlList<ChatDataControl>(Manager)
            {
                Left = TxtChat.Left,
                Width = TxtChat.Width + 16,
                Height = (int) (Manager.TargetHeight*.25f)
            };
            LstChats.Init();
            LstChats.Color = Color.Transparent;
            LstChats.HideSelection = true;
            LstChats.Passive = true;
            LstChats.HideScrollbars = true;
            LstChats.Top = TxtChat.Top - LstChats.Height;
            AddControl(LstChats);

            // Tablist.
            LstPlayers = new ControlList<PlayerListDataControl>(Manager)
            {
                Width = 256,
                Top = 256
            };
            LstPlayers.Init();
            LstPlayers.HideSelection = true;
            LstPlayers.Left = Manager.TargetWidth/2 - (LstPlayers.Width/2);
            LstPlayers.Passive = true;
            LstPlayers.HideScrollbars = true;
            LstPlayers.Visible = false;
            AddControl(LstPlayers);

            foreach (var player in Level.Players)
                LstPlayers.Items.Add(new PlayerListDataControl(player, Manager, LstPlayers));

            Client.Input.Level = Level;

            // Listen for later player joins.
            Client.Events.Network.Game.PlayerJoinReceived.AddHandler(
                PlayerJoined);

            // Listen for ping updates for players.
            Client.Events.Network.Game.PingUpdateReceived.AddHandler(PingUpdated);


            // Hackish way to get chats to start at the bottom.
            for (var i = 0; i < (Manager.TargetHeight*0.25f)/18; i++)
                LstChats.Items.Add(new ChatDataControl("", Manager, LstChats, this));

            Client.Events.Network.Game.ChatReceived.AddHandler(ChatReceived);

            // Level event handlers.
            Client.Events.Game.Level.BlockPlaced.AddHandler(BlockPlaced);
        }

        private void BlockPlaced(EventManager.GameEvents.LevelEvents.BlockPlacedEventArgs args)
        {
            // Directly access the tile array, as we don't want to send two BlockPlaced events, as the tile indexer will
            // automatically call the event and send a network message.
            if (args.Level != null)
                args.Level.Tiles.Tiles[args.X, args.Y, args.Z] = args.Type;
        }

        private void ChatReceived(EventManager.NetEvents.GameServerEvents.ChatEventArgs args)
        {
            AddChat(args.Message);
        }

        private void PingUpdated(EventManager.NetEvents.GameServerEvents.PingUpdateEventArgs args)
        {
            foreach (var ping in args.Pings)
            {
                var control = (PlayerListDataControl) LstPlayers.Items.FirstOrDefault(i => ((PlayerListDataControl) i).User.UUID == ping.Key);
                if (control != null) control.Ping = ping.Value;
            }
        }

        private void PlayerJoined(EventManager.NetEvents.GameServerEvents.PlayerJoinEventArgs args)
        {
            LstPlayers.Items.Add(new PlayerListDataControl(args.Player, Manager, LstPlayers));
        }

        public override void Update(GameTime gameTime)
        {
            LblStats.Text = "FPS: " + Window.FPS;

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            HandleInput(delta);

            base.Update(gameTime);
        }

        /// <summary>
        /// Handle input related to the game screen.
        /// </summary>
        private void HandleInput(float delta)
        {
            // Mouse Input.
            if (Client.Input.IsLeftDown() && Level != null)
            {
                var layer = Client.Input.IsKeyDown(Keys.LeftAlt) && SelectedBlock.Layer.HasFlag(Layer.Background)
                    ? Layer.Background
                    : SelectedBlock.Layer;
                var pos = Client.Input.GetMouseGridPositon(layer);
                if (!Window.IsMouseOverUI())
                {
                    // Place block.
                    if (Level.InBounds(pos.X, pos.Y) && Level.Tiles[pos.X, pos.Y, layer] != SelectedBlock)
                    {
                        Level.Tiles[pos.X, pos.Y, layer] = SelectedBlock;
                    }
                }
            }

            // Key Input.
            if (Client.Input.IsKeyPressed(Keys.T) && !TxtChat.Visible) // Open chat.
            {
                TxtChat.Visible = true;
                TxtChat.Passive = false;
                TxtChat.Focused = true;
                LstChats.Passive = false;
                LstChats.Items.ForEach(x => ((ChatDataControl) x).Show());
            }
            // Close or send chat.
            else if ((Client.Input.IsKeyPressed(Keys.Enter) && TxtChat.Visible) || Client.Input.IsKeyPressed(Keys.Escape))
            {
                // If there's characters in chatbox, send chat.
                // Cancel out of chat if player clicks escape.
                if (!string.IsNullOrWhiteSpace(TxtChat.Text) && !Client.Input.IsKeyPressed(Keys.Escape))
                {
                    Client.Network.Send(new ChatMessage(TxtChat.Text.Trim()));
                    TxtChat.Text = string.Empty;
                }
                // If nothing is typed and player clicked enter, close out of chat.
                TxtChat.Visible = false;
                TxtChat.Passive = true;
                TxtChat.Focused = false;
                LstChats.Passive = true;
                LstChats.Items.ForEach(x => ((ChatDataControl) x).Hide());
            }
            // Open or close inventory.
            else if (Client.Input.IsKeyPressed(Keys.E) && !IsChatOpen())
            {
                if (!Inventory.SizeChanging)
                {
                    Inventory.IsOpen = !Inventory.IsOpen;
                    Inventory.SizeChanging = true;
                }
            }
            // Move camera
            var direction = Client.Input.GetDirection();
            var speed = 750f * delta;
            Level?.Camera.Move(direction * speed);
            LstPlayers.Visible = Client.Input.IsKeyDown(Keys.Tab);
        }

        /// <summary>
        /// Adds a chat message to the list of chats, wrapping the text if too long.
        /// </summary>
        /// <remarks>
        /// The latest message will be scrolled to.
        /// </remarks>
        public void AddChat(string text)
        {
            var lines = WrapText(text, LstChats.Width - 8, FontSize.Default8).Split('\n');
            foreach (var line in lines)
                LstChats.Items.Add(new ChatDataControl(line, Manager, LstChats, this));
            LstChats.ScrollTo(LstChats.Items.Count);
        }

        private string WrapText(string text, float maxLineWidth, FontSize fontsize)
        {
            var spriteFont = Manager.Skin.Fonts[fontsize.ToString()].Resource;
            var words = text.Split(' ');
            var sb = new StringBuilder();
            var lineWidth = 0f;
            var spaceWidth = spriteFont.MeasureString(" ").X;
            foreach (var word in words)
            {
                var size = spriteFont.MeasureRichString(word, Manager);

                if (lineWidth + size.X < maxLineWidth)
                {
                    sb.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    sb.Append("\n" + word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// If chat is open.
        /// </summary>
        public bool IsChatOpen() => TxtChat.Visible;

        /// <summary>
        /// Removes the game UI and handlers.
        /// </summary>
        public override void Clear()
        {
            Client.Events.Network.Game.PlayerJoinReceived.RemoveHandler(PlayerJoined);
            Client.Events.Network.Game.PingUpdateReceived.RemoveHandler(PingUpdated);
            Client.Events.Network.Game.ChatReceived.RemoveHandler(ChatReceived);
            Client.Events.Game.Level.BlockPlaced.RemoveHandler(BlockPlaced);
            base.Clear();
        }
    }
}