using NeoServer.Scripts.LuaJIT.Enums;

namespace NeoServer.Scripts.LuaJIT.Models;

public enum CallBackType
{
    [LuaEnumName("CALLBACK_PARAM_LEVELMAGICVALUE")]
    LevelMagicValue,

    [LuaEnumName("CALLBACK_PARAM_SKILLVALUE")]
    SkillValue,

    [LuaEnumName("CALLBACK_PARAM_TARGETTILE")]
    TargetTile,

    [LuaEnumName("CALLBACK_PARAM_TARGETCREATURE")]
    TargetCreature,

    [LuaEnumName("CALLBACK_PARAM_CHAINVALUE")]
    ChainValue,

    [LuaEnumName("CALLBACK_PARAM_CHAINPICKER")]
    ChainPicker
}