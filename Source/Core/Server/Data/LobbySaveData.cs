using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Server.Data
{
    /// <summary>
    /// Room data for displaying in the lobby list, to be serialized to JSON
    /// </summary>
    public class LobbySaveData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Online { get; set; }
        public double Rating { get; set; }
        public int Plays { get; set; }
        public int ID { get; set; }

        public LobbySaveData(string name, int id, string description, int players, int plays, double rating)
        {
            Name = name;
            ID = id;
            Description = description;
            Online = players;
            Rating = rating;
            Plays = plays;
        }

        /// <summary>
        /// Creates a LobbySaveData instance from a Map
        /// </summary>
        //public static LobbySaveData FromMap(World.Map map)
        //{
        //    return new LobbySaveData(map.Name, map.ID, map.Description, map.Online, map.Plays, map.Rating);
        //}
    }
}
