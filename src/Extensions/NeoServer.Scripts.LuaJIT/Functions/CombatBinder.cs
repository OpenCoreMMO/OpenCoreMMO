using LuaNET;
using NeoServer.Domain.Combat.Attacks;
using NeoServer.Domain.Combat.Services.Attacks;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Models;
using NeoServer.Scripts.LuaJIT.Models.Combat;
using NeoServer.Server.Common.Contracts;
using LuaDataType = NeoServer.Scripts.LuaJIT.Enums.LuaDataType;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class CombatBinder : LuaScriptInterface, ICombatFunctionMapper
{
    private static IAttackService _attackService;
    private static IGameCreatureManager _creatureManager;

    public CombatBinder(IAttackService attackService, IGameCreatureManager creatureManager) : base(nameof(CombatBinder))
    {
        _attackService = attackService;
        _creatureManager = creatureManager;
    }

    public void Init(LuaState lua)
    {
        RegisterSharedClass(lua, "Combat", "", HandleCombatCreate);
        RegisterMetaMethod(lua, "Combat", "__eq", LuaUserdataCompare<LuaCombat>);
        
        RegisterMethod(lua, "Combat", "setParameter", HandleSetParameterFunction);
        RegisterMethod(lua, "Combat", "setFormula", HandleSetFormulaFunction);

        RegisterMethod(lua, "Combat", "setArea", HandleSetAreaFunction);
        RegisterMethod(lua, "Combat", "addCondition", HandleNotImplementedFunction);
        RegisterMethod(lua, "Combat", "setCallback", HandleSetCallbackFunction);
        RegisterMethod(lua, "Combat", "setOrigin", HandleNotImplementedFunction);

        RegisterMethod(lua, "Combat", "execute", HandleExecuteFunction);
    }

    private static int HandleSetAreaFunction(LuaState L)
    {
        var combat = GetUserdata<LuaCombat>(L, 1);
        if (combat is null)
        {
            Lua.PushNil(L);
            return 1;
        }

        return 1;
    }

    private static int HandleExecuteFunction(LuaState lua)
    {
        // combat:execute(creature, variant)
        var combat = GetUserdata<LuaCombat>(lua, 1);
        if (combat is null)
        {
            Lua.PushNil(lua);
            return 1;
        }

        if (IsUserdata(lua, 2))
        {
            var type = GetUserdataType(lua, 2);
            if (type != LuaDataType.Player && type != LuaDataType.Monster && type != LuaDataType.Npc)
            {
                PushBoolean(lua, false);
                return 1;
            }

            var creature = GetUserdata<ICreature>(lua, 2);
            var variant = GetVariant(lua, 3);

            switch (variant.Type)
            {
                case LuaVariantType.Number:
                {
                    _creatureManager.TryGetCreature(variant.Number, out var target);

                    if (target is null)
                    {
                        PushBoolean(lua, false);
                        return 1;
                    }

                    // if (combat->hasArea())
                    // {
                    //     combat->doCombat(creature, target->getPosition());
                    // }
                    // else
                    {
                        var combatParameter = combat.BuildCombatParameter(creature as IPlayer);
                        _attackService.Execute(new AttackInput(creature, target, combatParameter));
                    }

                    break;
                }
            }
        }


        Lua.PushNil(lua);
        return 1;
    }

    private static int HandleSetCallbackFunction(LuaState lua)
    {
        // combat:setCallback(key, function)
        var combat = GetUserdata<LuaCombat>(lua, 1);
        if (combat is null)
        {
            Lua.PushNil(lua);
            return 1;
        }

        var key = GetNumber<CallBackType>(lua, 2);
        var callbackName = GetString(lua, 3);

        var callback = combat.SetCallback(key);

        if (callback is null)
        {
            Lua.PushNil(lua);
            return 1;
        }

        PushBoolean(lua, callback.LoadCallback(callbackName));
        return 1;
    }

    private static int HandleSetFormulaFunction(LuaState l)
    {
        // combat:setFormula(type, mina, minb, maxa, maxb)
        var combat = GetUserdata<LuaCombat>(l, 1);
        if (combat is null)
        {
            Lua.PushNil(l);
            return 1;
        }

        var type = GetNumber<CombatFormula>(l, 2);
        var minA = GetNumber<double>(l, 3);
        var minB = GetNumber<double>(l, 4);
        var maxA = GetNumber<double>(l, 5);
        var maxB = GetNumber<double>(l, 6);

        combat.SetPlayerCombatValues(new CombatValues
        {
            CombatFormula = type,
            MinA = minA,
            MinB = minB,
            MaxA = maxA,
            MaxB = maxB
        });

        PushBoolean(l, true);
        return 1;
    }

    private static int HandleSetParameterFunction(LuaState l)
    {
        // combat:setParameter(key, value)
        var combat = GetUserdata<LuaCombat>(l, 1);
        if (combat is null)
        {
            Lua.PushNil(l);
            return 1;
        }

        var key = GetNumber<CombatParam>(l, 2);
        int value;
        if (IsBoolean(l, 3))
            value = GetBoolean(l, 3) ? 1 : 0;
        else
            value = GetNumber<int>(l, 3);

        combat.SetParameter(key, value);
        PushBoolean(l, true);
        return 1;
    }


    private static int HandleCombatCreate(LuaState lua)
    {
        //Combat
        var combat = new LuaCombat(GetScriptEnv().GetScriptInterface());

        PushUserdata(lua, combat);
        SetMetatable(lua, -1, "Combat");
        return 1;
    }
}