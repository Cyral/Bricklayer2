namespace Bricklayer.Core.Common.Net
{
    /// <summary>
    /// Possible message types.
    /// </summary>
    public enum MessageTypes : byte
    {
        AuthLogin,
        AuthInit,
        FailedLogin,
        Session,
        PublicKey,
        Verified,
        ValidSession,
        Init,
        ServerInfo
    }
}
