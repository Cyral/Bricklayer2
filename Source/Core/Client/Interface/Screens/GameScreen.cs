using System.Linq;
using System.Text;
using Bricklayer.Core.Client.Interface.Controls;
using Bricklayer.Core.Client.World;
using Bricklayer.Core.Common.Net.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Screens
{
    public class GameScreen : Screen
    {
        protected internal override GameState State => GameState.Game;
        internal Level Level => Client.Level;
        private Button[] btnInventory;
        private bool inventorySizeChanging;
        private int inventoryWidthChange;
        private bool isInventoryOpen;
        private Label lblStats;
        private ControlList<ChatDataControl> lstChats;
        private ControlList<PlayerListDataControl> lstPlayers;
        private int normalInventoryHeight = 64, normalInventoryWidth;
        private Panel pnInventory;
        private StatusBar sbStats;
        private TextBox txtChat;

        /// <summary>
        /// Setup the game UI.
        /// </summary>
        public override void Add(ScreenManager screenManager)
        {
            base.Add(screenManager);

            sbStats = new StatusBar(Manager);
            sbStats.Init();
            sbStats.Bottom = Manager.ScreenHeight;
            sbStats.Left = 0;
            sbStats.Width = Manager.ScreenWidth;

            lblStats = new Label(Manager) {Top = 4, Left = 8, Width = Manager.ScreenWidth - 16};
            lblStats.Init();

            sbStats.Add(lblStats);
            Window.Add(sbStats);

            // Block buttons
            btnInventory = new Button[11];

            // Inventory
            normalInventoryWidth = (48*btnInventory.Length) + ((btnInventory.Length + 1)*10);
            pnInventory = new Panel(Manager)
            {
                // (48*9) + (10*10)
                // Make room for 11 slots and 10 space in between
                Width = normalInventoryWidth,
                Height = normalInventoryHeight
            };
            pnInventory.Left = Manager.TargetWidth/2 - (pnInventory.Width/2);
            pnInventory.Init();

            Window.Add(pnInventory);
            normalInventoryHeight = 50;
            normalInventoryWidth = (48*btnInventory.Length) + ((btnInventory.Length + 1)*10);

            for (var i = 0; i < btnInventory.Count(); i++)
            {
                btnInventory[i] = new Button(Manager)
                {
                    Left = 10 + (58*i),
                    Width = 48,
                    Height = 48,
                    Text = ""
                };
                btnInventory[i].Init();
                pnInventory.Add(btnInventory[i]);
            }


            //Chat input box
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


            // Listen for later player joins
            Client.Events.Network.Game.PlayerJoinReceived.AddHandler(
                args => { lstPlayers.Items.Add(new PlayerListDataControl(args.Player, Manager, lstPlayers)); });

            // Listen for ping updates for players
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


            // Hackish way to get chats to start at the bottom
            for (var i = 0; i < (Manager.TargetHeight*0.25f)/18; i++)
            {
                lstChats.Items.Add(new ChatDataControl("", Manager, lstChats, this));
            }

            Client.Events.Network.Game.ChatReceived.AddHandler(args => { AddChat(args.Message, Manager, lstChats); });
        }

        public override void Update(GameTime gameTime)
        {
            lblStats.Text = "FPS: " + Window.FPS;

            HandleInput();
            UpdateInventory(gameTime);


            base.Update(gameTime);
        }

        private void UpdateInventory(GameTime gameTime)
        {
            if (inventorySizeChanging)
            {
                var delta = (float)gameTime.ElapsedGameTime.TotalSeconds * 20;
                if (isInventoryOpen) // Make inventory bigger as it opens.
                {
                    // 25 is added/subtracted to make the lerp faster, as it will become very slow at the end.
                    pnInventory.Width =
                        (int) MathHelper.Lerp(pnInventory.Width, (normalInventoryWidth*1.5f) + 25, delta);
                    pnInventory.Height =
                        (int) MathHelper.Lerp(pnInventory.Height, (normalInventoryHeight*2) + 25, delta);
                    if (pnInventory.Width >= normalInventoryWidth*1.5)
                        inventorySizeChanging = false;
                }
                else
                {
                    pnInventory.Width =
                        (int)MathHelper.Lerp(pnInventory.Width, normalInventoryWidth - 25, delta);
                    pnInventory.Height =
                        (int)MathHelper.Lerp(pnInventory.Height, normalInventoryHeight, delta);
                    if (pnInventory.Width <= normalInventoryWidth)
                    {
                        inventorySizeChanging = false;
                        pnInventory.Width = normalInventoryWidth;
                        pnInventory.Height = normalInventoryHeight;
                    }
                }

                // Keep inventory box and controls in the center.
                pnInventory.Left = Manager.TargetWidth/2 - (pnInventory.Width/2);
                for (var i = 0; i < btnInventory.Length; i++)
                    btnInventory[i].Left = ((pnInventory.Width/2) + 24*btnInventory.Length + 2) - (58*i);
            }
        }

        private void HandleInput()
        {
            if (Client.Input.IsKeyPressed(Keys.T) && !txtChat.Visible) // Open chat.
            {
                txtChat.Visible = true;
                txtChat.Passive = false;
                txtChat.Focused = true;
                lstChats.Passive = false;
                lstChats.Items.ForEach(x => ((ChatDataControl) x).Show());
            }
            else if ((Client.Input.IsKeyPressed(Keys.Enter) && txtChat.Visible) || Client.Input.IsKeyPressed(Keys.Escape)) // Close or send chat.
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
            else if (Client.Input.IsKeyPressed(Keys.E)) // Open or close inventory.
            {
                if (!inventorySizeChanging)
                {
                    isInventoryOpen = !isInventoryOpen;
                    inventorySizeChanging = true;
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
        }
    }
}