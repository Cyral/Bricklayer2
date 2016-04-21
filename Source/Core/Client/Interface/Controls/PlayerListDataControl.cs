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
        public Player User { get; private set; }
        private readonly Label lblName;
        private readonly Label lblPing;

        public PlayerListDataControl(Player user, Manager manager, Control parent) : base(manager)
        {
            User = user;
            Height = 16;
            Width = parent.Width - 24; // -24 due to scrollbar, TODO: remove scrollbar completely

            lblName = new Label(manager)
            {
                Text = user.Username,
                Left = 2
            };
            lblName.Init();
            Add(lblName);

            lblPing = new Label(manager);
            lblPing.Init();
            lblPing.Width = 100;
            ChangePing(0);
            Add(lblPing);
        }

        public void ChangePing(int ping)
        {
            lblPing.TextColor = GetTextColor(ping);
            lblPing.Text = $"{ping} ms";
            lblPing.Left = Width - (int)Manager.Skin.Fonts[lblPing.Font.ToString()].Resource.MeasureString(lblPing.Text).X - 2;
        }

        private Color GetTextColor(int ping)
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