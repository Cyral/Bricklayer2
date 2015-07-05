namespace Bricklayer.Core.Common.Net
{
    /// <summary>
    /// Represents which side of the connection a message is being read or sent on.
    /// </summary>
    public enum MessageContext
    {
        /// <summary>
        /// The message is being encoded or decoded on the client side.
        /// </summary>
        Client,

        /// <summary>
        /// The message is being encoded or decoded on the server side.
        /// </summary>
        Server
    }
}
