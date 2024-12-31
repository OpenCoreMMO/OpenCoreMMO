using LuaNET;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Location;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
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
        RegisterEnumCustom<PlayerFlag>(L, false);
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

    private static void RegisterEnumCustom<T>(LuaState L, bool upperCase = true) where T : Enum
    {
        var prefix = typeof(T).Name.Replace("Type", "") + "_";

        foreach (var item in Enum.GetValues(typeof(T)))
        {
            var name = prefix + item.ToString();

            if (upperCase)
                name = name.ToUpperInvariant();

            RegisterGlobalVariable(L, name, Convert. ToUInt64(item));
        }
    }
}