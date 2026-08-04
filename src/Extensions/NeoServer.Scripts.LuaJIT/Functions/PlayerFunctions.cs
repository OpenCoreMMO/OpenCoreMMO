using LuaNET;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Guild;
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

        RegisterMethod(luaState, "Player", "isPlayer", LuaPlayerIsPlayer);
        RegisterMethod(luaState, "Player", "getFreeCapacity", LuaPlayerGetFreeCapacity);

        RegisterMethod(luaState, "Player", "getSkillLevel", LuaPlayerGetSkillLevel);
        RegisterMethod(luaState, "Player", "getEffectiveSkillLevel", LuaPlayerGetEffectiveSkillLevel);
        RegisterMethod(luaState, "Player", "getSkillPercent", LuaPlayerGetSkillPercent);
        RegisterMethod(luaState, "Player", "getSkillTries", LuaPlayerGetSkillTries);
        RegisterMethod(luaState, "Player", "addSkillTries", LuaPlayerAddSkillTries);

        RegisterMethod(luaState, "Player", "getSex", LuaPlayerGetSex);
        RegisterMethod(luaState, "Player", "setSex", LuaPlayerSetSex);
        RegisterMethod(luaState, "Player", "getVocation", LuaPlayerGetVocation);

        RegisterMethod(luaState, "Player", "getMana", LuaPlayerGetMana);
        RegisterMethod(luaState, "Player", "addMana", LuaPlayerAddMana);
        RegisterMethod(luaState, "Player", "getManaSpent", LuaPlayerGetManaSpent);
        RegisterMethod(luaState, "Player", "addManaSpent", LuaPlayerAddManaSpent);

        //RegisterMethod(luaState, "Player", "getPronoun", LuaPlayerGetPronoun);

        //RegisterMethod(luaState, "Player", "getTown", LuaPlayerGetTown);

        RegisterMethod(luaState, "Player", "getGroup", LuaPlayerGetGroup);
        RegisterMethod(luaState, "Player", "setGroup", LuaPlayerSetGroup);

        RegisterMethod(luaState, "Player", "getStorageValue", LuaPlayerGetStorageValue);
        RegisterMethod(luaState, "Player", "setStorageValue", LuaPlayerSetStorageValue);

        RegisterMethod(luaState, "Player", "showTextDialog", LuaPlayerShowTextDialog);

        RegisterMethod(luaState, "Player", "addItem", LuaPlayerAddItem);
        RegisterMethod(luaState, "Player", "removeItem", LuaPlayerRemoveItem);

        RegisterMethod(luaState, "Player", "sendTextMessage", LuaPlayerSendTextMessage);

        RegisterMethod(luaState, "Player", "isPzLocked", LuaPlayerIsPzLocked);

        RegisterMethod(luaState, "Player", "setGhostMode", LuaPlayerSetGhostMode);
        RegisterMethod(luaState, "Player", "feed", LuaPlayerFeed);
        RegisterMethod(luaState, "Player", "getLevel", LuaGetLevel);
        RegisterMethod(luaState, "Player", "getSlotItem", LuaPlayerGetSlotItem);

        // Guild methods
        RegisterMethod(luaState, "Player", "getGuild", LuaPlayerGetGuild);
        RegisterMethod(luaState, "Player", "setGuild", LuaPlayerSetGuild);
        RegisterMethod(luaState, "Player", "getGuildLevel", LuaPlayerGetGuildLevel);
        RegisterMethod(luaState, "Player", "getGuildId", LuaPlayerGetGuildId);
        RegisterMethod(luaState, "Player", "setGuildNick", LuaPlayerSetGuildNick);
        RegisterMethod(luaState, "Player", "getGuildNick", LuaPlayerGetGuildNick);
        RegisterMethod(luaState, "Player", "removeMoneyBank", LuaPlayerRemoveMoneyBank);
        RegisterMethod(luaState, "Player", "addMoneyBank", LuaPlayerAddMoneyBank);
        RegisterMethod(luaState, "Player", "inviteToGuild", LuaPlayerInviteToGuild);
        RegisterMethod(luaState, "Player", "kickFromGuild", LuaPlayerKickFromGuild);
        RegisterMethod(luaState, "Player", "hasMoneyBank", LuaPlayerHasMoneyBank);
        RegisterMethod(luaState, "Player", "getGuildRank", LuaPlayerGetGuildRank);
    }

    private static int LuaGetLevel(LuaState l)
    {
        var player = GetUserdata<IPlayer>(l, 1);
        if (player is not null)
        {
            Lua.PushNumber(l, player.Level);
            return 1;
        }

        Lua.PushNil(l);
        return 1;
    }

    private static int LuaPlayerIsPlayer(LuaState luaState)
    {
        // player:isPlayer()
        Lua.PushBoolean(luaState, GetUserdata<IPlayer>(luaState, 1) is not null);
        return 1;
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

    private static int LuaPlayerGetSex(LuaState luaState)
    {
        // player:getSex()
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player != null)
            Lua.PushNumber(luaState, (byte)player.Gender);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    private static int LuaPlayerSetSex(LuaState luaState)
    {
        // player:setSex(newSex)
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player != null)
        {
            var newSex = GetNumber<Gender>(luaState, 2);
            player.Gender = newSex;
            Lua.PushBoolean(luaState, true);
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaPlayerGetMana(LuaState luaState)
    {
        // player:getMana()
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player != null)
            Lua.PushNumber(luaState, player.Mana);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    private static int LuaPlayerAddMana(LuaState luaState)
    {
        // player:addMana(manaChange[, animationOnLoss = false])
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var manaChange = GetNumber<int>(luaState, 2);
        var animationOnLoss = GetBoolean(luaState, 3, false);
        if (!animationOnLoss && manaChange < 0)
            player.DecreaseMana((uint)manaChange);
        else
            player.IncreaseMana((uint)manaChange);

        PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaPlayerGetManaSpent(LuaState luaState)
    {
        // player:getManaSpent()
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player != null)
            Lua.PushNumber(luaState, player.ManaSpent);
        else
            Lua.PushNil(luaState);

        return 1;
    }

    private static int LuaPlayerAddManaSpent(LuaState luaState)
    {
        // player:addManaSpent(amount)
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player != null)
            player.UpdateManaSpent(GetNumber<uint>(luaState, 2));
        else
            Lua.PushNil(luaState);

        return 1;
    }

    //private static int LuaPlayerGetPronoun(LuaState luaState)
    //{
    //    // player:getPronoun()
    //    var player = GetUserdata<IPlayer>(luaState, 1);
    //    if (player != null)
    //        Lua.PushString(luaState, player.GenderPronoun);
    //    else
    //        Lua.PushNil(luaState);

    //    return 1;
    //}

    //private static int LuaPlayerSetPronoun(LuaState luaState)
    //{
    //    // player:setPronoun(newPronoun)
    //    var player = GetUserdata<IPlayer>(luaState, 1);
    //    if (player != null)
    //    {
    //        var newPronoun = GetString(luaState, 2);
    //        player.Gender = newPronoun;
    //        Lua.PushBoolean(luaState, true);
    //    }
    //    else
    //        Lua.PushNil(luaState);

    //    return 1;
    //}

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
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaPlayerSetGroup(LuaState luaState)
    {
        // player:setGroup(group)
        var group = GetUserdata<Group>(luaState, 2);

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
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaPlayerGetStorageValue(LuaState luaState)
    {
        // player:getStorageValue(key)
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player != null)
            Lua.PushNumber(luaState, player.GetStorageValue(GetNumber<uint>(luaState, 2)));
        else
            Lua.PushNil(luaState);

        return 1;
    }

    private static int LuaPlayerSetStorageValue(LuaState luaState)
    {
        // player:setStorageValue(key, value)

        var player = GetUserdata<IPlayer>(luaState, 1);

        if (player is null)
        {
            PushBoolean(luaState, false);
            return 1;
        }

        var key = GetNumber<uint>(luaState, 2);
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
            _logger.Error("Accessing reserved storage key range: {Key}", key);
            PushBoolean(luaState, false);
            return 1;
        }

        if (player != null)
        {
            player.AddOrUpdateStorageValue(key, value);
            PushBoolean(luaState, true);
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    private static int LuaPlayerShowTextDialog(LuaState luaState)
    {
        // player:showTextDialog(id or name or userdata[, text[, canWrite[, length]]])
        var player = GetUserdata<IPlayer>(luaState, 1);

        if (player is null)
        {
            PushBoolean(luaState, false);
            return 1;
        }

        IItem item = null;
        if (Lua.IsNumber(luaState, 2))
        {
            var itemId = GetNumber<ushort>(luaState, 2);
            item = _itemFactory.Create(itemId, Location.Zero);
        }
        else if (IsString(luaState, 2))
        {
            var itemName = GetString(luaState, 2);
            var itemType = _itemTypeStore.GetByName(itemName);

            if (itemType != null)
                item = _itemFactory.Create(itemType, Location.Zero);
        }
        else if (Lua.IsUserData(luaState, 2))
        {
            item = (IReadable)GetUserdata<IItem>(luaState, 2);

            if (item == null)
            {
                Lua.PushBoolean(luaState, false);
                return 1;
            }
        }

        if (item == null)
        {
            ReportError(GetErrorDesc(ErrorCodeType.LUA_ERROR_ITEM_NOT_FOUND));
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        //todo: implements length and canWrite
        var canWrite = GetBoolean(luaState, 4, false);
        var length = GetNumber(luaState, 5, -1);
        var text = string.Empty;

        var parameters = Lua.GetTop(luaState);
        if (parameters >= 3)
            text = GetString(luaState, 3);

        var reliableItem = (IReadable)item;

        reliableItem.Attributes.SetAttribute(ItemAttribute.Text, text);

        player.Read(reliableItem);

        return 1;
    }

    public static int LuaPlayerAddItem(LuaState luaState)
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
                stackCount = int.Min(stackCount, it.Count);
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

        var parameters = Lua.GetTop(luaState);

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

    private static int LuaPlayerSetGhostMode(LuaState luaState)
    {
        // player:setGhostMode(enabled)
        var player = GetUserdata<IPlayer>(luaState, 1);
        var enabled = GetBoolean(luaState, 2);

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

        if (player != null && food > 0) player.Feed(food);

        PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaPlayerGetSlotItem(LuaState luaState)
    {
        // player:getSlotItem(slot)
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var slot = GetNumber<Slot>(luaState, 2);
        var thing = player.Inventory.TryGetItem<IThing>(slot);
        if (thing == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var item = thing as IItem;
        if (item != null)
        {
            PushUserdata(luaState, item);
            SetItemMetatable(luaState, -1, item);
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    // Guild-related methods
    private static int LuaPlayerGetGuild(LuaState luaState)
    {
        // player:getGuild()
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player?.Guild == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        PushUserdata(luaState, player.Guild);
        SetMetatable(luaState, -1, "Guild");
        return 1;
    }

    private static int LuaPlayerSetGuild(LuaState luaState)
    {
        // player:setGuild(guild)
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        // Get guild parameter - can be nil to remove guild
        var guild = GetUserdata<Guild>(luaState, 2);

        try
        {
            player.SetGuild(guild);
            _logger?.Information("Player {PlayerName} joined guild {GuildName}",
                player.Name, guild?.Name ?? "None");

            Lua.PushBoolean(luaState, true);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, "Failed to set guild for player {PlayerName}", player.Name);
            Lua.PushBoolean(luaState, false);
        }

        return 1;
    }

    private static int LuaPlayerGetGuildLevel(LuaState luaState)
    {
        // player:getGuildLevel()
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player?.Guild == null)
        {
            Lua.PushNumber(luaState, 0);
            return 1;
        }

        // TODO: Implement guild level retrieval from player-guild relationship
        // Need to check the guild level/rank from the Guild domain object
        // For now, return 1 (member) as default
        Lua.PushNumber(luaState, 1);
        return 1;
    }

    private static int LuaPlayerGetGuildId(LuaState luaState)
    {
        // player:getGuildId()
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player?.Guild == null)
        {
            Lua.PushNumber(luaState, 0);
            return 1;
        }

        Lua.PushNumber(luaState, player.Guild.Id);
        return 1;
    }

    private static int LuaPlayerSetGuildNick(LuaState luaState)
    {
        // player:setGuildNick(nick)
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player?.Guild == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var nick = GetString(luaState, 2);

        // TODO: Implement guild nick setting in player entity
        // For now, just return true to indicate success
        Lua.PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaPlayerGetGuildNick(LuaState luaState)
    {
        // player:getGuildNick()
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player?.Guild == null)
        {
            PushString(luaState, "");
            return 1;
        }

        // TODO: Implement guild nick retrieval from player entity
        // For now, return empty string
        PushString(luaState, "");
        return 1;
    }

    private static int LuaPlayerRemoveMoneyBank(LuaState luaState)
    {
        // player:removeMoneyBank(amount)
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var amount = GetNumber<ulong>(luaState, 2);

        if (player.BankAmount >= amount)
        {
            player.WithdrawFromBank(amount);
            Lua.PushBoolean(luaState, true);
        }
        else
        {
            Lua.PushBoolean(luaState, false);
        }

        return 1;
    }

    private static int LuaPlayerAddMoneyBank(LuaState luaState)
    {
        // player:addMoneyBank(amount)
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var amount = GetNumber<ulong>(luaState, 2);
        player.Bank.Credit(amount);
        Lua.PushBoolean(luaState, true);

        return 1;
    }

    private static int LuaPlayerInviteToGuild(LuaState luaState)
    {
        // player:inviteToGuild(invitedPlayer)
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player?.Guild == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var invitedPlayer = GetUserdata<IPlayer>(luaState, 2);
        if (invitedPlayer?.Guild != null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        // TODO: Implement guild invitation system
        // 1. Check if player has permission to invite (vice-leader or leader)
        // 2. Add invitation to player's pending invitations
        // 3. Notify invited player

        Lua.PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaPlayerKickFromGuild(LuaState luaState)
    {
        // player:kickFromGuild(targetPlayer)
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player?.Guild == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var targetPlayer = GetUserdata<IPlayer>(luaState, 2);
        if (targetPlayer?.Guild?.Id != player.Guild.Id)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        // TODO: Implement guild kick system
        // 1. Check if player has permission to kick (vice-leader or leader)
        // 2. Check if target rank is lower than kicker
        // 3. Remove target from guild
        // 4. Notify guild members

        Lua.PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaPlayerHasMoneyBank(LuaState luaState)
    {
        // player:hasMoneyBank(amount)
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var amount = GetNumber<ulong>(luaState, 2);

        Lua.PushBoolean(luaState, player.BankAmount >= amount);
        return 1;
    }

    private static int LuaPlayerGetGuildRank(LuaState luaState)
    {
        // player:getGuildRank()
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player?.Guild == null)
        {
            PushString(luaState, "");
            return 1;
        }

        // TODO: Implement guild rank name retrieval from Guild entity
        // For now, we'll return "Member" as default since we don't have
        // the guild level stored in the player entity yet
        var rankName = "Member";

        PushString(luaState, rankName);
        return 1;
    }

    private static int LuaPlayerGetVocation(LuaState luaState)
    {
        // player:getVocation()
        var player = GetUserdata<IPlayer>(luaState, 1);
        if (player != null)
        {
            var vocation = player.Vocation;
            if (vocation != null)
            {
                PushUserdata(luaState, vocation);
                SetMetatable(luaState, -1, "Vocation");
            }
            else
            {
                Lua.PushNil(luaState);
            }
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }
}