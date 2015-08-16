using System;
using Lidgren.Network;

namespace Bricklayer.Core.Common.Net.Messages
{
    /// <summary>
    /// Contains username and password information to sign into the server.
    /// The server will verify the account is valid
    /// Client => Server
    /// 4x Int: Version (Used for updates)
    /// String: Username
    /// String: Password
    /// </summary>
    public class AuthLoginMessage : IMessage
    {
        public string Password { get; set; }
        public string Username { get; set; }
        public Version Version { get; set; }

        public AuthLoginMessage(Version version, string username, string password)
        {
            Version = version;
            Username = username;
            Password = password;
        }

        public MessageContext Context { get; set; }
        public MessageTypes MessageType => MessageTypes.AuthLogin;

        public void Decode(NetIncomingMessage im)
        {
            Version = im.ReadVersion();
            Username = im.ReadString();
            Password = im.ReadString();
        }

        public void Encode(NetOutgoingMessage om)
        {
            om.Write(Version);
            om.Write(Username);
            om.Write(Password);
        }
    }
}
