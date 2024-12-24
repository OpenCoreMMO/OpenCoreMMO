using LuaNET;

namespace NeoServer.Scripts.LuaJIT;

public interface ILuaEnvironment : ILuaScriptInterface
{
    public LuaState GetLuaState();

    public bool InitState();

    public bool ReInitState();

    public bool CloseState();

    public LuaScriptInterface GetTestInterface();

    public bool IsShuttingDown();

    public void ExecuteTimerEvent(uint eventIndex);

    public void CollectGarbage();
}
