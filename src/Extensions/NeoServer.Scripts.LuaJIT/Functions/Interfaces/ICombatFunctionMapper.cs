using LuaNET;

namespace NeoServer.Scripts.LuaJIT.Functions.Interfaces;

public interface ICombatFunctionMapper
{
    void Init(LuaState lua);
}