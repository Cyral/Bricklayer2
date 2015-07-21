using Bricklayer.Core.Client.Interface.Controls;
using Bricklayer.Core.Client.World;
using Bricklayer.Core.Common.Net.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Screens
{
    internal class GameScreen : Screen
    {
        protected internal override GameState State => GameState.Game;
        internal Level Level => Client.Level;
        private StatusBar sbStats;
        private Label lblStats;
        private ControlList<ChatDataControl> lstChats;
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

            lblStats = new Label(Manager) { Top = 4, Left = 8, Width = Manager.ScreenWidth - 16 };
            lblStats.Init();

            sbStats.Add(lblStats);
            Window.Add(sbStats);

            //Chat input box
            txtChat = new TextBox(Manager);
            txtChat.Init();
            txtChat.Left = 30;
            txtChat.Top = sbStats.Top - 50;
            txtChat.Width = (int)(Manager.TargetWidth * 0.4f);
            txtChat.Visible = false;
            txtChat.Passive = true;
            Window.Add(txtChat);

            lstChats = new ControlList<ChatDataControl>(Manager)
            {
                Left = txtChat.Left,
                Width = txtChat.Width,
                Height = (int)(Manager.TargetHeight * 0.25f),
            };
            lstChats.Init();
            lstChats.Color = Color.Transparent;
            lstChats.HideSelection = true;
            lstChats.Passive = true;
            lstChats.Top = txtChat.Top - lstChats.Height;
            Window.Add(lstChats);
            // Hackish way to get chats to start at the bottom
            for (var i = 0; i < (Manager.TargetHeight * 0.25f) / 18; i++)
            {
                lstChats.Items.Add(new ChatDataControl("", Manager, lstChats));
            }

            Client.Events.Network.Game.ChatReceived.AddHandler(args =>
            {
                lstChats.Items.Add(new ChatDataControl(args.Message, Manager, lstChats));
                lstChats.ScrollTo(lstChats.Items.Count);
            });
        }

        public override void Update(GameTime gameTime)
        {
            lblStats.Text = "FPS: " + Window.FPS;

            if (Client.Input.WasKeyPressed(Keys.T) && !txtChat.Visible) // Open chat
            {
                txtChat.Visible = true;
                txtChat.Passive = false;
                txtChat.Focused = true;

                lstChats.Items.ForEach(x => ((ChatDataControl)x).Show());
            }
            else if (Client.Input.WasKeyPressed(Keys.Enter) && txtChat.Visible)
            {
                if (!string.IsNullOrWhiteSpace(txtChat.Text)) // If there's characters in chatbox, send chat
                {
                    Client.Network.Send(new ChatMessage(txtChat.Text));
                    //lstChats.Items.Add(new ChatDataControl(txtChat.Text, Manager, lstChats));
                    txtChat.Text = "";
                    txtChat.Visible = false;
                    txtChat.Passive = true;
                    txtChat.Focused = false;
                    lstChats.Items.ForEach(x => ((ChatDataControl)x).UnShow());
                }
                else // If nothing is typed and player clicked enter, close out of chat
                {
                    txtChat.Visible = false;
                    txtChat.Passive = true;
                    txtChat.Focused = false;
                    lstChats.Items.ForEach(x => ((ChatDataControl)x).UnShow());
                }
            }
            // Cancel out of chat if player clicks escape
            else if (Client.Input.WasKeyPressed(Keys.Escape) && txtChat.Visible)
            {
                txtChat.Visible = false;
                txtChat.Passive = true;
                txtChat.Focused = false;
                lstChats.Items.ForEach(x => ((ChatDataControl)x).UnShow());
            }
            base.Update(gameTime);
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