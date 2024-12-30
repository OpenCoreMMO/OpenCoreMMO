using LuaNET;
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

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "Player", "Creature", LuaPlayerCreate);
        RegisterMetaMethod(L, "Player", "__eq", LuaUserdataCompare<IPlayer>);
        RegisterMethod(L, "Player", "teleportTo", LuaTeleportTo);

        RegisterMethod(L, "Player", "getFreeCapacity", LuaPlayerGetFreeCapacity);

        RegisterMethod(L, "Player", "getSkillLevel", LuaPlayerGetSkillLevel);
        RegisterMethod(L, "Player", "getEffectiveSkillLevel", LuaPlayerGetEffectiveSkillLevel);
        RegisterMethod(L, "Player", "getSkillPercent", LuaPlayerGetSkillPercent);
        RegisterMethod(L, "Player", "getSkillTries", LuaPlayerGetSkillTries);
        RegisterMethod(L, "Player", "addSkillTries", LuaPlayerAddSkillTries);

        RegisterMethod(L, "Player", "getStorageValue", LuaPlayerGetStorageValue);
        RegisterMethod(L, "Player", "setStorageValue", LuaPlayerSetStorageValue);

        RegisterMethod(L, "Player", "getGroup", LuaPlayerGetGroup);
        RegisterMethod(L, "Player", "setGroup", LuaPlayerSetGroup);

        RegisterMethod(L, "Player", "addItem", LuaPlayerAddItem);
        RegisterMethod(L, "Player", "removeItem", LuaPlayerRemoveItem);

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
        // player:teleportTo(position[, pushMovement = false])
        var pushMovement = GetBoolean(L, 3, false);

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

    private static int LuaPlayerGetFreeCapacity(LuaState L)
    {
        // player:getFreeCapacity()
        var player = GetUserdata<IPlayer>(L, 1);

        if (player is not null)
            Lua.PushNumber(L, player.FreeCapacity);
        else
            Lua.PushNil(L);

        return 1;
    }

    private static int LuaPlayerGetSkillLevel(LuaState L)
    {
        // player:getSkillLevel(skillType)
        var player = GetUserdata<IPlayer>(L, 1);
        var skillType = GetNumber<SkillType>(L, 2);

        if (player is not null)
            Lua.PushNumber(L, player.GetRawSkillLevel(skillType));
        else
            Lua.PushNil(L);

        return 1;
    }

    private static int LuaPlayerGetEffectiveSkillLevel(LuaState L)
    {
        // player:getEffectiveSkillLevel(skillType)
        var player = GetUserdata<IPlayer>(L, 1);
        var skillType = GetNumber<SkillType>(L, 2);

        if (player is not null)
            Lua.PushNumber(L, player.GetSkillLevel(skillType));
        else
            Lua.PushNil(L);

        return 1;
    }

    private static int LuaPlayerGetSkillPercent(LuaState L)
    {
        // player:getSkillPercent(skillType)
        var player = GetUserdata<IPlayer>(L, 1);
        var skillType = GetNumber<SkillType>(L, 2);

        if (player is not null)
            Lua.PushNumber(L, player.GetSkillPercent(skillType));
        else
            Lua.PushNil(L);

        return 1;
    }

    private static int LuaPlayerGetSkillTries(LuaState L)
    {
        // player:getSkillTries(skillType)
        var player = GetUserdata<IPlayer>(L, 1);
        var skillType = GetNumber<SkillType>(L, 2);

        if (player is not null)
            Lua.PushNumber(L, player.GetSkillTries(skillType));
        else
            Lua.PushNil(L);

        return 1;
    }

    private static int LuaPlayerAddSkillTries(LuaState L)
    {
        // player:addSkillTries(skillType, tries)
        var player = GetUserdata<IPlayer>(L, 1);
        if (player is not null)
        {
            var skillType = GetNumber<SkillType>(L, 2);
            var tries = GetNumber<long>(L, 3);
            player.IncreaseSkillCounter(skillType, tries);
            PushBoolean(L, true);
        }
        else
        {
            Lua.PushBoolean(L, false);
        }
        return 1;
    }

    private static int LuaPlayerAddItem(LuaState L)
    {
        // player:addItem(itemId, count = 1, canDropOnMap = true, subType = 1, slot = CONST_SLOT_BACKPACK)

        var player = GetUserdata<IPlayer>(L, 1);
        if (!player)
        {
            Lua.PushBoolean(L, false);
            return 1;
        }

        ushort itemId = 0;
        if (Lua.IsNumber(L, 2))
        {
            itemId = GetNumber<ushort>(L, 2);
        }
        else
        {
            var itemName = GetString(L, 2);
            var itemTypeByName = _itemTypeStore.GetByName(itemName);

            if (itemTypeByName == null || string.IsNullOrEmpty(itemTypeByName.Name) || itemTypeByName.ServerId == 0)
            {
                Lua.PushNil(L);
                return 1;
            }

            itemId = itemTypeByName.ServerId;
        }

        var count = GetNumber(L, 3, 1);
        var subType = GetNumber(L, 5, 1);

        var it = _itemTypeStore.Get(itemId);

        var itemCount = 1;
        var parameters = Lua.GetTop(L);
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
            Lua.NewTable(L);
        }
        else if (itemCount == 0)
        {
            Lua.PushNil(L);
            return 1;
        }

        var canDropOnMap = GetBoolean(L, 4, true);
        var slot = GetNumber(L, 6, SlotsType.CONST_SLOT_BACKPACK);

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
                    Lua.PushNil(L);

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
                    Lua.PushNumber(L, i);
                    PushUserdata(L, item);
                    SetItemMetatable(L, -1, item);
                    Lua.SetTable(L, -3);
                }
                else
                {
                    PushUserdata(L, item);
                    SetItemMetatable(L, -1, item);
                }
            }
            else if (hasTable)
            {
                Lua.PushNil(L);
            }
        }

        return 1;
    }

    private static int LuaPlayerRemoveItem(LuaState L)
    {
        // player:removeItem(itemId, count, subType = -1, ignoreEquipped = false)

        var player = GetUserdata<IPlayer>(L, 1);
        if (!player)
        {
            Lua.PushBoolean(L, false);
            return 1;
        }

        ushort itemId = 0;
        if (Lua.IsNumber(L, 2))
        {
            itemId = GetNumber<ushort>(L, 2);
        }
        else
        {
            var itemName = GetString(L, 2);
            var itemTypeByName = _itemTypeStore.GetByName(itemName);

            if (itemTypeByName == null || string.IsNullOrEmpty(itemTypeByName.Name) || itemTypeByName.ServerId == 0)
            {
                Lua.PushNil(L);
                return 1;
            }

            itemId = itemTypeByName.ServerId;
        }

        var count = GetNumber(L, 3, 1);
        var subType = GetNumber(L, 4, 1);
        var ignoreEquipped = GetBoolean(L, 5); 

        var result = player.Inventory.RemoveItem(itemId, (byte)count, ignoreEquipped);

        PushBoolean(L, result.Succeeded);
        return 1;
    }

    private static int LuaPlayerSendTextMessage(LuaState L)
    {
        // player:sendTextMessage(type, text)

        var player = GetUserdata<IPlayer>(L, 1);
        if (player == null)
        {
            Lua.PushNil(L);
            return 1;
        }

        int parameters = Lua.GetTop(L);

        var messageType = GetNumber<MessageClassesType>(L, 2);
        var messageText = GetString(L, 3);

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

    private static int LuaPlayerGetStorageValue(LuaState L)
    {
        // player:getStorageValue(key)
        var player = GetUserdata<IPlayer>(L, 1);
        if (player != null)
            Lua.PushNumber(L, player.GetStorageValue(GetNumber<int>(L, 2)));
        else
            Lua.PushNil(L);

        return 1;
    }

    private static int LuaPlayerSetStorageValue(LuaState L)
    {
        // player:setStorageValue(key, value)
        var player = GetUserdata<IPlayer>(L, 1);
        var key = GetNumber<int>(L, 2);
        var value = GetNumber<int>(L, 3);

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
            PushBoolean(L, false);
            return 1;
        }

        if (player != null)
        {
            player.AddOrUpdateStorageValue(key, value);
            PushBoolean(L, true);
        }
        else
            Lua.PushNil(L);

        return 1;
    }

    private static int LuaPlayerGetGroup(LuaState L)
    {
        // player:getGroup()
        var player = GetUserdata<IPlayer>(L, 1);
        if (player != null)
            PushUserdata(L, player.Group);
        else
            Lua.PushNil(L);

        return 1;
    }

    private static int LuaPlayerSetGroup(LuaState L)
    {
        // player:setGroup(group)
        var group = GetUserdata<IGroup>(L, 2);

        if (group is null)
        {
            PushBoolean(L, false);
            return 1;
        }

        var player = GetUserdata<IPlayer>(L, 1);

        if (player is not null)
        {
            player.Group = group;
            PushBoolean(L, true);
        }
        else
            Lua.PushNil(L);

        return 1;
    }
}
