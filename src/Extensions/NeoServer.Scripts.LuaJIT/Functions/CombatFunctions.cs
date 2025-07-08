using LuaNET;
using NeoServer.Domain.Combat.Attacks;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Models;
using NeoServer.Scripts.LuaJIT.Models.Combat;
using NeoServer.Server.Common.Contracts;
using LuaDataType = NeoServer.Scripts.LuaJIT.Enums.LuaDataType;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class CombatFunctions : LuaScriptInterface, ICombatFunctions
{
    private static IAttackService _attackService;
    private static IGameCreatureManager _creatureManager;
    private static IMap _map;

    public CombatFunctions(
        IAttackService attackService,
        IGameCreatureManager creatureManager,
        IMap map) : base(nameof(CombatFunctions))
    {
        _attackService = attackService;
        _creatureManager = creatureManager;
        _map = map;
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

    #region Lua Methods

    private static int HandleSetAreaFunction(LuaState L)
    {
        // setArea( {area}, <optional> {extArea} )
        var combat = GetUserdata<LuaCombat>(L, 1);
        if (combat is null)
        {
            Lua.PushNil(L);
            return 1;
        }

        var env = GetScriptEnv();
        if (env.GetScriptId() != EVENT_ID_LOADING)
        {
            ReportError("This function can only be used while loading the script.");
            Lua.PushBoolean(L, false);
            return 1;
        }

        var parameters = Lua.GetTop(L);
        if (parameters >= 3)
        {
            var listAreaExtra = GetArea(L, 3);

            if (listAreaExtra == null || listAreaExtra.Length == 0)
            {
                ReportError("Invalid extra area table.");
                Lua.PushBoolean(L, false);
                return 1;
            }

            combat.Areas[Direction.NorthWest] = listAreaExtra;
            combat.Areas[Direction.NorthEast] = listAreaExtra.Rotate(Direction.NorthEast);
            combat.Areas[Direction.SouthWest] = listAreaExtra.Rotate(Direction.SouthWest);
            combat.Areas[Direction.SouthEast] = listAreaExtra.Rotate(Direction.SouthEast);
        }

        var listArea = GetArea(L, 2);
        if (listArea == null || listArea.Length == 0)
        {
            ReportError("Invalid area table.");
            Lua.PushBoolean(L, false);
            return 1;
        }

        combat.Areas[Direction.West] = listArea;
        combat.Areas[Direction.East] = listArea.Rotate(Direction.East);
        combat.Areas[Direction.North] = listArea.Rotate(Direction.North);
        combat.Areas[Direction.South] = listArea.Rotate(Direction.South);

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

                        //if (target is null)
                        //{
                        //    PushBoolean(lua, false);
                        //    return 1;
                        //}

                        // if (combat->hasArea())
                        // {
                        //     combat->doCombat(creature, target->getPosition());
                        // }
                        // else
                        {
                            var combatParameter = combat.BuildCombatParameter(creature as IPlayer, target);
                            _attackService.Execute(new AttackInput(creature, target, combatParameter));
                        }

                        break;
                    }
                case LuaVariantType.VARIANT_POSITION:
                    {
                        //if (target is null)
                        //{
                        //    PushBoolean(lua, false);
                        //    return 1;
                        //}

                        // if (combat->hasArea())
                        // {
                        //     combat->doCombat(creature, target->getPosition());
                        // }
                        // else
                        {
                            var target = _map.GetTile(variant.Pos);
                            var combatParameter = combat.BuildCombatParameter(creature as IPlayer, target);
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

    #endregion

    private static byte[,] GetArea(LuaState L, int index)
    {
        if (!Lua.IsTable(L, index))
            return null;

        var rows = new List<List<byte>>();

        Lua.PushNil(L);
        while (Lua.Next(L, index) != 0)
        {
            if (!Lua.IsTable(L, -1))
            {
                Lua.Pop(L, 1);
                return null;
            }

            var row = new List<byte>();
            Lua.PushNil(L);
            while (Lua.Next(L, -2) != 0)
            {
                if (!Lua.IsNumber(L, -1))
                {
                    Lua.Pop(L, 2);
                    return null;
                }

                row.Add(GetNumber<byte>(L, -1));
                Lua.Pop(L, 1);
            }

            rows.Add(row);
            Lua.Pop(L, 1);
        }

        if (rows.Count == 0 || rows[0].Count == 0)
            return null;

        int height = rows.Count;
        int width = rows[0].Count;
        var matrix = new byte[height, width];

        for (int y = 0; y < height; y++)
        {
            if (rows[y].Count != width)
                return null;

            for (int x = 0; x < width; x++)
                matrix[y, x] = rows[y][x];
        }

        return matrix;
    }
}