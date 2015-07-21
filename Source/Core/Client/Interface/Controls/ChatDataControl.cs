using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Bricklayer.Client.Interface;
using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Client.Interface.Windows;
using Bricklayer.Core.Common.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Controls
{
    public sealed class ChatDataControl : Control
    {
        private readonly Label lblMsg;
        private double timePassed;
        private bool startTrans;
        private bool allowTrans = true;
        private bool done;

        public ChatDataControl(string text, Manager manager, Control parent)
            : base(manager)
        {
            Width = parent.ClientWidth;
            Height = 16;
            this.parent = parent;
            lblMsg = new Label(Manager)
            {
                Width = parent.Width -8,
            };
            lblMsg.Init();
            lblMsg.Ellipsis = false;
            lblMsg.Text = text;
            Add(lblMsg);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (allowTrans)
            {
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
                        lblMsg.Alpha -= (float)timePassed * 255f;
                    else
                        done = true;
                }
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
            {
                allowTrans = true;
            }
            else
                lblMsg.Alpha = 0;
        }


        public override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            //Don't draw anything
            //base.DrawControl(renderer,rect,gameTime);
        }
    }
}
