using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Common
{
    /// Represents the most basic form of user data, straight from the Joomla! database (Containing only name and ID)
    /// </summary>
    public class UserData 
    {
        /// <summary>
        /// The user's username (not real name)
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The database ID of the user, defined in the Joomla! database
        /// </summary>
        /// <remarks>
        /// Note that this is different than any other IDs used, such as those to identify the connection number
        /// Guests will have a DatabaseID equal to their ID, negative
        /// </remarks>
        public int DatabaseID { get; set; }

        public UserData()
        {

        }

        public UserData(string userName, int databaseId)
        {
            Username = userName;
            DatabaseID = databaseId;
        }
    }
}
