using System;

namespace Bricklayer.Core.Common.Data
{
    /// <summary>
    /// Basic data for a level.
    /// </summary>
    /// <remarks>
    /// This is the data stored in the database and displayed in the lobby window.
    /// </remarks>
    public class LevelData
    {
        /// <summary>
        /// The name of this level.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The description of this level.
        /// </summary>
        public string Description { get; protected set; }

        /// <summary>
        /// The number of players currently online this level.
        /// </summary>
        public virtual int Online { get; protected set; }
        
        /// <summary>
        /// The rating, from 0 to 5, of this level.
        /// </summary>
        public double Rating { get; protected set; }

        /// <summary>
        /// The number of times this level has been played. (Non unique)
        /// </summary>
        public int Plays { get; protected set; }

        /// <summary>
        /// The unique ID of this level.
        /// </summary>
        public Guid UUID { get; protected set; }

        public LevelData(string name, string uuid, string description, int players, int plays, double rating)
        {
            Name = name;
            UUID = Guid.Parse(uuid);
            Description = description;
            Online = players;
            Rating = rating;
            Plays = plays;
        }
    }
}
