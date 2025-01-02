﻿using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Services;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class PositionFunctions : LuaScriptInterface, IPositionFunctions
{
    private static ILogger _logger;
    private static IConfigManager _configManager;
    private static IMapTool _mapTool;

    public PositionFunctions(
        ILogger logger,
        IConfigManager configManager,
        IMapTool mapTool) : base(nameof(PositionFunctions))
    {
        _logger = logger;
        _configManager = configManager;
        _mapTool = mapTool;
    }

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "Position", "", LuaCreatePosition);
        RegisterMetaMethod(L, "Position", "__add", LuaPositionAdd);
        RegisterMetaMethod(L, "Position", "__sub", LuaPositionSub);
        RegisterMetaMethod(L, "Position", "__eq", LuaUserdataCompareStruct<Location>);

        RegisterMethod(L, "Position", "getDistance", LuaPositionGetDistance);
        RegisterMethod(L, "Position", "getPathTo", LuaPositionGetPathTo);
        RegisterMethod(L, "Position", "isSightClear", LuaPositionIsSightClear);

        RegisterMethod(L, "Position", "sendMagicEffect", LuaPositionSendMagicEffect);
        RegisterMethod(L, "Position", "toString", LuaPositionToString);
        RegisterMethod(L, "Position", "getNextPosition", LuaPositionGetNextPosition);
    }

    public static int LuaCreatePosition(LuaState L)
    {
        // Position(x = 0, y = 0, z = 0, stackpos = 0)
        // Position(position)
        if (Lua.GetTop(L) <= 1)
        {
            PushPosition(L, new Location());
            return 1;
        }

        var stackpos = 0;

        if (Lua.IsTable(L, 2))
        {
            var position = GetPosition(L, 2, out stackpos);
            PushPosition(L, position);
        }
        else
        {
            var x = GetNumber<ushort>(L, 2, 0);
            var y = GetNumber<ushort>(L, 3, 0);
            var z = GetNumber<byte>(L, 4, 0);
            stackpos = GetNumber(L, 5, 0);

            var position = new Location(x, y, z);

            PushPosition(L, position);
        }

        return 1;
    }

    public static int LuaPositionAdd(LuaState L)
    {
        // positionValue = position + positionEx
        var position = GetPosition(L, 1, out var stackpos);
        Location positionEx;
        if (stackpos == 0)
            positionEx = GetPosition(L, 2, out stackpos);
        else
            positionEx = GetPosition(L, 2);

        position.X += positionEx.X;
        position.Y += positionEx.Y;
        position.Z += positionEx.Z;

        PushPosition(L, position, stackpos);

        return 1;
    }

    public static int LuaPositionSub(LuaState L)
    {
        // positionValue = position - positionEx
        var position = GetPosition(L, 1, out var stackpos);
        Location positionEx;
        if (stackpos == 0)
            positionEx = GetPosition(L, 2, out stackpos);
        else
            positionEx = GetPosition(L, 2);

        position.X -= positionEx.X;
        position.Y -= positionEx.Y;
        position.Z -= positionEx.Z;

        PushPosition(L, position, stackpos);

        return 1;
    }

    public static int LuaPositionGetDistance(LuaState L)
    {
        // position:getDistance(positionEx)
        var position = GetPosition(L, 1);
        var positionEx = GetPosition(L, 2);
        Lua.PushNumber(L, position.GetMaxSqmDistance(positionEx));
        return 1;
    }

    public static int LuaPositionGetPathTo(LuaState L)
    {
        // position:getPathTo(positionEx, minTargetDist = 0, maxTargetDist = 1, fullPathSearch = true, clearSight = true, maxSearchDist = 0)
        var position = GetPosition(L, 1);
        var positionEx = GetPosition(L, 2);

        var fpp = new FindPathParams(true);

        fpp.MinTargetDist = GetNumber<int>(L, 3, 0);
        fpp.MaxTargetDist = GetNumber<int>(L, 4, 1);
        fpp.FullPathSearch = GetBoolean(L, 5, fpp.FullPathSearch);
        fpp.ClearSight = GetBoolean(L, 6, fpp.ClearSight);
        fpp.MaxSearchDist = GetNumber<int>(L, 7, fpp.MaxSearchDist);

        (bool hasPath, Direction[] directions) = _mapTool.PathFinder.Find(position, positionEx, fpp);

        if (hasPath)
        {
            Lua.NewTable(L);

            for (int i = 0; i < directions.Length; i++)
            {
                Lua.PushNumber(L, (byte)directions[i]);
                Lua.RawSetI(L, -2, ++i);
            }
        }
        else
        {
            PushBoolean(L, false);
        }

        return 1;
    }

    public static int LuaPositionIsSightClear(LuaState L)
    {
        // position:isSightClear(positionEx, sameFloor = true)
        var position = GetPosition(L, 1);
        var positionEx = GetPosition(L, 2);
        var sameFloor = GetBoolean(L, 3, true);

        var result = _mapTool.SightClearChecker?.Invoke(position, positionEx, sameFloor);
        PushBoolean(L, result.HasValue ? result.Value : false);
        return 1;
    }

    public static int LuaPositionSendMagicEffect(LuaState L)
    {
        // position:sendMagicEffect(magicEffect, player = nullptr)
        var spectators = new List<ICreature>();
        if (Lua.GetTop(L) >= 3)
        {
            var player = GetUserdata<IPlayer>(L, 3);
            if (player == null)
            {
                ReportError(nameof(LuaPositionSendMagicEffect), GetErrorDesc(ErrorCodeType.LUA_ERROR_PLAYER_NOT_FOUND));
                return 1;
            }

            spectators.Add(player);
        }

        var magicEffect = GetNumber<MagicEffectClassesType>(L, 2);
        if (_configManager.GetBoolean(BooleanConfigType.WARN_UNSAFE_SCRIPTS) /* &&*/
            /*!g_game().isMagicEffectRegistered(magicEffect)*/)
        {
            _logger.Warning(
                "[PositionFunctions::luaPositionSendMagicEffect] An unregistered magic effect type with id '{}' was blocked to prevent client crash.",
                magicEffect);
            Lua.PushBoolean(L, false);
            return 1;
        }

        var position = GetPosition(L, 1);

        EffectService.Send(position, (EffectT)magicEffect, spectators);

        Lua.PushBoolean(L, true);
        return 1;
    }

    public static int LuaPositionToString(LuaState L)
    {
        // position:toString()
        var position = GetPosition(L, 1);
        PushString(L, position.ToString());
        return 1;
    }

    public static int LuaPositionGetNextPosition(LuaState L)
    {
        // position:getNextPosition(direction)
        var direction = GetNumber<Direction>(L, 2);
        var position = GetPosition(L, 1);
        PushPosition(L, position.GetNextLocation(direction));
        return 1;
    }
}