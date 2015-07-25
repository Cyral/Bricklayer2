using Microsoft.Xna.Framework;
using MonoForce.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Client.Interface.Controls
{
    /// <summary>
    /// Control for playerlist displaying the player's username and ping
    /// </summary>
    public class PlayerListDataControl : Control
    {
        private Label lblName;
        private Label lblPing;
         
        public PlayerListDataControl(string user, Manager manager, Control parent) : base(manager)
        {
            Height = 16;
            
            lblName = new Label(manager)
            {
                Text = user,
                Left = 15
            };
            lblName.Init();
            Add(lblName);

            lblPing = new Label(manager)
            {
                Text = "0 ms",
                Left = parent.Width - 50
            };
            lblPing.Init();
            Add(lblPing);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
