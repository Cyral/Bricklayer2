using Bricklayer.Core.Common.Entity;
using Microsoft.Xna.Framework;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Controls
{
    /// <summary>
    /// Control for playerlist displaying the player's username and ping
    /// </summary>
    public class PlayerListDataControl : Control
    {
        /// <summary>
        /// The user the control is for.
        /// </summary>
        public Player User { get; private set; }

        /// <summary>
        /// The ping time, in milliseconds. Updated by the server regularly.
        /// </summary>
        public int Ping
        {
            get { return ping; }
            internal set
            {
                ping = value;
                if (LblPing != null)
                {
                    LblPing.TextColor = GetTextColor(ping);
                    LblPing.Text = $"{ping} ms";
                    LblPing.Left = Width -
                                   (int)
                                       Manager.Skin.Fonts[LblPing.Font.ToString()].Resource.MeasureString(LblPing.Text)
                                           .X - 2;
                }
            }
        }

        public Label LblName { get; }
        public Label LblPing { get; }
        private int ping;

        public PlayerListDataControl(Player user, Manager manager, Control parent) : base(manager)
        {
            User = user;
            Height = 16;
            Width = parent.Width - 24; // -24 due to scrollbar, TODO: remove scrollbar completely

            LblName = new Label(manager)
            {
                Text = user.Username,
                Left = 2
            };
            LblName.Init();
            Add(LblName);

            LblPing = new Label(manager);
            LblPing.Init();
            LblPing.Width = 100;
            Ping = 0;
            Add(LblPing);
        }

        private static Color GetTextColor(int ping)
        {
            var color = Color.Lime;
            if (ping > 500)
                color = Color.Red;
            else if (ping > 300)
                color = Color.OrangeRed;
            else if (ping > 140)
                color = Color.Yellow;
            else if (ping > 90)
                color = Color.GreenYellow;
            return color;
        }
    }
}