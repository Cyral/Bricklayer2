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
        private Label lblMsg;
        private double timePassed = 0;
        private bool startTrans = false;
        private bool allowTrans = true;
        private bool done = false;

        public ChatDataControl(string text, Manager manager, Control parent)
            : base(manager)
        {
            Width = parent.ClientWidth;
            Height = 16;
            this.parent = parent;
            lblMsg = new Label(Manager)
            {
                Width = parent.Width,
            };
            lblMsg.Init();
            lblMsg.Ellipsis = false;
            lblMsg.Text = WrapText(text, lblMsg.Width);
            Add(lblMsg);
        }

        /// <summary>hj
        /// Wraps a string around the width of an area (Ex: The chat box)
        /// </summary>
        public string WrapText(string text, float maxLineWidth)
        {
            SpriteFont spriteFont = Manager.Skin.Fonts[lblMsg.Font.ToString()].Resource;
            string[] words = text.Split(' ');
            StringBuilder sb = new StringBuilder();
            float lineWidth = 0f;
            float spaceWidth = spriteFont.MeasureString(" ").X;
            int lines = 1;
            foreach (string word in words)
            {
                Vector2 size = spriteFont.MeasureRichString(word, Manager);

                if (lineWidth + size.X < maxLineWidth)
                {
                    sb.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    sb.Append("\n" + word + " ");
                    lines++;
                    lineWidth = size.X + spaceWidth;
                }
            }
            lblMsg.Height = Height = lines*spriteFont.LineSpacing;
            return sb.ToString();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (allowTrans)
            {
                timePassed += gameTime.ElapsedGameTime.TotalSeconds;

                // If (somewhat) 5 seconds have passed, start fade transition
                if (timePassed > 3 && !startTrans)
                {
                    timePassed = 0;
                    startTrans = true;
                }
                else if (startTrans) // If transition is started
                {
                    if (lblMsg.Alpha > 0) // If not already faded away
                    {
                        lblMsg.Alpha -= (float) timePassed*255f;
                    }
                    else
                    {
                        done = true;
                    }
                }
                parent.Invalidate();
                lblMsg.Invalidate();
            }

        }

        public void Show()
        {
            lblMsg.Alpha = 255;
            allowTrans = false;
        }

        public void UnShow()
        {
            if (!done)
            {
                allowTrans = true;
                startTrans = false;
                timePassed = 0;
            }
            else
            {
                lblMsg.Alpha = 0;
            }
        }


        public override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            //Don't draw anything
            //base.DrawControl(renderer,rect,gameTime);
        }

    }
}
