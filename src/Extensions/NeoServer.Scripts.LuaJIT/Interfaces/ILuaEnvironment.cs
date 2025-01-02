using LuaNET;
using NeoServer.Scripts.LuaJIT.Enums;

namespace NeoServer.Scripts.LuaJIT.Interfaces;

public interface ILuaEnvironment : ILuaScriptInterface
{
    public uint LastEventTimerId { get; set; }

    public Dictionary<uint, LuaTimerEventDesc> TimerEvents { get; }

    public LuaState GetLuaState();

    public bool InitState();

    public bool ReInitState();

    public bool CloseState();

    public LuaScriptInterface GetTestInterface();

    public bool IsShuttingDown();

    public void ExecuteTimerEvent(uint eventIndex);

    public void CollectGarbage();
}