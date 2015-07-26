using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricklayer.Core.Client
{
    /// <summary>
    /// Contains the 2 keys used for authentication, as well as the database UID and username of the user.
    /// </summary>
    internal class Token
    {
        public string Username { get; set; }
        internal Guid UUID { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }
}
