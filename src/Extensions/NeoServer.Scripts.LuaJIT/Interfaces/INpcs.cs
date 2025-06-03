using NeoServer.Scripts.LuaJIT.Enums;

namespace NeoServer.Scripts.LuaJIT.Interfaces;

public interface INpcs
{
    void Add(string npcName, NpcsEventType eventType, LuaScriptInterface luaScriptInterface);
    bool LoadCallback(string npcName);
    NpcEvents GetEvents(string npcName);
    void Clear();
}
