using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Scripts.LuaJIT.Enums;
using Serilog;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Contracts.Services;

namespace NeoServer.Scripts.LuaJIT;

public class MoveEventList
{
    private readonly List<MoveEvent>[] _moveEvents;

    public MoveEventList()
    {
        _moveEvents = new List<MoveEvent>[(int)MoveEventType.MOVE_EVENT_LAST];

        for (int i = 0; i < _moveEvents.Length; i++)
        {
            _moveEvents[i] = new List<MoveEvent>();
        }
    }

    public List<MoveEvent> this[MoveEventType eventType]
    {
        get
        {
            if (eventType < 0 || eventType >= MoveEventType.MOVE_EVENT_LAST)
                throw new IndexOutOfRangeException("Invalid move event type.");
            return _moveEvents[(int)eventType];
        }
    }
}

public class MoveEvents : IMoveEvents
{
    private readonly ILogger _logger;
    private readonly IItemTypeStore _itemTypeStore;
    private readonly IMap _map;
    private readonly IItemRequirementService _itemRequirementService;
    private readonly IItemAbilityApplierService _itemAbilityApplierService;

    #region Constructors

    public MoveEvents(
        ILogger logger,
        IItemTypeStore itemTypeStore,
        IItemRequirementService itemRequirementService,
        IItemAbilityApplierService itemAbilityApplierService,
        IMap map)
    {
        _logger = logger;
        _itemTypeStore = itemTypeStore;
        _map = map;
        _itemRequirementService = itemRequirementService;
        _itemAbilityApplierService = itemAbilityApplierService;
    }

    #endregion

    #region Members

    private readonly Dictionary<int, MoveEventList> _uniqueIdMap = new();
    private readonly Dictionary<int, MoveEventList> _actionIdMap = new();
    private readonly Dictionary<int, MoveEventList> _itemIdMap = new();
    private readonly Dictionary<Location, MoveEventList> _positionMap = new();

    #endregion

    #region Public Methods

    public bool RegisterLuaItemEvent(MoveEvent moveEvent)
    {
        var itemIdVector = moveEvent.ItemIdsVector;
        if (!itemIdVector.Any()) return false;

        var tmpVector = new List<ushort>(itemIdVector.Count);

        foreach (var itemId in itemIdVector)
            if (RegisterEvent(moveEvent, itemId, _itemIdMap))
                tmpVector.Add(itemId);

        itemIdVector = tmpVector;
        return itemIdVector.Count > 0;
    }

    public bool RegisterLuaUniqueEvent(MoveEvent moveEvent)
    {
        var uniqueIdVector = moveEvent.UniqueIdsVector;
        if (!uniqueIdVector.Any()) return false;

        var tmpVector = new List<int>(uniqueIdVector.Count);

        foreach (var uniqueId in uniqueIdVector)
            if (RegisterEvent(moveEvent, uniqueId, _uniqueIdMap))
            {
                tmpVector.Add(uniqueId);
            }

        uniqueIdVector = tmpVector;
        return uniqueIdVector.Count > 0;
    }

    public bool RegisterLuaActionEvent(MoveEvent moveEvent)
    {
        var actionIdVector = moveEvent.ActionIdsVector;
        if (!actionIdVector.Any()) return false;

        var tmpVector = new List<ushort>(actionIdVector.Count);

        foreach (var actionId in actionIdVector)
            if (RegisterEvent(moveEvent, actionId, _actionIdMap))
            {
                tmpVector.Add(actionId);
            }

        actionIdVector = tmpVector;
        return actionIdVector.Count > 0;
    }

    public bool RegisterLuaPositionEvent(MoveEvent moveEvent)
    {
        var positionVector = moveEvent.PositionsVector;
        if (!positionVector.Any()) return false;

        var tmpVector = new List<Location>(positionVector.Count);

        foreach (var position in positionVector)
            if (RegisterEvent(moveEvent, position, _positionMap))
            {
                tmpVector.Add(position);
            }

        positionVector = tmpVector;
        return positionVector.Count > 0;
    }

    public bool RegisterLuaEvent(MoveEvent moveEvent)
    {
        // Check if event is correct
        if (RegisterLuaItemEvent(moveEvent)
            || RegisterLuaUniqueEvent(moveEvent)
            || RegisterLuaActionEvent(moveEvent)
            || RegisterLuaPositionEvent(moveEvent))
        {
            return true;
        }
        else
        {
            _logger.Warning(
                "[{}] missing id, aid, uid or position for script: {}",
                nameof(RegisterLuaEvent),
                moveEvent.GetScriptInterface().GetLoadingScriptName()
            );
            return false;
        }
    }

    public bool RegisterEvent(MoveEvent moveEvent, int id, Dictionary<int, MoveEventList> moveListMap)
    {
        if (!moveListMap.TryGetValue(id, out MoveEventList moveEventList))
        {
            moveEventList = new MoveEventList();
            moveEventList[moveEvent.EventType].Add(moveEvent);
            moveListMap[id] = moveEventList;
            return true;
        }
        else
        {
            var eventList = moveEventList[moveEvent.EventType];
            foreach (var existingMoveEvent in eventList)
            {
                if (existingMoveEvent.Slot == moveEvent.Slot)
                {
                    _logger.Warning(
                        $"[RegisterEvent] Duplicate move event found: {id}, for script: {moveEvent.GetScriptInterface().GetLoadingScriptName()}"
                    );
                    return false;
                }
            }
            eventList.Add(moveEvent);
            return true;
        }
    }

    public bool RegisterEvent(
        MoveEvent moveEvent,
        Location position,
        Dictionary<Location, MoveEventList> moveListMap)
    {
        if (!moveListMap.TryGetValue(position, out var moveEventList))
        {
            moveEventList = new MoveEventList();
            moveEventList[moveEvent.EventType].Add(moveEvent);
            moveListMap[position] = moveEventList;
            return true;
        }
        else
        {
            var eventListForType = moveEventList[moveEvent.EventType];
            if (eventListForType.Count > 0)
            {
                _logger.Warning(
                    $"[RegisterEvent] Duplicate move event found: {position}, for script {moveEvent.GetScriptInterface().GetLoadingScriptName()}"
                );
                return false;
            }

            eventListForType.Add(moveEvent);
            return true;
        }
    }

    public MoveEvent GetEvent(IItem item, MoveEventType eventType)
    {
        if (item == null)
            return null;

        if (item.UniqueId != 0 &&
            _uniqueIdMap.TryGetValue((int)item.UniqueId, out MoveEventList moveEventListUniqueId) && 
            moveEventListUniqueId[eventType].Count > 0)
                return moveEventListUniqueId[eventType][0];

        if (item.ActionId != 0 &&
            _actionIdMap.TryGetValue(item.ActionId, out MoveEventList moveEventListActionId) &&
            moveEventListActionId[eventType].Count > 0)
                return moveEventListActionId[eventType][0];

        if (_itemIdMap.TryGetValue(item.ServerId, out MoveEventList moveEventListItemId) && 
            moveEventListItemId[eventType].Count > 0)
                return moveEventListItemId[eventType][0];

        return null;
    }

    public MoveEvent GetEvent(Location location, MoveEventType eventType)
    {
        if (_positionMap.TryGetValue(location, out MoveEventList moveEventList))
        {
            var events = moveEventList[eventType];
            if (events.Count > 0)
                return events[0];
        }
        return null;
    }

    public void OnCreatureMove(ICreature creature, Location fromLocation, Location toLocation)
    {
        OnCreatureMoveInternal(creature, fromLocation, toLocation, MoveEventType.MOVE_EVENT_STEP_OUT);
        OnCreatureMoveInternal(creature, fromLocation, toLocation, MoveEventType.MOVE_EVENT_STEP_IN);
    }

    public void OnCreatureMoveInternal(ICreature creature, Location fromLocation, Location toLocation, MoveEventType eventType)
    {
        var pos = eventType == MoveEventType.MOVE_EVENT_STEP_IN ? toLocation : fromLocation;
        var tile = _map.GetTile(pos);

        var moveEvent = GetEvent(pos, eventType);
        if (moveEvent is not null)
        {
            moveEvent.FireStepEvent(creature, null, fromLocation, toLocation);
        }

        if (tile.ItemsCount == 0)
            return;

        foreach (var item in tile.AllItems)
        {
            moveEvent = GetEvent(item, eventType);
            if (moveEvent is not null)
            {
                var step = moveEvent.FireStepEvent(creature, item, fromLocation, toLocation);
                if (!step)
                    break;
            }
        }
    }

    public void OnItemMove(IItem item, ITile tile, bool isAdd)
    {
        MoveEventType eventType1, eventType2;

        if (isAdd)
        {
            eventType1 = MoveEventType.MOVE_EVENT_ADD_ITEM;
            eventType2 = MoveEventType.MOVE_EVENT_ADD_ITEM_ITEMTILE;
        }
        else
        {
            eventType1 = MoveEventType.MOVE_EVENT_REMOVE_ITEM;
            eventType2 = MoveEventType.MOVE_EVENT_REMOVE_ITEM_ITEMTILE;
        }

        var moveEvent = GetEvent(tile.Location, eventType1);

        if (moveEvent is not null)
            moveEvent.FireAddOrRemoveItem(item, tile.Location);

        moveEvent = GetEvent(item, eventType1);

        if (moveEvent is not null)
            moveEvent.FireAddOrRemoveItem(item, tile.Location);

        if (tile.ItemsCount == 0)
            return;

        foreach (var itemFromTile in tile.AllItems)
        {
            moveEvent = GetEvent(itemFromTile, eventType2);
            if (moveEvent is not null)
            {
                var moveItem = moveEvent.FireAddOrRemoveItem(item, tile.Location, itemFromTile);
                if (!moveItem)
                    break;
            }
        }
    }

    public void Clear()
    {
        _actionIdMap.Clear();
        _uniqueIdMap.Clear();
        _itemIdMap.Clear();
        _positionMap.Clear();
    }

    #region Functions

    public bool StepIn(ICreature creature, IItem item, Location fromLocation, Location toLocation)
    {
        if (!creature)
        {
            _logger.Error("[MoveEvent::StepInField] - Creature is null");
            return false;
        }

        if (!item)
        {
            _logger.Error("[MoveEvent::StepInField] - Item is null");
            return false;
        }

        return false;
    }

    public bool StepOut(ICreature creature, IItem item, Location fromLocation, Location toLocation)
    {
        return true;
    }

    public bool AddItem(IItem item, Location position, IItem tileitem = null)
    {
        return true;
    }

    public bool RemoveItem(IItem item, Location position, IItem tileitem = null)
    {
        return true;
    }

    #endregion

    #endregion
}