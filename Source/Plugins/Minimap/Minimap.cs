using System;
using System.Threading;
using Bricklayer.Core.Client;
using Bricklayer.Core.Client.Interface.Screens;
using Bricklayer.Core.Common.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Bricklayer.Plugins.Minimap
{
    /// <summary>
    /// Level minimap plugin.
    /// </summary>
    public class Minimap : ClientPlugin
    {
        private static readonly string configFile = "config.json";
        private MinimapConfig config;
        private MinimapControl minimap;

        public Minimap(Client host) : base(host)
        {
        }

        public override async void Load()
        {
            // Load or create config file.
            config = await Client.IO.LoadConfig<MinimapConfig>(this, configFile);

            // Create the control.
            Client.Events.Game.ScreenChanged.AddHandler(args =>
            {
                var screen = args.NewScreen as GameScreen;
                if (screen != null)
                {
                    minimap = new MinimapControl(Client.UI, screen.Level, config.RegionAlpha);
                    minimap.Init();
                    minimap.Alpha = config.Alpha * 255;
                    minimap.Left = config.X;
                    minimap.Top = config.Y;
                    minimap.Visible = false;
                    minimap.Width = Math.Min(config.Width, screen.Level.Width);
                    minimap.Height = Math.Min(config.Height, screen.Level.Height);
                    screen.AddControl(minimap);

                    // Use thread as the initial color data can take a while to fill for large maps.
                    new Thread(() =>
                    {
                        // Set the initial color data.
                        var colors = new Color[screen.Level.Height * screen.Level.Width];
                        for (var y = 0; y < screen.Level.Height; y++)
                        {
                            for (var x = 0; x < screen.Level.Width; x++)
                            {
                                var foreground = screen.Level.Tiles[x, y, 1];
                                var background = screen.Level.Tiles[x, y, 0];

                                colors[x + y * screen.Level.Width] = minimap.GetColor(foreground, background);
                            }
                        }
                        minimap?.SetData(colors);
                        minimap.Visible = true;
                    }).Start();
                }
            });

            // Set each pixel when a block is placed.
            Client.Events.Game.Level.BlockPlaced.AddHandler(
                args =>
                {
                    minimap?.ChangePixel(args.X, args.Y,
                        minimap.GetColor(args.Level.Tiles[args.X, args.Y],
                            args.Level.Tiles[args.X, args.Y, Layer.Background]));
                });
        }

        public override void Update(GameTime delta)
        {
            if (Client.State == GameState.Game && Client.Input.IsKeyPressed(Keys.M) && minimap != null)
            {
                minimap.Visible = !minimap.Visible;
            }
            base.Update(delta);
        }

        public override async void Unload()
        {
            // Save minimap size, position, etc.
            if (minimap != null)
            {
                config.X = minimap.Left;
                config.Y = minimap.Top;
                config.Width = minimap.Width;
                config.Height = minimap.Height;
                await Client.IO.SaveConfig(config, this, configFile);
            }
        }
    }
}