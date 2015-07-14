using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bricklayer.Core.Common.Data;
using Bricklayer.Core.Common.World;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace Bricklayer.Core.Common.Entity
{
    /// <summary>
    /// A player entity.
    /// </summary>
    public class Player : PlayerData
    {
        //Contants
        public static readonly int Width = 16, Height = 16;

        //Entity States
        /// <summary>
        /// The current internal state (What the game sees, without interpolation)
        /// </summary>
        public EntityState SimulationState { get; protected set; }

        /// <summary>
        /// Current display state (What the player sees)
        /// </summary>
        public EntityState DisplayState { get; protected set; }

        /// <summary>
        /// The last internal state. (What was seen the last frame)
        /// </summary>
        public EntityState PreviousState { get; protected set; }

        /// <summary>
        /// The direction the player is facing.
        /// </summary>
        public FacingDirection Direction { get; protected set; }

        //Physic States
        public bool IsJumping { get; protected set; }
        public bool IsOnGround { get; protected set; }
        public bool WasJumping { get; protected set; }
        public float JumpTime { get; protected set; }
        public float IdleTime { get; protected set; }
        public bool IsIdle => IdleTime > 0;
        public bool VirtualJump { get; protected set; }

        /// <summary>
        /// The currently occupied map the player is in
        /// </summary>
        public virtual Level Level { get; protected set; }

        /// <summary>
        /// The color the body should be tinted.
        /// </summary>
        public virtual Color Tint { get; protected set; }

        /// <summary>
        /// The rectangular collision bounds of the player.
        /// </summary>
        public virtual Rectangle Bounds => new Rectangle((int)SimulationState.Position.X, (int)SimulationState.Position.Y, Width, Height);

        /// <summary>
        /// The position on the grid a player is occupying (based on DisplayState)
        /// </summary>
        public virtual Point GridPosition => new Point((int)Math.Round(DisplayState.Position.X / Tile.Width), (int)Math.Round(DisplayState.Position.Y / Tile.Height));

        /// <summary>
        /// The user's underlying network connection.
        /// </summary>
        public NetConnection Connection { get; private set; }

        /// <summary>
        /// Creates a new player.
        /// </summary>
        public Player(NetConnection connection, Level level, Vector2 position, string name, Guid uuid, bool isGuest) : base(name, uuid, isGuest)
        {
            Connection = connection;
            Level = level;
            //Smiley = SmileyType.Default;
            //Mode = PlayerMode.Normal;
            Tint = Color.White;

            SimulationState = new EntityState();
            DisplayState = new EntityState();
            PreviousState = new EntityState();

            SimulationState.Position = PreviousState.Position = DisplayState.Position = position;
        }
    }
}
