using System;
using System.Diagnostics;
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
        private StatusBar sbStats;
        private Label lblStats;
        private ControlList<ChatDataControl> lstChats;
        private ControlList<PlayerListDataControl> lstPlayers;
        private TextBox txtChat;

        private Panel pnInventory;
        private Button[] btnInventory;
        private bool OpenInventory = false;
        private int NormWidth;
        private int NormHeight;
        private int WidthChange;
        private bool Changing = false;

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

            lblStats = new Label(Manager) { Top = 4, Left = 8, Width = Manager.ScreenWidth - 16 };
            lblStats.Init();

            sbStats.Add(lblStats);
            Window.Add(sbStats);

            // Block buttons
            btnInventory = new Button[11];

            // Inventory
            pnInventory = new Panel(Manager)
            {
                // (48*9) + (10*10)
                Width = (48*btnInventory.Length)+((btnInventory.Length+1)* 10), // Make room for 11 slots and 10 space inbetween
                Height = 50
            };
            pnInventory.Left = Manager.TargetWidth/2 - (pnInventory.Width/2);
            pnInventory.Init();
            Window.Add(pnInventory);
            NormHeight = 50;
            NormWidth = (48 * btnInventory.Length) + ((btnInventory.Length + 1) * 10);

            for (int i = 0; i < btnInventory.Count(); i++)
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
            txtChat.Width = (int)(Manager.TargetWidth * .4f) - 16; // Remove 16 to align due to invisible scrollbar
            txtChat.Visible = false;
            txtChat.Passive = true;
            Window.Add(txtChat);

            lstChats = new ControlList<ChatDataControl>(Manager)
            {
                Left = txtChat.Left,
                Width = txtChat.Width + 16,
                Height = (int)(Manager.TargetHeight * .25f),
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
                Top = 256,
            };
            lstPlayers.Init();
            lstPlayers.HideSelection = true;
            lstPlayers.Left = Manager.TargetWidth/2 - (lstPlayers.Width / 2);
            lstPlayers.Passive = true;
            lstPlayers.HideScrollbars = true;
            lstPlayers.Visible = false;
            Window.Add(lstPlayers);

            foreach (var player in Level.Players)
                lstPlayers.Items.Add(new PlayerListDataControl(player, Manager, lstPlayers));


            // Listen for later player joins
            Client.Events.Network.Game.PlayerJoinReceived.AddHandler(args =>
            {
                lstPlayers.Items.Add(new PlayerListDataControl(args.Player, Manager, lstPlayers));
            });

            // Listen for ping updates for players
            Client.Events.Network.Game.PingUpdateReceived.AddHandler(args =>
            {
                foreach (var ping in args.Pings)
                {
                    var control = (PlayerListDataControl)lstPlayers.Items.FirstOrDefault(i => ((PlayerListDataControl)i).User.UUID == ping.Key);
                    control?.ChangePing(ping.Value);
                }
            });


            // Hackish way to get chats to start at the bottom
            for (var i = 0; i < (Manager.TargetHeight * 0.25f) / 18; i++)
            {
                lstChats.Items.Add(new ChatDataControl("", Manager, lstChats, this));
            }

            Client.Events.Network.Game.ChatReceived.AddHandler(args =>
            {
                AddChat(args.Message, Manager, lstChats);
            });
        }


        public override void Update(GameTime gameTime)
        {
            lblStats.Text = "FPS: " + Window.FPS;

            HandleInput();
            UpdateInventory();


            base.Update(gameTime);
        }

        private double effect = 9; // Current movement amount for transition
        private void UpdateInventory()
        {
            if (OpenInventory) // If inventory is opening or is open
            {
                if (pnInventory.Width < WidthChange) // If the open process is still going
                {
                    if (effect > 1) 
                        effect -= 0.25;

                    pnInventory.Width += (int)effect;
                    pnInventory.Height += (int)effect;
                    pnInventory.Left = Manager.TargetWidth / 2 - (pnInventory.Width / 2); // Center the inventory box

                    // Keep buttons in the center
                    for (var i = 0; i < btnInventory.Count(); i++)
                        btnInventory[i].Left = ((pnInventory.Width / 2) + 24 * btnInventory.Count() + 2) - (58 * i);
                }
                else
                {
                    Changing = false;
                    effect = 9;
                }
            }
            else // If inventory is closing or is closed
            {
                if (pnInventory.Width > NormWidth) // If the close process is still going
                {
                    if (effect > 1)
                        effect -= 0.25;

                    pnInventory.Width -= (int)effect;
                    pnInventory.Height -= (int)effect;
                    pnInventory.Left = Manager.TargetWidth / 2 - (pnInventory.Width / 2); // Center the inventory box

                    // Keep buttons in the center
                    for (var i = 0; i < btnInventory.Count(); i++)
                        btnInventory[i].Left = ((pnInventory.Width / 2) + 24 * btnInventory.Count() + 2) - (58 * i);
                }
                else
                {
                    Changing = false;
                    effect = 9;
                }

            }
        }
        private void HandleInput()
        {
            if (Client.Input.IsKeyPressed(Keys.T) && !txtChat.Visible) // Open chat
            {
                txtChat.Visible = true;
                txtChat.Passive = false;
                txtChat.Focused = true;
                lstChats.Items.ForEach(x => ((ChatDataControl)x).Show());
            }
            else if ((Client.Input.IsKeyPressed(Keys.Enter) && txtChat.Visible) || Client.Input.IsKeyPressed(Keys.Escape))
            {
                // If there's characters in chatbox, send chat
                // Cancel out of chat if player clicks escape
                if (!string.IsNullOrWhiteSpace(txtChat.Text) && !Client.Input.IsKeyPressed(Keys.Escape))
                {
                    Client.Network.Send(new ChatMessage(txtChat.Text));
                    txtChat.Text = string.Empty;
                }
                // If nothing is typed and player clicked enter, close out of chat
                txtChat.Visible = false;
                txtChat.Passive = true;
                txtChat.Focused = false;
                lstChats.Items.ForEach(x => ((ChatDataControl)x).Hide());
            }
            else if (Client.Input.IsKeyPressed(Keys.I))
            {
                if (!Changing)
                {
                    if (!OpenInventory)
                    {
                        OpenInventory = true;
                        WidthChange = pnInventory.Width + 150;
                    }
                    else if (OpenInventory)
                    {
                        OpenInventory = false;
                    }
                    Changing = true;
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
        /// If chat is open
        /// </summary>
        /// <returns></returns>
        public bool ChatOpen()
        {
            return txtChat.Visible;
        }

        /// <summary>
        /// Remove the game UI.
        /// </summary>
        public override void Remove()
        {
            Manager.Remove(sbStats);
        }
    }
}
