using System.Text.RegularExpressions;
using LuaNET;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Scripts.LuaJIT.Attributes;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Scripts.LuaJIT.Models;
using NeoServer.Scripts.LuaJIT.Models.Combat;
using Serilog;

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
        RegisterEnumCustom<ConditionParamType>(luaState, true, true);
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
        RegisterEnum<CylinderFlagsType>(luaState);

        RegisterEnum<CombatType>(luaState);
        RegisterEnum<CombatParam>(luaState);
        RegisterEnum<MagicEffect>(luaState);
        RegisterEnum<ShootType>(luaState);

        RegisterEnumCustom<SoundEffect>(luaState, prefix: "SOUND_EFFECT_TYPE");
        RegisterEnum<CallBackType>(luaState);
    }

    private static void RegisterEnum(LuaState luaState, string name, Enum value)
    {
        var enumValue = (uint)Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));
        RegisterGlobalVariable(luaState, name, enumValue);
    }

    private static void RegisterEnum<T>(LuaState luaState) where T : Enum
    {
        var type = typeof(T);
        foreach (var item in Enum.GetValues(type))
        {
            var memberName = item.ToString();
            var memberInfo = type.GetMember(memberName).FirstOrDefault();

            // Default name is the enum member name
            string luaName = memberName;

            // Try to get the attribute
            if (memberInfo?.GetCustomAttributes(typeof(LuaEnumNameAttribute), false)
                    .FirstOrDefault() is LuaEnumNameAttribute attr)
                luaName = attr.Name;

            RegisterGlobalVariable(luaState, luaName, Convert.ToUInt32(item));
        }
    }

    private static void RegisterEnumCustom<T>(
        LuaState luaState,
        bool upperCase = true,
        bool addSeparationbewteenWords = false,
        (string, string)? renameFromTo = null,
        string prefix = null) where T : Enum
    {
        prefix ??= typeof(T).Name.Replace("Type", "");
        prefix += "_";

        if (renameFromTo.HasValue)
            prefix = prefix.Replace(renameFromTo.Value.Item1, renameFromTo.Value.Item2);

        foreach (var item in Enum.GetValues(typeof(T)))
        {
            var nameFromEnum = item.ToString();

            nameFromEnum = prefix + nameFromEnum;

            if (addSeparationbewteenWords)
                nameFromEnum = Regex.Replace(nameFromEnum, @"(?<=[a-z])(?=[A-Z])", "_");


            if (upperCase)
                nameFromEnum = nameFromEnum.ToUpperInvariant();

            RegisterGlobalVariable(luaState, nameFromEnum, Convert.ToUInt64(item));
        }
    }
}