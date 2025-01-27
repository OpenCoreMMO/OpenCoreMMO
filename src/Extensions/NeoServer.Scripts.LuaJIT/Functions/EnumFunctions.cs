using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Location;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;
using System.Text.RegularExpressions;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class EnumFunctions : LuaScriptInterface, IEnumFunctions
{
    public EnumFunctions(
        ILuaEnvironment luaEnvironment,
        ILogger logger) : base(nameof(ConfigFunctions))
    {
    }

    public void Init(LuaState luaState)
    {
        RegisterEnumCustom<ConditionType>(luaState, true, true);
        RegisterEnumCustom<Direction>(luaState);
        //RegisterEnum<DirectionType>(luaState);
        RegisterEnumCustom<Gender>(luaState, renameFromTo: ("Gender", "PlayerSex"));
        RegisterEnum<ItemAttributeType>(luaState);
        RegisterEnum<ItemIdType>(luaState);
        RegisterEnum<ItemPropertyType>(luaState);
        RegisterEnum<MagicEffectClassesType>(luaState);
        RegisterEnum<SpeakClassesType>(luaState);
        RegisterEnum<MessageClassesType>(luaState);
        RegisterEnum<NpcsEventType>(luaState);
        RegisterEnumCustom<PlayerFlag>(luaState, false);
        RegisterEnum<ReloadType>(luaState);
        RegisterEnum<ReturnValueType>(luaState);
        //RegisterEnum<SkillsType>(luaState);
        RegisterEnumCustom<SkillType>(luaState);
        RegisterEnum<TileFlagsType>(luaState);
    }

    private static void RegisterEnum(LuaState luaState, string name, Enum value)
    {
        var enumValue = (uint)Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));
        RegisterGlobalVariable(luaState, name, enumValue);
    }

    private static void RegisterEnum<T>(LuaState luaState) where T : Enum
    {
        foreach (var item in Enum.GetValues(typeof(T)))
            RegisterGlobalVariable(luaState, item.ToString(), Convert.ToUInt32(item));
    }

    private static void RegisterEnumCustom<T>(
        LuaState luaState, 
        bool upperCase = true,
        bool addSeparationbewteenWords = false,
        (string, string)? renameFromTo = null) where T : Enum
    {
        var prefix = typeof(T).Name.Replace("Type", "") + "_";

        if (renameFromTo.HasValue)
            prefix = prefix.Replace(renameFromTo.Value.Item1, renameFromTo.Value.Item2);

        foreach (var item in Enum.GetValues(typeof(T)))
        {
            var nameFromEnum = item.ToString();

            if (addSeparationbewteenWords)
                nameFromEnum = Regex.Replace(nameFromEnum, @"(?<=[a-z])(?=[A-Z])", "_");

            var name = prefix + nameFromEnum;

            if (upperCase)
                name = name.ToUpperInvariant();

            RegisterGlobalVariable(luaState, name, Convert. ToUInt64(item));
        }
    }
}