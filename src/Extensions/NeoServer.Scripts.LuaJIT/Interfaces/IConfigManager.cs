using LuaNET;
using NeoServer.Scripts.LuaJIT.Enums;

namespace NeoServer.Scripts.LuaJIT.Interfaces;

public interface IConfigManager
{
    public bool Load(string file);

    //public bool Reload();

    public string GetString(StringConfigType what);

    public int GetNumber(IntegerConfigType what);

    public short GetShortNumber(IntegerConfigType what);

    public ushort GetUShortNumber(IntegerConfigType what);

    public bool GetBoolean(BooleanConfigType what);

    public float GetFloat(FloatingConfigType what);

    public string SetConfigFileLua(string what);

    public string GetConfigFileLua();

    public string GetGlobalString(LuaState luaState, string identifier, string defaultValue);

    public int GetGlobalNumber(LuaState luaState, string identifier, int defaultValue = 0);

    public bool GetGlobalBoolean(LuaState luaState, string identifier, bool defaultValue);

    public float GetGlobalFloat(LuaState luaState, string identifier, float defaultValue = 0.0f);
}