using System;

namespace Bricklayer.Core.Common.Data
{
    /// <summary>
    /// Data for ratings of level.
    /// </summary>
    public class RatingsData
    {
        /// <summary>
        /// UUID of rater.
        /// </summary>
        public Guid Player { get; private set; }

        /// <summary>
        /// GUID of level rated.
        /// </summary>
        public Guid Level { get; private set; }

        /// <summary>
        /// Rating of level,
        /// </summary>
        public int Rating { get; private set; }

        public RatingsData(Guid player, Guid level, int rating)
        {
            Player = player;
            Level = level;
            Rating = rating;
        }
    }
}