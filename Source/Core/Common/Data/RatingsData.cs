using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Common.Data
{
    /// <summary>
    /// Data for ratings of level
    /// </summary>
    public class RatingsData
    {
        /// <summary>
        /// Player Guid of rater
        /// </summary>
        public Guid Player { get; private set; }

        /// <summary>
        /// Guid of level rated
        /// </summary>
        public Guid Level { get; private set; }

        /// <summary>
        /// Rating of level
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
