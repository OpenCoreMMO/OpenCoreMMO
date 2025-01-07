using LuaNET;
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

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "Position", "", LuaCreatePosition);
        RegisterMetaMethod(luaState, "Position", "__add", LuaPositionAdd);
        RegisterMetaMethod(luaState, "Position", "__sub", LuaPositionSub);
        RegisterMetaMethod(luaState, "Position", "__eq", LuaUserdataCompareStruct<Location>);

        RegisterMethod(luaState, "Position", "getDistance", LuaPositionGetDistance);
        RegisterMethod(luaState, "Position", "getPathTo", LuaPositionGetPathTo);
        RegisterMethod(luaState, "Position", "isSightClear", LuaPositionIsSightClear);

        RegisterMethod(luaState, "Position", "sendMagicEffect", LuaPositionSendMagicEffect);
        RegisterMethod(luaState, "Position", "toString", LuaPositionToString);
        RegisterMethod(luaState, "Position", "getNextPosition", LuaPositionGetNextPosition);
    }

    public static int LuaCreatePosition(LuaState luaState)
    {
        // Position(x = 0, y = 0, z = 0, stackpos = 0)
        // Position(position)
        if (Lua.GetTop(luaState) <= 1)
        {
            PushPosition(luaState, new Location());
            return 1;
        }

        var stackpos = 0;

        if (Lua.IsTable(luaState, 2))
        {
            var position = GetPosition(luaState, 2, out stackpos);
            PushPosition(luaState, position);
        }
        else
        {
            var x = GetNumber<ushort>(luaState, 2, 0);
            var y = GetNumber<ushort>(luaState, 3, 0);
            var z = GetNumber<byte>(luaState, 4, 0);
            stackpos = GetNumber(luaState, 5, 0);

            var position = new Location(x, y, z);

            PushPosition(luaState, position);
        }

        return 1;
    }

    public static int LuaPositionAdd(LuaState luaState)
    {
        // positionValue = position + positionEx
        var position = GetPosition(luaState, 1, out var stackpos);
        Location positionEx;
        if (stackpos == 0)
            positionEx = GetPosition(luaState, 2, out stackpos);
        else
            positionEx = GetPosition(luaState, 2);

        position.X += positionEx.X;
        position.Y += positionEx.Y;
        position.Z += positionEx.Z;

        PushPosition(luaState, position, stackpos);

        return 1;
    }

    public static int LuaPositionSub(LuaState luaState)
    {
        // positionValue = position - positionEx
        var position = GetPosition(luaState, 1, out var stackpos);
        Location positionEx;
        if (stackpos == 0)
            positionEx = GetPosition(luaState, 2, out stackpos);
        else
            positionEx = GetPosition(luaState, 2);

        position.X -= positionEx.X;
        position.Y -= positionEx.Y;
        position.Z -= positionEx.Z;

        PushPosition(luaState, position, stackpos);

        return 1;
    }

    public static int LuaPositionGetDistance(LuaState luaState)
    {
        // position:getDistance(positionEx)
        var position = GetPosition(luaState, 1);
        var positionEx = GetPosition(luaState, 2);
        Lua.PushNumber(luaState, position.GetMaxSqmDistance(positionEx));
        return 1;
    }

    public static int LuaPositionGetPathTo(LuaState luaState)
    {
        // position:getPathTo(positionEx, minTargetDist = 0, maxTargetDist = 1, fullPathSearch = true, clearSight = true, maxSearchDist = 0)
        var position = GetPosition(luaState, 1);
        var positionEx = GetPosition(luaState, 2);

        var fpp = new FindPathParams(true);

        fpp.MinTargetDist = GetNumber<int>(luaState, 3, 0);
        fpp.MaxTargetDist = GetNumber<int>(luaState, 4, 1);
        fpp.FullPathSearch = GetBoolean(luaState, 5, fpp.FullPathSearch);
        fpp.ClearSight = GetBoolean(luaState, 6, fpp.ClearSight);
        fpp.MaxSearchDist = GetNumber<int>(luaState, 7, fpp.MaxSearchDist);

        (bool hasPath, Direction[] directions) = _mapTool.PathFinder.Find(position, positionEx, fpp);

        if (hasPath)
        {
            Lua.NewTable(luaState);

            for (int i = 0; i < directions.Length; i++)
            {
                Lua.PushNumber(luaState, (byte)directions[i]);
                Lua.RawSetI(luaState, -2, ++i);
            }
        }
        else
        {
            PushBoolean(luaState, false);
        }

        return 1;
    }

    public static int LuaPositionIsSightClear(LuaState luaState)
    {
        // position:isSightClear(positionEx, sameFloor = true)
        var position = GetPosition(luaState, 1);
        var positionEx = GetPosition(luaState, 2);
        var sameFloor = GetBoolean(luaState, 3, true);

        var result = _mapTool.SightClearChecker?.Invoke(position, positionEx, sameFloor);
        PushBoolean(luaState, result.HasValue ? result.Value : false);
        return 1;
    }

    public static int LuaPositionSendMagicEffect(LuaState luaState)
    {
        // position:sendMagicEffect(magicEffect, player = nullptr)
        var spectators = new List<ICreature>();
        if (Lua.GetTop(luaState) >= 3)
        {
            var player = GetUserdata<IPlayer>(luaState, 3);
            if (player == null)
            {
                ReportError(nameof(LuaPositionSendMagicEffect), GetErrorDesc(ErrorCodeType.LUA_ERROR_PLAYER_NOT_FOUND));
                return 1;
            }

            spectators.Add(player);
        }

        var magicEffect = GetNumber<MagicEffectClassesType>(luaState, 2);
        if (_configManager.GetBoolean(BooleanConfigType.WARN_UNSAFE_SCRIPTS) /* &&*/
            /*!g_game().isMagicEffectRegistered(magicEffect)*/)
        {
            _logger.Warning(
                "[PositionFunctions::luaPositionSendMagicEffect] An unregistered magic effect type with id '{}' was blocked to prevent client crash.",
                magicEffect);
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var position = GetPosition(luaState, 1);

        EffectService.Send(position, (EffectT)magicEffect, spectators);

        Lua.PushBoolean(luaState, true);
        return 1;
    }

    public static int LuaPositionToString(LuaState luaState)
    {
        // position:toString()
        var position = GetPosition(luaState, 1);
        PushString(luaState, position.ToString());
        return 1;
    }

    public static int LuaPositionGetNextPosition(LuaState luaState)
    {
        // position:getNextPosition(direction)
        var direction = GetNumber<Direction>(luaState, 2);
        var position = GetPosition(luaState, 1);
        PushPosition(luaState, position.GetNextLocation(direction));
        return 1;
    }
}