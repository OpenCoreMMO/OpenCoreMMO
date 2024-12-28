namespace NeoServer.Scripts.LuaJIT;

public interface IScripts
{
    public void ClearAllScripts();

    public bool LoadEventSchedulerScripts(string fileName);

    public bool LoadScripts(string loadPath, bool isLib, bool reload);

    public LuaScriptInterface GetScriptInterface();

    public int GetScriptId();
}
