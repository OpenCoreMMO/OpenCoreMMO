namespace NeoServer.Scripts.LuaJIT;

public class ArgManager
{
    #region Members

    #endregion

    #region Instance

    private static ArgManager _instance = null;
    public static ArgManager GetInstance() => _instance == null ? _instance = new ArgManager() : _instance;

    #endregion

    #region Properties

    public string ExePath { get; set; }

    #endregion
}
