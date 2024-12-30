using System.Text;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class Actions : Scripts, IActions
{
    #region Constructors

    public Actions(ILogger logger) : base(logger)
    {
    }

    #endregion

    #region Members

    private readonly Dictionary<ushort, Action> _useItemMap = new();
    private readonly Dictionary<uint, Action> _uniqueItemMap = new();
    private readonly Dictionary<ushort, Action> _actionItemMap = new();
    private readonly Dictionary<Location, Action> _actionPositionMap = new();

    #endregion

    #region Public Methods

    public bool RegisterLuaItemEvent(Action action)
    {
        var itemIdVector = action.ItemIdsVector;
        if (!itemIdVector.Any()) return false;

        var tmpVector = new List<ushort>(itemIdVector.Count);

        foreach (var itemId in itemIdVector)
        {
            // Check if the item is already registered and prevent it from being registered again
            if (HasItemId(itemId))
            {
                _logger.Warning(
                    $"{nameof(RegisterLuaItemEvent)} - Duplicate registered item with id: {itemId} in range from id: {itemIdVector.First()}, to id: {itemIdVector.Last()}, for script: {action.GetScriptInterface().GetLoadingScriptName()}"
                );
                continue;
            }

            // Register item in the action item map
            SetItemId(itemId, action);
            tmpVector.Add(itemId);
        }

        itemIdVector = tmpVector;
        return itemIdVector.Count > 0;
    }

    public bool RegisterLuaUniqueEvent(Action action)
    {
        var uniqueIdVector = action.UniqueIdsVector;
        if (!uniqueIdVector.Any()) return false;

        var tmpVector = new List<uint>(uniqueIdVector.Count);

        foreach (var uniqueId in uniqueIdVector)
            // Check if the unique is already registered and prevent it from being registered again
            if (!HasUniqueId(uniqueId))
            {
                // Register unique id in the unique item map
                SetUniqueId(uniqueId, action);
                tmpVector.Add(uniqueId);
            }
            else
            {
                _logger.Warning(
                    $"{nameof(RegisterLuaUniqueEvent)} - Duplicate registered item with uid: {uniqueId} in range from uid: {uniqueIdVector.First()}, to uid: {uniqueIdVector.Last()}, for script: {action.GetScriptInterface().GetLoadingScriptName()}"
                );
            }

        uniqueIdVector = tmpVector;
        return uniqueIdVector.Count > 0;
    }

    public bool RegisterLuaActionEvent(Action action)
    {
        var actionIdVector = action.ActionIdsVector;
        if (!actionIdVector.Any()) return false;

        var tmpVector = new List<ushort>(actionIdVector.Count);

        foreach (var actionId in actionIdVector)
            // Check if the action is already registered and prevent it from being registered again
            if (!HasActionId(actionId))
            {
                // Register action in the action item map
                SetActionId(actionId, action);
                tmpVector.Add(actionId);
            }
            else
            {
                _logger.Warning(
                    $"{nameof(RegisterLuaActionEvent)} - Duplicate registered item with aid: {actionId} in range from aid: {actionIdVector.First()}, to aid: {actionIdVector.Last()}, for script: {action.GetScriptInterface().GetLoadingScriptName()}"
                );
            }

        actionIdVector = tmpVector;
        return actionIdVector.Count > 0;
    }

    public bool RegisterLuaPositionEvent(Action action)
    {
        var positionVector = action.PositionsVector;
        if (!positionVector.Any()) return false;

        var tmpVector = new List<Location>(positionVector.Count);

        foreach (var position in positionVector)
            // Check if the position is already registered and prevent it from being registered again
            if (!HasPosition(position))
            {
                // Register position in the action position map
                SetPosition(position, action);
                tmpVector.Add(position);
            }
            else
            {
                _logger.Warning(
                    $"{nameof(RegisterLuaPositionEvent)} - Duplicate registered script with range position: {position.ToString()}, for script: {action.GetScriptInterface().GetLoadingScriptName()}"
                );
            }

        positionVector = tmpVector;
        return positionVector.Count > 0;
    }

    public bool RegisterLuaEvent(Action action)
    {
        // Call all register lua events
        if (RegisterLuaItemEvent(action) || RegisterLuaUniqueEvent(action) || RegisterLuaActionEvent(action) ||
            RegisterLuaPositionEvent(action)) return true;

        _logger.Warning(
            $"{nameof(RegisterLuaEvent)} - Missing id/aid/uid/position for one script event, for script: {action.GetScriptInterface().GetLoadingScriptName()}"
        );
        return false;
        _logger.Information(
            $"{nameof(RegisterLuaEvent)} - Missing or incorrect script: {action.GetScriptInterface().GetLoadingScriptName()}");
        return false;
    }

    public bool UseItem(IPlayer player, Location pos, byte index, IItem item, bool isHotkey)
    {
        //todo: implement this?
        //if (item == null)
        //    throw new ArgumentNullException(nameof(item));

        //var it = Item.items[item.ID];

        //if (it.isRune() || it.type == ItemType.Potion)
        //{
        //    if (player.walkExhausted())
        //    {
        //        player.sendCancelMessage(ReturnValue.YOUAREEXHAUSTED);
        //        return false;
        //    }
        //}

        //if (isHotkey)
        //{
        //    ushort subType = item.SubType;
        //    ShowUseHotkeyMessage(player, item, player.GetItemTypeCount(item.ID, subType != item.Count ? subType : -1));
        //}

        //ReturnValue ret = InternalUseItem(player, pos, index, item, isHotkey);
        //if (ret != ReturnValue.NOERROR)
        //{
        //    player.sendCancelMessage(ret);
        //    return false;
        //}

        //if (it.isRune() || it.type == ItemType.Potion)
        //{
        //    player.setNextPotionAction(OTSYS_TIME() + g_configManager().getNumber(ACTIONS_DELAY_INTERVAL, nameof(Actions)));
        //}
        //else
        //{
        //    player.setNextAction(OTSYS_TIME() + g_configManager().getNumber(ACTIONS_DELAY_INTERVAL, nameof(Actions)));
        //}

        //// only send cooldown icon if it's a multi-use item
        //if (it.isMultiUse())
        //{
        //    player.sendUseItemCooldown(g_configManager().getNumber(ACTIONS_DELAY_INTERVAL, nameof(Actions)));
        //}

        return true;
    }

    public bool UseItemEx(IPlayer player, Location fromPos, Location toPos, byte toStackPos, IItem item, bool isHotkey,
        ICreature creature = null)
    {
        //todo: implement this?
        //var it = Item.items[item.ID];
        //if (it.isRune() || it.type == ItemType.Potion)
        //{
        //    if (player.walkExhausted())
        //    {
        //        player.sendCancelMessage(ReturnValue.YOUAREEXHAUSTED);
        //        return false;
        //    }
        //}

        //var action = GetAction(item);
        //if (action == null)
        //{
        //    player.sendCancelMessage(ReturnValue.CANNOTUSETHISOBJECT);
        //    return false;
        //}

        //ReturnValue ret = action.canExecuteAction(player, toPos);
        //if (ret != ReturnValue.NOERROR)
        //{
        //    player.sendCancelMessage(ret);
        //    return false;
        //}

        //if (isHotkey)
        //{
        //    ushort subType = item.SubType;
        //    ShowUseHotkeyMessage(player, item, player.GetItemTypeCount(item.ID, subType != item.Count ? subType : -1));
        //}

        //if (action.useFunction != null)
        //{
        //    if (action.useFunction(player, item, fromPos, action.getTarget(player, creature, toPos, toStackPos), toPos, isHotkey))
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        //if (!action.executeUse(player, item, fromPos, action.getTarget(player, creature, toPos, toStackPos), toPos, isHotkey))
        //{
        //    if (!action.hasOwnErrorHandler())
        //    {
        //        player.sendCancelMessage(ReturnValue.CANNOTUSETHISOBJECT);
        //    }
        //    return false;
        //}

        //if (it.isRune() || it.type == ItemType.Potion)
        //{
        //    player.setNextPotionAction(OTSYS_TIME() + g_configManager().getNumber(EX_ACTIONS_DELAY_INTERVAL, nameof(Actions)));
        //}
        //else
        //{
        //    player.setNextAction(OTSYS_TIME() + g_configManager().getNumber(EX_ACTIONS_DELAY_INTERVAL, nameof(Actions)));
        //}

        //if (it.isMultiUse())
        //{
        //    player.sendUseItemCooldown(g_configManager().getNumber(EX_ACTIONS_DELAY_INTERVAL, nameof(Actions)));
        //}
        return true;
    }

    public ReturnValueType CanUse(IPlayer player, Location pos)
    {
        if (pos.X != 0xFFFF)
        {
            var playerPos = player.Location;
            if (playerPos.Z != pos.Z)
                return playerPos.Z > pos.Z
                    ? ReturnValueType.RETURNVALUE_FIRSTGOUPSTAIRS
                    : ReturnValueType.RETURNVALUE_FIRSTGODOWNSTAIRS;

            //if (!Location.areInRange < 1, 1 > (playerPos, pos))
            //{
            //    return ReturnValueType.RETURNVALUE_TOOFARAWAY;
            //}
        }

        return ReturnValueType.RETURNVALUE_NOERROR;
    }

    public ReturnValueType CanUse(IPlayer player, Location pos, IItem item)
    {
        var action = GetAction(item);
        if (action != null) return action.CanExecuteAction(player, pos);
        return ReturnValueType.RETURNVALUE_NOERROR;
    }

    public ReturnValueType CanUseFar(ICreature creature, Location toPos, bool checkLineOfSight, bool checkFloor)
    {
        //todo: implement this?
        //if (toPos.x == 0xFFFF)
        //{
        //    return ReturnValueType.RETURNVALUE_NOERROR;
        //}

        //Position creaturePos = creature.getPosition();
        //if (checkFloor && creaturePos.z != toPos.z)
        //{
        //    return creaturePos.z > toPos.z ? ReturnValue.FIRSTGOUPSTAIRS : ReturnValue.FIRSTGODOWNSTAIRS;
        //}

        //if (!Position.areInRange < 7, 5 > (toPos, creaturePos))
        //{
        //    return ReturnValue.TOOFARAWAY;
        //}

        //if (checkLineOfSight && !g_game().canThrowObjectTo(creaturePos, toPos))
        //{
        //    return ReturnValueType.RETURNVALUE_CANNOTTHROW;
        //}

        return ReturnValueType.RETURNVALUE_NOERROR;
    }

    public Action GetAction(IItem item)
    {
        if (_uniqueItemMap.TryGetValue(item.UniqueId, out var uniqueIdAction))
            return uniqueIdAction;

        if (_actionItemMap.TryGetValue(item.ActionId, out var actionIdAction))
            return actionIdAction;

        if (_useItemMap.TryGetValue(item.ServerId, out var action))
            return action;

        return null;
    }

    public ReturnValueType InternalUseItem(IPlayer player, Location pos, byte index, IItem item, bool isHotkey)
    {
        //todo: implement this?
        //if (std.shared_ptr < Door > door = item.GetDoor())
        //{
        //    if (!door.CanUse(player))
        //    {
        //        return ReturnValue.CANNOTUSETHISOBJECT;
        //    }
        //}

        //var itemId = item.ID;
        //var itemType = Item.items[itemId];
        //var transformTo = itemType.m_transformOnUse;
        //var action = GetAction(item);
        //if (action == null && transformTo > 0 && itemId != transformTo)
        //{
        //    if (g_game().TransformItem(item, transformTo) == null)
        //    {
        //        Logger.GetInstance().Warn($"{nameof(InternalUseItem)} - item with id {itemId} failed to transform to item {transformTo}");
        //        return ReturnValue.CANNOTUSETHISOBJECT;
        //    }

        //    return ReturnValue.NOERROR;
        //}
        //else if (transformTo > 0 && action != null)
        //{
        //    Logger.GetInstance().Warn($"{nameof(InternalUseItem)} - item with id {itemId} already has an action registered and cannot use the transformTo tag");
        //}

        //if (action != null)
        //{
        //    if (action.IsLoadedCallback())
        //    {
        //        if (action.ExecuteUse(player, item, pos, null, pos, isHotkey))
        //        {
        //            return ReturnValue.NOERROR;
        //        }

        //        if (item.IsRemoved())
        //        {
        //            return ReturnValue.CANNOTUSETHISOBJECT;
        //        }
        //    }
        //    else if (action.UseFunction != null && action.UseFunction(player, item, pos, null, pos, isHotkey))
        //    {
        //        return ReturnValue.NOERROR;
        //    }
        //}

        //if (std.shared_ptr < BedItem > bed = item.GetBed())
        //{
        //    if (!bed.CanUse(player))
        //    {
        //        return ReturnValue.CANNOTUSETHISOBJECT;
        //    }

        //    if (bed.TrySleep(player))
        //    {
        //        player.SetBedItem(bed);
        //        g_game().SendOfflineTrainingDialog(player);
        //    }
        //    return ReturnValue.NOERROR;
        //}

        //if (std.shared_ptr < Container > container = item.GetContainer())
        //{
        //    std.shared_ptr<Container> openContainer;

        //    // depot container
        //    if (std.shared_ptr < DepotLocker > depot = container.GetDepotLocker())
        //    {
        //        std.shared_ptr<DepotLocker> myDepotLocker = player.GetDepotLocker(depot.GetDepotId());
        //        myDepotLocker.SetParent(depot.GetParent().GetTile());
        //        openContainer = myDepotLocker;
        //        player.SetLastDepotId(depot.GetDepotId());
        //    }
        //    else
        //    {
        //        openContainer = container;
        //    }

        //    // reward chest
        //    if (container.GetRewardChest() != null && container.GetParent() != null)
        //    {
        //        if (!player.HasOtherRewardContainerOpen(container.GetParent().GetContainer()))
        //        {
        //            player.RemoveEmptyRewards();
        //        }

        //        std.shared_ptr<RewardChest> playerRewardChest = player.GetRewardChest();
        //        if (playerRewardChest.Empty())
        //        {
        //            return ReturnValue.REWARDCHESTISEMPTY;
        //        }

        //        playerRewardChest.SetParent(container.GetParent().GetTile());
        //        foreach (var (mapRewardId, reward) in player.rewardMap)
        //        {
        //            reward.SetParent(playerRewardChest);
        //        }
        //        openContainer = playerRewardChest;
        //    }

        //    var rewardId = container.GetAttribute<time_t>(ItemAttribute_t.DATE);
        //    // Reward container proxy created when the boss dies
        //    if (container.GetID() == ITEM_REWARD_CONTAINER && !container.GetReward())
        //    {
        //        var reward = player.GetReward(rewardId, false);
        //        if (reward == null)
        //        {
        //            return ReturnValue.THISISIMPOSSIBLE;
        //        }

        //        if (reward.Empty())
        //        {
        //            return ReturnValue.REWARDCONTAINERISEMPTY;
        //        }

        //        reward.SetParent(container.GetRealParent());
        //        openContainer = reward;
        //    }

        //    uint32_t corpseOwner = container.GetCorpseOwner();
        //    if (container.IsRewardCorpse())
        //    {
        //        // only players who participated in the fight can open the corpse
        //        if (player.GetGroup().id >= account.GROUP_TYPE_GAMEMASTER)
        //        {
        //            return ReturnValue.YOUCANTOPENCORPSEADM;
        //        }

        //        var reward = player.GetReward(rewardId, false);
        //        if (reward == null)
        //        {
        //            return ReturnValue.YOUARENOTTHEOWNER;
        //        }

        //        if (reward.Empty())
        //        {
        //            return ReturnValue.REWARDCONTAINERISEMPTY;
        //        }
        //    }
        //    else if (corpseOwner != 0 && !player.CanOpenCorpse(corpseOwner))
        //    {
        //        return ReturnValue.YOUARENOTTHEOWNER;
        //    }

        //    // open/close container
        //    int32_t oldContainerId = player.GetContainerID(openContainer);
        //    if (oldContainerId != -1)
        //    {
        //        player.OnCloseContainer(openContainer);
        //        player.CloseContainer(oldContainerId);
        //    }
        //    else
        //    {
        //        player.AddContainer(index, openContainer);
        //        player.OnSendContainer(openContainer);
        //    }

        //    return ReturnValue.NOERROR;
        //}

        //const ItemType &it = Item.items[item.ID];
        //if (it.CanReadText)
        //{
        //    if (it.CanWriteText)
        //    {
        //        player.SetWriteItem(item, it.MaxTextLen);
        //        player.SendTextWindow(item, it.MaxTextLen, true);
        //    }
        //    else
        //    {
        //        player.SetWriteItem(null);
        //        player.SendTextWindow(item, 0, false);
        //    }

        //    return ReturnValue.NOERROR;
        //}

        return ReturnValueType.RETURNVALUE_CANNOTUSETHISOBJECT;
    }

    public void ShowUseHotkeyMessage(IPlayer player, IItem item, uint count)
    {
        //todo: implement this?
        var ss = new StringBuilder();

        //const ItemType &it = Item.items[item.ID];
        //if (!it.ShowCount)
        //{
        //    ss.Append($"Using one of {item.GetName()}...");
        //}
        //else if (count == 1)
        //{
        //    ss.Append($"Using the last {item.GetName()}...");
        //}
        //else
        //{
        //    ss.Append($"Using one of {count} {item.GetPluralName()}...");
        //}
        //player.SendTextMessage(MESSAGE_HOTKEY_PRESSED, ss.ToString());
    }

    public bool HasPosition(Location position)
    {
        return _actionPositionMap.ContainsKey(position);
    }

    public Dictionary<Location, Action> GetPositionsMap()
    {
        return _actionPositionMap;
    }

    public void SetPosition(Location position, Action action)
    {
        _actionPositionMap.TryAdd(position, action);
    }

    public bool HasItemId(ushort itemId)
    {
        return _useItemMap.ContainsKey(itemId);
    }

    public void SetItemId(ushort itemId, Action action)
    {
        _useItemMap.TryAdd(itemId, action);
    }

    public bool HasUniqueId(uint uniqueId)
    {
        return _uniqueItemMap.ContainsKey(uniqueId);
    }

    public void SetUniqueId(uint uniqueId, Action action)
    {
        _uniqueItemMap.TryAdd(uniqueId, action);
    }

    public bool HasActionId(ushort actionId)
    {
        return _actionItemMap.ContainsKey(actionId);
    }

    public void SetActionId(ushort actionId, Action action)
    {
        _actionItemMap.TryAdd(actionId, action);
    }

    public void Clear()
    {
        _useItemMap.Clear();
        _uniqueItemMap.Clear();
        _actionItemMap.Clear();
        _actionPositionMap.Clear();
    }

    public Action GetAction(ushort id)
    {
        _useItemMap.TryGetValue(id, out var action);
        return action;
    }

    #endregion
}