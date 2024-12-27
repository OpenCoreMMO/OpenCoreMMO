using LuaNET;

namespace NeoServer.Scripts.LuaJIT.Functions.Interfaces;

public interface IBaseFunctions
{
    void Init(LuaState L);
}