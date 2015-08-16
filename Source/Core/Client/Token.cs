using System;

namespace Bricklayer.Core.Client
{
    /// <summary>
    /// Contains the two keys used for authentication, as well as the database UID and username of the user.
    /// </summary>
    internal class Token
    {
        public string Username { get; set; }
        internal Guid UUID { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] PrivateKey { get; set; }
    }
}
