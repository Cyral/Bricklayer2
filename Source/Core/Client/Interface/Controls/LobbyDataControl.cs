using System;
using System.Threading;
using Microsoft.Xna.Framework;
using MonoForce.Controls;
using Bricklayer.Core.Server.Data;
using MonoForce.Controls;

namespace Bricklayer.Core.Client.Interface.Controls
{
    /// <summary>
    /// A control for displaying a room's name, rating, online, etc, in the lobby 
    /// </summary>
    public class LobbyDataControl : Control
    {
        public LobbySaveData Data;

        private Label lblName, lblDescription, lblStats;
        private ImageBox[] imgRating = new ImageBox[5];
        private StatusBar Gradient;

        public LobbyDataControl(Manager manager, LobbySaveData data)
            : base(manager)
        {
            //Setup
            Passive = false;
            Height = 60;
            ClientWidth = (700 / 2) - 16;
            Data = data;

            //Background "gradient" image
            //TODO: Make an actual control. not a statusbar
            Gradient = new StatusBar(manager);
            Gradient.Init();
            Gradient.Alpha = .8f;
            Add(Gradient);

            for (int i = 0; i < 5; i++)
            {
                imgRating[i] = new ImageBox(Manager) { Top = 4, Width = 16, Height = 16, Left = ClientWidth - (((4 - i) * 18) + 48) };
                imgRating[i].Init();
                bool half = data.Rating > i + .25 && data.Rating < i + .75;
                bool whole = data.Rating >= i + .75;
                //imgRating[i].Image = whole ? ContentPack.Textures["gui\\icons\\full_star"] : half ? ContentPack.Textures["gui\\icons\\half_star"] : ContentPack.Textures["gui\\icons\\empty_star"];
                Add(imgRating[i]);
            }

            lblStats = new Label(Manager) { Width = 100, Left = ClientWidth - 100 - 16, Top = imgRating[0].Bottom + 4, Alignment = Alignment.TopLeft, TextColor = new Color(160, 160, 160) };
            lblStats.Init();
            lblStats.Text = string.Format("Online: {0}\nPlays: {1}", data.Online, "N/A" /*data.Plays*/);
            Add(lblStats);

            //Add controls
            lblName = new Label(Manager) { Width = 100, Text = data.Name, Left = 4, Top = 4, Font = FontSize.Default14, Alignment = Alignment.TopLeft };
            lblName.Init();
            Add(lblName);
            lblName.Text = data.Name;

            lblDescription = new Label(Manager) { Width = 200, Text = data.Name, Left = 4, Top = lblName.Bottom + 4, Alignment = Alignment.TopLeft };
            lblDescription.Init();
            Add(lblDescription);
            lblDescription.Text = data.Description;
        }
        public override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            //Don't draw anything
            //base.DrawControl(renderer,rect,gameTime);
        }
    }
}
