using LuaNET;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Services;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class PlayerFunctions : LuaScriptInterface, IPlayerFunctions
{
    private static IGameCreatureManager _gameCreatureManager;
    private static IItemFactory _itemFactory;
    private static IItemTypeStore _itemTypeStore;
    private static ILogger _logger;

    public PlayerFunctions(
        IGameCreatureManager gameCreatureManager,
        IItemFactory itemFactory,
        IItemTypeStore itemTypeStore,
        ILogger logger) : base(nameof(PlayerFunctions))
    {
        _gameCreatureManager = gameCreatureManager;
        _itemFactory = itemFactory;
        _itemTypeStore = itemTypeStore;
        _logger = logger;
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "Player", "Creature", LuaPlayerCreate);
        RegisterMetaMethod(luaState, "Player", "__eq", LuaUserdataCompare<IPlayer>);
        RegisterMethod(luaState, "Player", "teleportTo", LuaTeleportTo);

        RegisterMethod(luaState, "Player", "getFreeCapacity", LuaPlayerGetFreeCapacity);

        RegisterMethod(luaState, "Player", "getSkillLevel", LuaPlayerGetSkillLevel);
        RegisterMethod(luaState, "Player", "getEffectiveSkillLevel", LuaPlayerGetEffectiveSkillLevel);
        RegisterMethod(luaState, "Player", "getSkillPercent", LuaPlayerGetSkillPercent);
        RegisterMethod(luaState, "Player", "getSkillTries", LuaPlayerGetSkillTries);
        RegisterMethod(luaState, "Player", "addSkillTries", LuaPlayerAddSkillTries);

        RegisterMethod(luaState, "Player", "getStorageValue", LuaPlayerGetStorageValue);
        RegisterMethod(luaState, "Player", "setStorageValue", LuaPlayerSetStorageValue);

        RegisterMethod(luaState, "Player", "getGroup", LuaPlayerGetGroup);
        RegisterMethod(luaState, "Player", "setGroup", LuaPlayerSetGroup);

        RegisterMethod(luaState, "Player", "addItem", LuaPlayerAddItem);
        RegisterMethod(luaState, "Player", "removeItem", LuaPlayerRemoveItem);

        RegisterMethod(luaState, "Player", "sendTextMessage", LuaPlayerSendTextMessage);

        RegisterMethod(luaState, "Player", "isPzLocked", LuaPlayerIsPzLocked);

        RegisterMethod(luaState, "Player", "setGhostMode", LuaPlayerSetGhostMode);
        RegisterMethod(luaState, "Player", "feed", LuaPlayerFeed);
    }

    private static int LuaPlayerCreate(LuaState luaState)
    {
        // Player(id or guid or name or userdata)
        IPlayer player = null;
        if (IsNumber(luaState, 2))
        {
            var id = GetNumber<uint>(luaState, 2);
            _gameCreatureManager.TryGetPlayer(id, out player);
        }
        else if (IsString(luaState, 2))
        {
            var name = GetString(luaState, 2);
            _gameCreatureManager.TryGetPlayer(name, out player);

            if (player == null)
            {
                Lua.PushNil(luaState);
                Lua.PushNumber(luaState, (int)ReturnValueType.RETURNVALUE_PLAYERWITHTHISNAMEISNOTONLINE);
                return 2;
            }
        }
        else if (IsUserdata(luaState, 2))
        {
            if (GetUserdataType(luaState, 2) != LuaDataType.Player)
            {
                Lua.PushNil(luaState);
                return 1;
            }

            player = GetUserdata<IPlayer>(luaState, 2);
        }
        else
        {
            player = null;
        }

        if (player != null)
        {
            PushUserdata(luaState, player);
            SetMetatable(luaState, -1, "Player");
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaTeleportTo(LuaState luaState)
    {
        // player:teleportTo(position[, pushMovement = false])
        var pushMovement = GetBoolean(luaState, 3, false);

        var position = GetPosition(luaState, 2);

        var creature = GetUserdata<IPlayer>(luaState, 1);

        if (creature == null)
        {
            ReportError(nameof(LuaTeleportTo), GetErrorDesc(ErrorCodeType.LUA_ERROR_CREATURE_NOT_FOUND));
            PushBoolean(luaState, false);
            return 1;
        }

        creature.TeleportTo(position);

        PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaPlayerGetFreeCapacity(LuaState luaState)
    {
        // player:getFreeCapacity()
        var player = GetUserdata<IPlayer>(luaState, 1);

        if (player is not null)
            Lua.PushNumber(luaState, player.FreeCapacity);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    private static int LuaPlayerGetSkillLevel(LuaState luaState)
    {
        // player:getSkillLevel(skillType)
        var player = GetUserdata<IPlayer>(luaState, 1);
        var skillType = GetNumber<SkillType>(luaState, 2);

        if (player is not null)
            Lua.PushNumber(luaState, player.GetRawSkillLevel(skillType));
        else
            Lua.PushNil(luaState);

        return 1;
    }

    private static int LuaPlayerGetEffectiveSkillLevel(LuaState luaState)
    {
        // player:getEffectiveSkillLevel(skillType)
        var player = GetUserdata<IPlayer>(luaState, 1);
        var skillType = GetNumber<SkillType>(luaState, 2);

        if (player is not null)
            Lua.PushNumber(luaState, player.GetSkillLevel(skillType));
        else
            Lua.PushNil(luaState);

        return 1;
    }

    private static int LuaPlayerGetSkillPercent(LuaState luaState)
    {
        // player:getSkillPercent(skillType)
        var player = GetUserdata<IPlayer>(luaState, 1);
        var skillType = GetNumber<SkillType>(luaState, 2);

        if (player is not null)
            Lua.PushNumber(luaState, player.GetSkillPercent(skillType));
        else
            Lua.PushNil(luaState);

        return 1;
    }

    private static int LuaPlayerGetSkillTries(LuaState luaState)
    {
        // player:getSkillTries(skillType)
        var player = GetUserdata<IPlayer>(luaState, 1);
        var skillType = GetNumber<SkillType>(luaState, 2);

        if (player is not null)
            Lua.PushNumber(luaState, player.GetSkillTries(skillType));
        else
            Lua.PushNil(luaState);

        return 1;
    }

    private static int LuaPlayerAddSkillTries(LuaState luaState)
    {
        // player:addSkillTries(skillType, tries)
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player is not null)
        {
            var skillType = GetNumber<SkillType>(luaState, 2);
            var tries = GetNumber<long>(luaState, 3);
            player.IncreaseSkillCounter(skillType, tries);
            PushBoolean(luaState, true);
        }
        else
        {
            Lua.PushBoolean(luaState, false);
        }
        return 1;
    }

    private static int LuaPlayerAddItem(LuaState luaState)
    {
        // player:addItem(itemId, count = 1, canDropOnMap = true, subType = 1, slot = CONST_SLOT_BACKPACK)

        var player = GetUserdata<IPlayer>(luaState, 1);
        if (!player)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        ushort itemId = 0;
        if (Lua.IsNumber(luaState, 2))
        {
            itemId = GetNumber<ushort>(luaState, 2);
        }
        else
        {
            var itemName = GetString(luaState, 2);
            var itemTypeByName = _itemTypeStore.GetByName(itemName);

            if (itemTypeByName == null || string.IsNullOrEmpty(itemTypeByName.Name) || itemTypeByName.ServerId == 0)
            {
                Lua.PushNil(luaState);
                return 1;
            }

            itemId = itemTypeByName.ServerId;
        }

        var count = GetNumber(luaState, 3, 1);
        var subType = GetNumber(luaState, 5, 1);

        var it = _itemTypeStore.Get(itemId);

        var itemCount = 1;
        var parameters = Lua.GetTop(luaState);
        if (parameters >= 4)
        {
            itemCount = int.Max(1, count);
        }
        else if (it.HasSubType())
        {
            if (it.IsStackable()) itemCount = (int)Math.Ceiling((float)(count / it.Count));

            subType = count;
        }
        else
        {
            itemCount = int.Max(1, count);
        }

        var hasTable = itemCount > 1;
        if (hasTable)
        {
            Lua.NewTable(luaState);
        }
        else if (itemCount == 0)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var canDropOnMap = GetBoolean(luaState, 4, true);
        var slot = GetNumber(luaState, 6, SlotsType.CONST_SLOT_BACKPACK);

        for (var i = 1; i <= itemCount; ++i)
        {
            var stackCount = subType;
            if (it.IsStackable())
            {
                stackCount = int.Max(stackCount, it.Count);
                subType -= stackCount;
            }

            var item = _itemFactory.Create(itemId, player.Location, stackCount);

            if (!item)
            {
                if (!hasTable)
                    Lua.PushNil(luaState);

                return 1;
            }

            var isSuccess = false;

            if (!item.IsPickupable && player.Tile is { } tile && tile.AddItem(item).Succeeded)
            {
                item.Decay?.StartDecay();
                isSuccess = true;
            }
            else
            {
                isSuccess = player.Inventory.AddItem(item, (Slot)slot).Succeeded;

                if (!isSuccess && canDropOnMap && player.Tile is { } playerTile && playerTile.AddItem(item).Succeeded)
                {
                    item.Decay?.StartDecay();
                    isSuccess = true;
                }
            }

            if (isSuccess)
            {
                if (hasTable)
                {
                    Lua.PushNumber(luaState, i);
                    PushUserdata(luaState, item);
                    SetItemMetatable(luaState, -1, item);
                    Lua.SetTable(luaState, -3);
                }
                else
                {
                    PushUserdata(luaState, item);
                    SetItemMetatable(luaState, -1, item);
                }
            }
            else if (hasTable)
            {
                Lua.PushNil(luaState);
            }
        }

        return 1;
    }

    private static int LuaPlayerRemoveItem(LuaState luaState)
    {
        // player:removeItem(itemId, count, subType = -1, ignoreEquipped = false)

        var player = GetUserdata<IPlayer>(luaState, 1);
        if (!player)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        ushort itemId = 0;
        if (Lua.IsNumber(luaState, 2))
        {
            itemId = GetNumber<ushort>(luaState, 2);
        }
        else
        {
            var itemName = GetString(luaState, 2);
            var itemTypeByName = _itemTypeStore.GetByName(itemName);

            if (itemTypeByName == null || string.IsNullOrEmpty(itemTypeByName.Name) || itemTypeByName.ServerId == 0)
            {
                Lua.PushNil(luaState);
                return 1;
            }

            itemId = itemTypeByName.ServerId;
        }

        var count = GetNumber(luaState, 3, 1);
        var subType = GetNumber(luaState, 4, 1);
        var ignoreEquipped = GetBoolean(luaState, 5);

        var result = player.Inventory.RemoveItem(itemId, (byte)count, ignoreEquipped);

        PushBoolean(luaState, result.Succeeded);
        return 1;
    }

    private static int LuaPlayerSendTextMessage(LuaState luaState)
    {
        // player:sendTextMessage(type, text)

        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        int parameters = Lua.GetTop(luaState);

        var messageType = GetNumber<MessageClassesType>(luaState, 2);
        var messageText = GetString(luaState, 3);

        NotificationSenderService.Send(player, messageText, (TextMessageOutgoingType)messageType);
        PushBoolean(luaState, true);

        return 1;
    }

    private static int LuaPlayerIsPzLocked(LuaState luaState)
    {
        // player:isPzLocked()

        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player != null)
            PushBoolean(luaState, player.IsProtectionZoneBlocked);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    private static int LuaPlayerGetStorageValue(LuaState luaState)
    {
        // player:getStorageValue(key)
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player != null)
            Lua.PushNumber(luaState, player.GetStorageValue(GetNumber<int>(luaState, 2)));
        else
            Lua.PushNil(luaState);

        return 1;
    }

    private static int LuaPlayerSetStorageValue(LuaState luaState)
    {
        // player:setStorageValue(key, value)
        var player = GetUserdata<IPlayer>(luaState, 1);
        var key = GetNumber<int>(luaState, 2);
        var value = GetNumber<int>(luaState, 3);

        var startReservedRange = 10000000;
        var endReservedRange = 20000000;

        if (key == 0)
        {
            _logger.Error("Storage key is nil");
            return 1;
        }

        if (key >= startReservedRange && key <= endReservedRange)
        {
            _logger.Error($"Accessing reserved storage key range: {key}");
            PushBoolean(luaState, false);
            return 1;
        }

        if (player != null)
        {
            player.AddOrUpdateStorageValue(key, value);
            PushBoolean(luaState, true);
        }
        else
            Lua.PushNil(luaState);

        return 1;
    }

    private static int LuaPlayerGetGroup(LuaState luaState)
    {
        // player:getGroup()
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player != null)
        {
            PushUserdata(luaState, player.Group);
            SetMetatable(luaState, -1, "Group");
        }
        else
            Lua.PushNil(luaState);

        return 1;
    }

    private static int LuaPlayerSetGroup(LuaState luaState)
    {
        // player:setGroup(group)
        var group = GetUserdata<IGroup>(luaState, 2);

        if (group is null)
        {
            PushBoolean(luaState, false);
            return 1;
        }

        var player = GetUserdata<IPlayer>(luaState, 1);

        if (player is not null)
        {
            player.Group = group;
            PushBoolean(luaState, true);
        }
        else
            Lua.PushNil(luaState);

        return 1;
    }

    private static int LuaPlayerSetGhostMode(LuaState luaState)
    {
        // player:setGhostMode(enabled)
        var player = GetUserdata<IPlayer>(luaState, 1);
        bool enabled = GetBoolean(luaState, 2);

        if (player != null && player.IsInvisible != enabled)
        {
            if (enabled)
                player.TurnInvisible();
            else
                player.TurnVisible();
        }

        PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaPlayerFeed(LuaState luaState)
    {
        // player:feed(food)
        var player = GetUserdata<IPlayer>(luaState, 1);
        var food = GetNumber(luaState, 2, 0);

        if (player != null && food > 0)
        {
            player.Feed(food);
        }

        PushBoolean(luaState, true);
        return 1;
    }
}
