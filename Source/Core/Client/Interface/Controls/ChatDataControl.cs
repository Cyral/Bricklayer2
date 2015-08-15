using System;
using Bricklayer.Core.Client.Interface.Screens;
using Microsoft.Xna.Framework;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Controls
{
    public sealed class ChatDataControl : Control
    {
        private readonly Label lblMsg;
        private bool allowTrans = true;
        private bool done;
        private bool startTrans;
        private double timePassed;

        public ChatDataControl(string text, Manager manager, Control parent, GameScreen screen)
            : base(manager)
        {
            Width = parent.ClientWidth;
            Height = 16;
            Parent = parent;

            lblMsg = new Label(Manager)
            {
                Width = parent.Width - 8
            };
            lblMsg.Init();
            lblMsg.DrawFormattedText = true;
            lblMsg.Ellipsis = false;
            lblMsg.Text = text;
            lblMsg.Shadow = true;
            Add(lblMsg);

            if (screen.IsChatOpen())
            {
                allowTrans = false;
                done = true;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!allowTrans) return;
            timePassed += gameTime.ElapsedGameTime.TotalSeconds;

            // If time has passed, start fading out
            if (timePassed > 8 && !startTrans)
            {
                timePassed = 0;
                startTrans = true;
            }
            else if (startTrans) // If transition is started
            {
                if (lblMsg.Alpha > 0) // If not already faded away
                    lblMsg.Alpha -= (float) timePassed*255f;
                else
                    done = true;
            }
        }

        public override void Show()
        {
            lblMsg.Alpha = 255;
            allowTrans = false;
        }

        public override void Hide()
        {
            if (!done)
                allowTrans = true;
            else
                lblMsg.Alpha = 0;
        }

        public override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            // Don't draw anything
            // base.DrawControl(renderer,rect,gameTime);
        }
    }
}
