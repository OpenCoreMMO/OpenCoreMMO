using LuaNET;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Location;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
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
        RegisterEnumCustom<Direction>(L);
        //RegisterEnum<DirectionType>(L);
        RegisterEnum<ItemAttributeType>(L);
        RegisterEnum<ItemIdType>(L);
        RegisterEnum<MagicEffectClassesType>(L);
        RegisterEnum<MessageClassesType>(L);
        RegisterEnum<ReloadType>(L);
        RegisterEnum<ReturnValueType>(L);
        //RegisterEnum<SkillsType>(L);
        RegisterEnumCustom<SkillType>(L);
        RegisterEnum<TileFlagsType>(L);
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

    private static void RegisterEnumCustom<T>(LuaState L) where T : Enum
    {
        string prefix = typeof(T).Name.Replace("Type", "").ToUpperInvariant() + "_"; 
        foreach (var item in Enum.GetValues(typeof(T)))
        {
            string name = prefix + item.ToString().ToUpperInvariant(); 
            RegisterGlobalVariable(L, name, Convert.ToUInt32(item));
        }
    }
}
