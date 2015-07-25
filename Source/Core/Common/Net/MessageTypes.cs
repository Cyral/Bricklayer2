namespace Bricklayer.Core.Common.Net
{
    /// <summary>
    /// Possible message types.
    /// </summary>
    public enum MessageTypes : byte
    {
        //Ordering matters, don't change the order of these!
        AuthLogin,
        AuthInit,
        FailedLogin,
        Session,
        PublicKey,
        Verified,
        ValidSession,
        Init,
        ServerInfo,
        CreateLevel,
        Request,
        Banner,
        PluginDownload,
        PingAuth,
        LevelData,
        JoinLevel,
        Chat,
        PlayerJoin
    }
}
