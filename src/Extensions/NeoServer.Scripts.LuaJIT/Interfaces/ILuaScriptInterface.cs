namespace NeoServer.Scripts.LuaJIT.Interfaces;

public interface ILuaScriptInterface
{
    public bool ReInitState();

    public bool LoadFile(string file, string scriptName);

    public int GetEvent(string eventName);

    public int GetEvent();

    public int GetMetaEvent(string globalName, string eventName);

    public string GetFileById(int scriptId);

    public string GetStackTrace(string errorDesc);

    public bool PushFunction(int functionId);

    public bool InitState();

    public bool CloseState();

    public bool CallFunction(int parameters);

    public void CallVoidFunction(int parameters);

    public string GetInterfaceName();

    public string GetLastLuaError();

    public string GetLoadingFile();

    public string GetLoadingScriptName();

    public void SetLoadingScriptName(string scriptName);
}