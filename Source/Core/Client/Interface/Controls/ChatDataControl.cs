using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private int time;

        public ChatDataControl(string text, Manager manager)
            : base(manager)
        {
            Passive = false;
            Height = 200;
            Width = 200;
            time = 0;

            lblMsg = new Label(Manager)
            {
                Width = 100,
                Text = text,
                Left = 10,
                Top = 10,
                Alignment = Alignment.TopLeft,
            };
            lblMsg.Init();
            lblMsg.Alpha = 5000;
            Add(lblMsg);
        }

        /// <summary>hj
        /// Wraps a string around the width of an area (Ex: The chat box)
        /// </summary>
        public string WrapText(SpriteFont spriteFont, string text, float maxLineWidth)
        {
            string[] words = text.Split(' ');
            StringBuilder sb = new StringBuilder();
            float lineWidth = 0f;
            float spaceWidth = spriteFont.MeasureString(" ").X;

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
                    sb.Append(MonoForce.Controls.Manager.StringNewline + word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }

            return sb.ToString();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            //Don't draw anything
            //base.DrawControl(renderer,rect,gameTime);
        }
    }
}
