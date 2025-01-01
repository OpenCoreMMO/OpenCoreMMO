using NeoServer.Scripts.LuaJIT.Enums;

namespace NeoServer.Scripts.LuaJIT.Interfaces;

public interface IGlobalEvents
{
    public void Startup();

    public void Timer();

    public void Think();

    public void Execute(GlobalEventType type);

    public Dictionary<string, GlobalEvent> GetEventMap(GlobalEventType type);

    public bool RegisterLuaEvent(GlobalEvent globalEvent);

    public void Clear();
}
