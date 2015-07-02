using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Common
{
    /// <summary>
    /// Represents the most basic form of user data, straight from the Joomla! database (Containing only name and ID)
    /// </summary>
    public class UserData 
    {
        /// <summary>
        /// The user's username (not real name)
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The permanent unique ID of the user.
        /// </summary>
        /// <remarks>
        /// This ID is different than the temporary server ID assigned.
        /// </remarks>
        public string UUID { get; set; }

        public UserData()
        {

        }

        public UserData(string username, string uuid)
        {
            Username = username;
            UUID = uuid;
        }
    }
}
