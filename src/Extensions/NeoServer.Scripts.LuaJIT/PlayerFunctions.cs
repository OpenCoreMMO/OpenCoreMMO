using LuaNET;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Scripts.LuaJIT;

public class PlayerFunctions : LuaScriptInterface
{
    public PlayerFunctions() : base(nameof(PlayerFunctions))
    {
    }

    public static void Init(LuaState L)
    {
        RegisterSharedClass(L, "Player", "Creature", LuaPlayerCreate);
        RegisterMetaMethod(L, "Player", "__eq", LuaUserdataCompare<IPlayer>);
        //RegisterMethod(L, "Player", "teleportTo", LuaTeleportTo);

        RegisterMethod(L, "Player", "sendTextMessage", LuaPlayerSendTextMessage);
    }

    private static int LuaPlayerCreate(LuaState L)
    {
        // Player(id or guid or name or userdata)
        IPlayer player = null;
        if (IsNumber(L, 2))
        {
            var id = GetNumber<int>(L, 2);
            //if (id >= Player::getFirstID() && id <= Player::getLastID())
            //{
            //    player = g_game().getPlayerByID(id);
            //}
            //else
            //{
            //    player = g_game().getPlayerByGUID(id);
            //}
        }
        else if (IsString(L, 2))
        {
            //ReturnValue ret = g_game().getPlayerByNameWildcard(getString(L, 2), player);
            //if (ret != RETURNVALUE_NOERROR)
            //{
            //    lua_pushnil(L);
            //    lua_pushnumber(L, ret);
            //    return 2;
            //}
        }
        else if (IsUserdata(L, 2))
        {
            //if (GetUserdataType(L, 2) != LuaData_t::Player)
            //{
            //    lua_pushnil(L);
            //    return 1;
            //}
            //player = GetUserdata<Player>(L, 2);
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

    //private static int LuaTeleportTo(LuaState L)
    //{
    //    // creature:teleportTo(position[, pushMovement = false])
    //    bool pushMovement = GetBoolean(L, 3, false);

    //    var position = GetPosition(L, 2);

    //    var creature = GetUserdata<Player>(L, 1);

    //    if (creature == null)
    //    {
    //        ReportError(nameof(LuaTeleportTo), GetErrorDesc(ErrorCodeType.LUA_ERROR_CREATURE_NOT_FOUND));
    //        PushBoolean(L, false);
    //        return 1;
    //    }

    //    var oldPosition = creature.Position;
    //    if (Game.GetInstance().InternalTeleport(creature, position, pushMovement))
    //    {
    //        Logger.GetInstance().Error($"[{nameof(LuaTeleportTo)}] Failed to teleport creature {creature.Name}, on position {oldPosition}, error code: {0}");// GetReturnMessage(ret));
    //        PushBoolean(L, false);
    //        return 1;
    //    }

    //    PushBoolean(L, true);
    //    return 1;
    //}

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

        //TextMessage message(GetNumber<MessageClassesType>(L, 2), GetString(L, 3));
        var messageType = GetNumber<MessageClassesType>(L, 2);
        var messageText = GetString(L, 3);

        if (parameters == 4)
        {
            //uint16_t channelId = getNumber<uint16_t>(L, 4);
            //const auto &channel = g_chat().getChannel(player, channelId);
            //if (!channel || !channel->hasUser(player))
            //{
            //    pushBoolean(L, false);
            //    return 1;
            //}
            //message.channelId = channelId;
        }
        else
        {
            //if (parameters >= 6)
            //{
            //    message.position = getPosition(L, 4);
            //    message.primary.value = getNumber<int32_t>(L, 5);
            //    message.primary.color = getNumber<TextColor_t>(L, 6);
            //}

            //if (parameters >= 8)
            //{
            //    message.secondary.value = getNumber<int32_t>(L, 7);
            //    message.secondary.color = getNumber<TextColor_t>(L, 8);
            //}
        }

        //player->sendTextMessage(message);

        //Game.GetInstance().Logger.Information($"LuaPlayerSendTextMessage: player {player.Name}, message: {messageText}");

        player.Say(messageText, SpeechType.Say);

        PushBoolean(L, true);

        return 1;
    }
}
