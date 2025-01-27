using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Creatures.Npcs;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class NpcFunctions : LuaScriptInterface, INpcFunctions
{
    private static IGameCreatureManager _gameCreatureManager;

    public NpcFunctions(IGameCreatureManager gameCreatureManager) : base(nameof(NpcFunctions))
    {
        _gameCreatureManager = gameCreatureManager;
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "Npc", "Creature", LuaNpcCreate);
        RegisterMetaMethod(luaState, "Npc", "__eq", LuaUserdataCompare<INpc>);
        RegisterMethod(luaState, "Npc", "isNpc", LuaNpcIsNpc);

        RegisterMethod(luaState, "Npc", "setPlayerInteraction", LuaNpcSetPlayerInteraction);
        RegisterMethod(luaState, "Npc", "removePlayerInteraction", LuaNpcRemovePlayerInteraction);
        RegisterMethod(luaState, "Npc", "isInteractingWithPlayer", LuaNpcIsInteractingWithPlayer);
        RegisterMethod(luaState, "Npc", "isInTalkRange", LuaNpcIsInTalkRange);
        RegisterMethod(luaState, "Npc", "isPlayerInteractingOnTopic", LuaNpcIsPlayerInteractingOnTopic);

        RegisterMethod(luaState, "Npc", "openShopWindow", LuaNpcOpenShopWindow);
        RegisterMethod(luaState, "Npc", "closeShopWindow", LuaNpcCloseShopWindow);

        RegisterMethod(luaState, "Npc", "isMerchant", LuaNpcIsMerchant);
    }

    private static int LuaNpcCreate(LuaState luaState)
    {
        // Npc([id or name or userdata])
        INpc npc = null;
        if (IsNumber(luaState, 2))
        {
            var id = GetNumber<uint>(luaState, 2);
            _gameCreatureManager.TryGetCreature(id, out var creature);

            if (creature is INpc)
            {
                npc = creature as INpc;
            }
            else
            {
                Lua.PushNil(luaState);
                return 1;
            }
        }
        else if (IsUserdata(luaState, 2))
        {
            if (GetUserdataType(luaState, 2) != LuaDataType.Npc)
            {
                Lua.PushNil(luaState);
                return 1;
            }

            npc = GetUserdata<INpc>(luaState, 2);
        }
        else
        {
            npc = null;
        }

        if (npc is not null)
        {
            PushUserdata(luaState, npc);
            SetMetatable(luaState, -1, "Npc");
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaNpcIsNpc(LuaState luaState)
    {
        // npc:isNpc()
        Lua.PushBoolean(luaState, GetUserdata<INpc>(luaState, 1) is not null);
        return 1;
    }

    private static int LuaNpcSetPlayerInteraction(LuaState luaState)
    {
        // npc:setPlayerInteraction(player, topic = 0)
        var npc = GetUserdata<INpc>(luaState, 1);
        var player = GetUserdata<IPlayer>(luaState, 2);
        var topicId = GetNumber<ushort>(luaState, 3);

        if (!npc)
        {
            ReportError(GetErrorDesc(ErrorCodeType.LUA_ERROR_NPC_NOT_FOUND));
            Lua.PushNil(luaState);
            return 1;
        }

        if (!player)
        {
            ReportError(GetErrorDesc(ErrorCodeType.LUA_ERROR_PLAYER_NOT_FOUND));
            Lua.PushNil(luaState);
            return 1;
        }

        npc.SetPlayerInteraction(player, topicId);
        Lua.PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaNpcRemovePlayerInteraction(LuaState luaState)
    {
        // npc:removePlayerInteraction()
        var npc = GetUserdata<INpc>(luaState, 1);
        var player = GetUserdata<IPlayer>(luaState, 2);

        if (!npc)
        {
            ReportError(GetErrorDesc(ErrorCodeType.LUA_ERROR_NPC_NOT_FOUND));
            Lua.PushNil(luaState);
            return 1;
        }

        if (!player)
        {
            ReportError(GetErrorDesc(ErrorCodeType.LUA_ERROR_PLAYER_NOT_FOUND));
            Lua.PushNil(luaState);
            return 1;
        }

        npc.RemovePlayerInteraction(player);
        Lua.PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaNpcIsInteractingWithPlayer(LuaState luaState)
    {
        // npc:isInteractingWithPlayer(player)
        var npc = GetUserdata<INpc>(luaState, 1);
        var player = GetUserdata<IPlayer>(luaState, 2);

        if (!npc)
        {
            ReportError(GetErrorDesc(ErrorCodeType.LUA_ERROR_NPC_NOT_FOUND));
            Lua.PushNil(luaState);
            return 1;
        }

        if (!player)
        {
            ReportError(GetErrorDesc(ErrorCodeType.LUA_ERROR_PLAYER_NOT_FOUND));
            Lua.PushNil(luaState);
            return 1;
        }

        Lua.PushBoolean(luaState, npc.IsInteractingWithPlayer(player));
        //Lua.PushBoolean(luaState, false);
        return 1;
    }

    private static int LuaNpcIsInTalkRange(LuaState luaState)
    {
        // npc:isInTalkRange(position, range = 4)
        var npc = GetUserdata<INpc>(luaState, 1);
        var position = GetPosition(luaState, 2);
        var range = GetNumber(luaState, 3, 4);

        if (!npc)
        {
            ReportError(GetErrorDesc(ErrorCodeType.LUA_ERROR_NPC_NOT_FOUND));
            Lua.PushNil(luaState);
            return 1;
        }

        Lua.PushBoolean(luaState, npc.CanInteract(position, range));
        return 1;
    }

    private static int LuaNpcIsPlayerInteractingOnTopic(LuaState luaState)
    {
        // npc:isPlayerInteractingOnTopic(player, topicId = 0)
        var npc = GetUserdata<INpc>(luaState, 1);
        var player = GetUserdata<IPlayer>(luaState, 2);
        var topicId = GetNumber<ushort>(luaState, 3);

        if (!npc)
        {
            ReportError(GetErrorDesc(ErrorCodeType.LUA_ERROR_NPC_NOT_FOUND));
            Lua.PushNil(luaState);
            return 1;
        }

        if (!player)
        {
            ReportError(GetErrorDesc(ErrorCodeType.LUA_ERROR_PLAYER_NOT_FOUND));
            Lua.PushNil(luaState);
            return 1;
        }

        Lua.PushBoolean(luaState, npc.IsPlayerInteractingOnTopic(player, topicId));
        //Lua.PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaNpcOpenShopWindow(LuaState luaState)
    {
        // npc:openShopWindow(player)
        var npc = GetUserdata<INpc>(luaState, 1);
        var player = GetUserdata<IPlayer>(luaState, 2);
        var topicId = GetNumber<ushort>(luaState, 3);

        if (!npc || npc is not IShopperNpc shopperNpc)
        {
            ReportError(GetErrorDesc(ErrorCodeType.LUA_ERROR_NPC_NOT_FOUND));
            Lua.PushNil(luaState);
            return 1;
        }

        if (!player)
        {
            ReportError(GetErrorDesc(ErrorCodeType.LUA_ERROR_PLAYER_NOT_FOUND));
            Lua.PushNil(luaState);
            return 1;
        }

        player.StopShopping();
        shopperNpc.StartSellingToCustomer(player);

        Lua.PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaNpcCloseShopWindow(LuaState luaState)
    {
        // npc:closeShopWindow(player)
        var npc = GetUserdata<INpc>(luaState, 1);
        var player = GetUserdata<IPlayer>(luaState, 2);
        var topicId = GetNumber<ushort>(luaState, 3);

        if (!npc || npc is not IShopperNpc shopperNpc)
        {
            ReportError(GetErrorDesc(ErrorCodeType.LUA_ERROR_NPC_NOT_FOUND));
            Lua.PushNil(luaState);
            return 1;
        }

        if (!player)
        {
            ReportError(GetErrorDesc(ErrorCodeType.LUA_ERROR_PLAYER_NOT_FOUND));
            Lua.PushNil(luaState);
            return 1;
        }

        player.StopShopping();
        shopperNpc.StopSellingToCustomer(player);

        Lua.PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaNpcIsMerchant(LuaState luaState)
    {
        // npc:isMerchant()
        var npc = GetUserdata<INpc>(luaState, 1);
        Lua.PushBoolean(luaState, npc is not null && npc is IShopperNpc);
        return 1;
    }
}