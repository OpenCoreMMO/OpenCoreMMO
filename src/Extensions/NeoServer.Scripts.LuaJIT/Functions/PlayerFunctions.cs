using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Services;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class PlayerFunctions : LuaScriptInterface, IPlayerFunctions
{
    private static IGameCreatureManager _gameCreatureManager;

    public PlayerFunctions(
        ILogger logger,
        ILuaEnvironment luaEnvironment, 
        IGameCreatureManager gameCreatureManager) : base(nameof(PlayerFunctions))
    {
        _gameCreatureManager = gameCreatureManager;
    }

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "Player", "Creature", LuaPlayerCreate);
        RegisterMetaMethod(L, "Player", "__eq", LuaUserdataCompare<IPlayer>);
        RegisterMethod(L, "Player", "teleportTo", LuaTeleportTo);

        RegisterMethod(L, "Player", "sendTextMessage", LuaPlayerSendTextMessage);
        RegisterMethod(L, "Player", "isPzLocked", LuaPlayerIsPzLocked);
    }

    private static int LuaPlayerCreate(LuaState L)
    {
        // Player(id or guid or name or userdata)
        IPlayer player = null;
        if (IsNumber(L, 2))
        {
            var id = GetNumber<uint>(L, 2);
            _gameCreatureManager.TryGetPlayer(id, out player);
        }
        else if (IsString(L, 2))
        {
            var name = GetString(L, 2);
            _gameCreatureManager.TryGetPlayer(name, out player);

            if (player == null)
            {
                Lua.PushNil(L);
                Lua.PushNumber(L, (int)ReturnValueType.RETURNVALUE_PLAYERWITHTHISNAMEISNOTONLINE);
                return 2;
            }
        }
        else if (IsUserdata(L, 2))
        {
            if (GetUserdataType(L, 2) != LuaDataType.Player)
            {
                Lua.PushNil(L);
                return 1;
            }
            player = GetUserdata<IPlayer>(L, 2);
        }
        else
        {
            player = null;
        }

        if (player != null)
        {
            PushUserdata(L, player);
            SetMetatable(L, -1, "Player");
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

    private static int LuaTeleportTo(LuaState L)
    {
        // creature:teleportTo(position[, pushMovement = false])
        bool pushMovement = GetBoolean(L, 3, false);

        var position = GetPosition(L, 2);

        var creature = GetUserdata<IPlayer>(L, 1);

        if (creature == null)
        {
            ReportError(nameof(LuaTeleportTo), GetErrorDesc(ErrorCodeType.LUA_ERROR_CREATURE_NOT_FOUND));
            PushBoolean(L, false);
            return 1;
        }

        creature.TeleportTo(position);

        PushBoolean(L, true);
        return 1;
    }

    private static int LuaPlayerSendTextMessage(LuaState L)
    {
        // player:sendTextMessage(type, text[, position, primaryValue = 0, primaryColor = TEXTCOLOR_NONE[, secondaryValue = 0, secondaryColor = TEXTCOLOR_NONE]])
        // player:sendTextMessage(type, text, channelId)

        var player = GetUserdata<IPlayer>(L, 1);
        if (player == null)
        {
            Lua.PushNil(L);
            return 1;
        }

        int parameters = Lua.GetTop(L);

        var messageType = GetNumber<MessageClassesType>(L, 2);
        var messageText = GetString(L, 3);

        //its not used
        //if (parameters == 4)
        //{
        //    var channelId = GetNumber<ushort>(L, 4);

        //    const auto &channel = g_chat().getChannel(player, channelId);
        //    if (!channel || !channel->hasUser(player))
        //    {
        //        pushBoolean(L, false);
        //        return 1;
        //    }
        //    message.channelId = channelId;
        //}
        //else
        //{
        //    if (parameters >= 6)
        //    {
        //        message.position = getPosition(L, 4);
        //        message.primary.value = getNumber<int32_t>(L, 5);
        //        message.primary.color = getNumber<TextColor_t>(L, 6);
        //    }

        //    if (parameters >= 8)
        //    {
        //        message.secondary.value = getNumber<int32_t>(L, 7);
        //        message.secondary.color = getNumber<TextColor_t>(L, 8);
        //    }
        //}

        NotificationSenderService.Send(player, messageText, (TextMessageOutgoingType)messageType);
        PushBoolean(L, true);

        return 1;
    }

    private static int LuaPlayerIsPzLocked(LuaState L)
    {
        // player:isPzLocked()

        var player = GetUserdata<IPlayer>(L, 1);
        if (player != null)
            PushBoolean(L, player.IsProtectionZoneLocked);
        else
            Lua.PushNil(L);

        return 1;
    }
}
