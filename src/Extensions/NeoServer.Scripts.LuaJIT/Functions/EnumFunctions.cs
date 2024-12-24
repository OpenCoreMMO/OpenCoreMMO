using LuaNET;
using NeoServer.Game.Common.Location;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;
public class EnumFunctions : LuaScriptInterface, IEnumFunctions
{
    public EnumFunctions(
        ILuaEnvironment luaEnvironment, 
        ILogger logger) : base(nameof(ConfigFunctions))
    {
    }

    public void Init(LuaState L)
    {
        //InitDirectionEnums(L);
        RegisterEnumEx<Direction>(L);
        //RegisterEnum<DirectionType>(L);
        RegisterEnum<ReturnValueType>(L);
        RegisterEnum<TileFlagsType>(L);
        RegisterEnum<MessageClassesType>(L);
        RegisterEnum<ReloadType>(L);
    }

    public void InitDirectionEnums(LuaState L)
    {
        RegisterEnum(L, "DIRECTION_NORTH", Direction.North);
        RegisterEnum(L, "DIRECTION_EAST", Direction.East);
        RegisterEnum(L, "DIRECTION_SOUTH", Direction.South);
        RegisterEnum(L, "DIRECTION_WEST", Direction.West);
        RegisterEnum(L, "DIRECTION_SOUTHWEST", Direction.SouthWest);
        RegisterEnum(L, "DIRECTION_SOUTHEAST", Direction.SouthEast);
        RegisterEnum(L, "DIRECTION_NORTHWEST", Direction.NorthWest);
        RegisterEnum(L, "DIRECTION_NORTHEAST", Direction.NorthEast);
        RegisterEnum(L, "DIRECTION_NONE", Direction.None);
    }

    public void InitReturnValueEnums(LuaState L)
    {
        //RegisterEnum(L, "RETURNVALUE_PLAYERISPZLOCKED", InvalidOperation.PlayerIsProtectionZoneLocked);
    }

    private static void RegisterEnum(LuaState L, string name, Enum value)
    {
        var enumValue = (uint)Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));
        RegisterGlobalVariable(L, name, enumValue);
    }

    private static void RegisterEnum<T>(LuaState L) where T : Enum
    {
        foreach (var item in Enum.GetValues(typeof(T)))
            RegisterGlobalVariable(L, item.ToString(), Convert.ToUInt32(item));
    }

    private static void RegisterEnumEx<T>(LuaState L) where T : Enum
    {
        string prefix = typeof(T).Name.ToUpperInvariant() + "_"; 
        foreach (var item in Enum.GetValues(typeof(T)))
        {
            string name = prefix + item.ToString().ToUpperInvariant(); 
            RegisterGlobalVariable(L, name, Convert.ToUInt32(item));
        }
    }
}
