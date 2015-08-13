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

        protected internal override GameState State => GameState.Game;
        internal Level Level => Client.Level;
        private Label lblStats;
        private ControlList<ChatDataControl> lstChats;
        private ControlList<PlayerListDataControl> lstPlayers;
        private InventoryControl pnlInventory;
        private StatusBar sbStats;
        private BlockType selectedBlock;
        private TextBox txtChat;

        /// <summary>
        /// Setup the game UI.
        /// </summary>
        public override void Add(ScreenManager screenManager)
        {
            base.Add(screenManager);

            // Status bar.
            sbStats = new StatusBar(Manager);
            sbStats.Init();
            sbStats.Bottom = Manager.ScreenHeight;
            sbStats.Left = 0;
            sbStats.Width = Manager.ScreenWidth;

            lblStats = new Label(Manager) {Top = 4, Left = 8, Width = Manager.ScreenWidth - 16};
            lblStats.Init();

            sbStats.Add(lblStats);
            Window.Add(sbStats);

            // Inventory.
            pnlInventory = new InventoryControl(this, Manager);
            pnlInventory.Left = Manager.TargetWidth/2 - (pnlInventory.Width/2);
            pnlInventory.Init();
            Window.Add(pnlInventory);

            // Chat.
            txtChat = new TextBox(Manager);
            txtChat.Init();
            txtChat.Left = 8;
            txtChat.DrawFormattedText = false;
            txtChat.Bottom = sbStats.Top - 8;
            txtChat.Width = (int) (Manager.TargetWidth*.4f) - 16; // Remove 16 to align due to invisible scrollbar
            txtChat.Visible = false;
            txtChat.Passive = true;
            Window.Add(txtChat);

            lstChats = new ControlList<ChatDataControl>(Manager)
            {
                Left = txtChat.Left,
                Width = txtChat.Width + 16,
                Height = (int) (Manager.TargetHeight*.25f)
            };
            lstChats.Init();
            lstChats.Color = Color.Transparent;
            lstChats.HideSelection = true;
            lstChats.Passive = true;
            lstChats.HideScrollbars = true;
            lstChats.Top = txtChat.Top - lstChats.Height;
            Window.Add(lstChats);

            // Tablist.
            lstPlayers = new ControlList<PlayerListDataControl>(Manager)
            {
                Width = 256,
                Top = 256
            };
            lstPlayers.Init();
            lstPlayers.HideSelection = true;
            lstPlayers.Left = Manager.TargetWidth/2 - (lstPlayers.Width/2);
            lstPlayers.Passive = true;
            lstPlayers.HideScrollbars = true;
            lstPlayers.Visible = false;
            Window.Add(lstPlayers);

            foreach (var player in Level.Players)
                lstPlayers.Items.Add(new PlayerListDataControl(player, Manager, lstPlayers));


            // Listen for later player joins.
            Client.Events.Network.Game.PlayerJoinReceived.AddHandler(
                args => { lstPlayers.Items.Add(new PlayerListDataControl(args.Player, Manager, lstPlayers)); });

            // Listen for ping updates for players.
            Client.Events.Network.Game.PingUpdateReceived.AddHandler(args =>
            {
                foreach (var ping in args.Pings)
                {
                    var control =
                        (PlayerListDataControl)
                            lstPlayers.Items.FirstOrDefault(i => ((PlayerListDataControl) i).User.UUID == ping.Key);
                    control?.ChangePing(ping.Value);
                }
            });


            // Hackish way to get chats to start at the bottom.
            for (var i = 0; i < (Manager.TargetHeight*0.25f)/18; i++)
                lstChats.Items.Add(new ChatDataControl("", Manager, lstChats, this));

            Client.Events.Network.Game.ChatReceived.AddHandler(args => { AddChat(args.Message, Manager, lstChats); });

            // Level event handlers.
            Client.Events.Game.Level.BlockPlaced.AddHandler(args =>
            {
                // Directly access the tile array, as we don't want to send two BlockPlaced events, as the tile indexer will
                // automatically call the event and send a network message.
                if (args.Level != null)
                    args.Level.Tiles.Tiles[args.X, args.Y, args.Z] = args.Type;
            });
        }

        public override void Update(GameTime gameTime)
        {
            lblStats.Text = "FPS: " + Window.FPS;

            HandleInput();

            base.Update(gameTime);
        }

        private void HandleInput()
        {
            // Mouse Input.
            if (Client.Input.IsLeftDown() && Level != null)
            {
                var pos = Client.Input.MouseGridPosition;
                if (!Window.IsMouseOverUI())
                {
                    // Place block.
                    if (Level.InBounds(pos.X, pos.Y) && Level.Tiles[pos.X, pos.Y] != SelectedBlock)
                        Level.Tiles[pos.X, pos.Y] = SelectedBlock;
                }
            }

            // Key Input.
            if (Client.Input.IsKeyPressed(Keys.T) && !txtChat.Visible) // Open chat.
            {
                txtChat.Visible = true;
                txtChat.Passive = false;
                txtChat.Focused = true;
                lstChats.Passive = false;
                lstChats.Items.ForEach(x => ((ChatDataControl) x).Show());
            }
            else if ((Client.Input.IsKeyPressed(Keys.Enter) && txtChat.Visible) || Client.Input.IsKeyPressed(Keys.Escape))
                // Close or send chat.
            {
                // If there's characters in chatbox, send chat.
                // Cancel out of chat if player clicks escape.
                if (!string.IsNullOrWhiteSpace(txtChat.Text) && !Client.Input.IsKeyPressed(Keys.Escape))
                {
                    Client.Network.Send(new ChatMessage(txtChat.Text.Trim()));
                    txtChat.Text = string.Empty;
                }
                // If nothing is typed and player clicked enter, close out of chat.
                txtChat.Visible = false;
                txtChat.Passive = true;
                txtChat.Focused = false;
                lstChats.Passive = true;
                lstChats.Items.ForEach(x => ((ChatDataControl) x).Hide());
            }
            else if (Client.Input.IsKeyPressed(Keys.E) && !IsChatOpen()) // Open or close inventory.
            {
                if (!pnlInventory.SizeChanging)
                {
                    pnlInventory.IsOpen = !pnlInventory.IsOpen;
                    pnlInventory.SizeChanging = true;
                }
            }
            lstPlayers.Visible = Client.Input.IsKeyDown(Keys.Tab);
        }

        private void AddChat(string text, Manager manager, ControlList<ChatDataControl> chatlist)
        {
            var lines = WrapText(text, lstChats.Width - 8, FontSize.Default9).Split('\n');
            foreach (var line in lines)
                chatlist.Items.Add(new ChatDataControl(line, manager, chatlist, this));
            chatlist.ScrollTo(chatlist.Items.Count);
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
        public bool IsChatOpen() => txtChat.Visible;

        /// <summary>
        /// Remove the game UI.
        /// </summary>
        public override void Remove()
        {
            Manager.Remove(sbStats);
            Manager.Remove(lblStats);
            Manager.Remove(lstChats);
            Manager.Remove(lstPlayers);
            Manager.Remove(txtChat);
            Manager.Remove(pnlInventory);
        }
    }
}