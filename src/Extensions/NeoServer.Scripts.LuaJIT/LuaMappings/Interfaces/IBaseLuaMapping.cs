using LuaNET;

namespace NeoServer.Scripts.LuaJIT.LuaMappings.Interfaces;

public interface IBaseLuaMapping
{
    void Init(LuaState L);
}