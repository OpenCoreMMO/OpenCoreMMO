using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Server.Services;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class PositionFunctions : LuaScriptInterface, IPositionFunctions
{
    private static ILogger _logger;
    private static IConfigManager _configManager;

    public PositionFunctions(ILogger logger, IConfigManager configManager) : base(nameof(PositionFunctions))
    {
        _logger = logger;
        _configManager = configManager;
    }

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "Position", "", LuaCreatePosition);
        RegisterMetaMethod(L, "Position", "__add", LuaPositionAdd);
        RegisterMetaMethod(L, "Position", "__sub", LuaPositionSub);
        RegisterMetaMethod(L, "Position", "__eq", LuaUserdataCompareStruct<Location>);
        RegisterMethod(L, "Position", "sendMagicEffect", LuaPositionSendMagicEffect);
        RegisterMethod(L, "Position", "toString", LuaPositionToString);
    }

    public static int LuaCreatePosition(LuaState L)
    {
        // Position([x = 0[, y = 0[, z = 0[, stackpos = 0]]]])
        // Position([position])
        if (Lua.GetTop(L) <= 1)
        {
            PushPosition(L, new Location());
            return 1;
        }

        int stackpos = 0;

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
            stackpos = GetNumber<int>(L, 5, 0);

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

    public static int LuaPositionSendMagicEffect(LuaState L)
    {
        // position:sendMagicEffect(magicEffect[, player = nullptr])
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
        if (_configManager.GetBoolean(BooleanConfigType.WARN_UNSAFE_SCRIPTS)/* &&*/
            /*!g_game().isMagicEffectRegistered(magicEffect)*/)
        {
            _logger.Warning("[PositionFunctions::luaPositionSendMagicEffect] An unregistered magic effect type with id '{}' was blocked to prevent client crash.", magicEffect);
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
}
