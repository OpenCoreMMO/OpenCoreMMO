using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class MoveEvent : Script
{
    private ILogger _logger;
    private IScripts _scripts;

    public MoveEvent(LuaScriptInterface scriptInterface, ILogger logger, IScripts scripts) : base(scriptInterface)
    {
        _logger = logger;
        _scripts = scripts;
    }

    public MoveEventType EventType { get; set; }
    public uint RequiredMinLevel { get; set; }
    public uint RequiredMinMagicLevel { get; set; }
    public bool RequirePremium { get; set; }
    public WieldInfoType WieldInfo { get; set; } = WieldInfoType.WIELDINFO_NONE;
    public Slot Slot { get; set; } = Slot.None;
    public List<byte> RequiredVocations { get; set; } = new();
    public string VocationsDescription { get; set; }

    public bool Loaded { get; set; } = false;

    public bool LoadScriptId()
    {
        var luaInterface = _scripts.GetScriptInterface();
        SetScriptId(luaInterface.GetEvent());
        if (GetScriptId() == -1)
        {
            _logger.Error("[CreatureEvent::LoadScriptId] Failed to load event. Script name: '{scriptName}', Module: '{moduloeName}'", luaInterface.GetLoadingScriptName(), luaInterface.GetInterfaceName());
            return false;
        }

        return true;
    }

    public bool IsLoadedScriptId() => GetScriptId() != 0;

    public List<ushort> ItemIdsVector { get; } = new();

    public List<ushort> ActionIdsVector { get; } = new();

    public List<int> UniqueIdsVector { get; } = new();

    public List<Location> PositionsVector { get; } = new();

    #region Step In/Out

    public delegate bool StepFunction(ICreature creature, IItem item, Location fromLocation, Location toLocation);
    public StepFunction OnStepFunction = null;

    public bool FireStepEvent(ICreature creature, IItem item, Location fromLocation, Location toLocation)
    {
        if (IsLoadedScriptId())
            return ExecuteStep(creature, item, fromLocation, toLocation);
        else
            return OnStepFunction?.Invoke(creature, item, fromLocation, toLocation) ?? false;
    }

    public bool ExecuteStep(ICreature creature, IItem item, Location fromPos, Location toPos)
    {
        // onStepIn(creature, item, pos, fromPosition)
        // onStepOut(creature, item, pos, fromPosition)

        // Check if the new position is the same as the old one
        // If it is, log a warning and either teleport the player to their temple position if item type is an teleport
        //var fromPosition = creature.GetLastPosition();
        if (creature is IPlayer player && !item && fromPos == toPos && EventType == MoveEventType.MOVE_EVENT_STEP_IN)
        {
            //if (const ItemType &itemType = Item::items[item->getID()]; player && itemType.isTeleport()) {
            //g_logger().warn("[{}] cannot teleport player: {}, to the same position: {} of fromPosition: {}", __FUNCTION__, player->getName(), pos.toString(), fromPosition.toString());
            //g_game().internalTeleport(player, player->getTemplePosition());
            //player->sendMagicEffect(player->getTemplePosition(), CONST_ME_TELEPORT);
            //player->sendCancelMessage(getReturnMessage(RETURNVALUE_CONTACTADMINISTRATOR));
            //}

            return false;
        }

        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            if (item is not null)
                _logger.Error(@"[MoveEvent::ExecuteStep - Creature {}, item {}, position {}] 
                            Call stack overflow. Too many lua script calls being nested.",
                            creature.Name, item.Name, toPos.ToString());
            else
                _logger.Error(@"[MoveEvent::ExecuteStep - Creature {}, position {}] 
                            Call stack overflow. Too many lua script calls being nested.",
                            creature.Name, toPos.ToString());
            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = GetScriptInterface().GetLuaState();
        GetScriptInterface().PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(L, creature);
        LuaScriptInterface.SetCreatureMetatable(L, -1, creature);
        LuaScriptInterface.PushThing(L, item);
        LuaScriptInterface.PushPosition(L, toPos);
        LuaScriptInterface.PushPosition(L, fromPos);

        return GetScriptInterface().CallFunction(4);
    }

    #endregion

    #region Item Add/Remove 

    public delegate bool MoveItemFunction(IItem item, Location position, IItem tileitem = null);
    public MoveItemFunction OnMoveItemFunction = null;

    public bool FireAddOrRemoveItem(IItem item, Location position, IItem tileitem = null)
    {
        if (IsLoadedScriptId())
            return ExecuteAddOrRemoveItem(item, position, tileitem);
        else
            return OnMoveItemFunction?.Invoke(item, position, tileitem) ?? false;
    }

    public bool ExecuteAddOrRemoveItem(IItem item, Location position, IItem tileitem = null)
    {
        // onAddItem(moveitem, tileitem, pos)
        // onRemoveItem(moveitem, tileitem, pos)
        // onaddItem(moveitem, pos)
        // onRemoveItem(moveitem, pos)
        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error(@"[MoveEvent::ExecuteAddOrRemoveItem - 
		                    Item {} item on tile x: {} y: {} z: {}]
                            Call stack overflow. Too many lua script calls being nested.",
                        item.Name, position.X, position.Y, position.Z);
            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = GetScriptInterface().GetLuaState();
        GetScriptInterface().PushFunction(GetScriptId());

        LuaScriptInterface.PushThing(L, item);

        if (tileitem != null)
            LuaScriptInterface.PushThing(L, tileitem);

        LuaScriptInterface.PushPosition(L, position);

        return GetScriptInterface().CallFunction(tileitem != null ? 3 : 2);
    }

    #endregion

    #region Item Equip

    public delegate bool EquipItemFunction(MoveEvent moveEvent, IPlayer player, IItem item, Slot onSlot, bool isCheck);
    public EquipItemFunction OnEquipItemFunction = null;

    public bool FireEquipItem(IPlayer player, IItem item, Slot onSlot, bool isCheck)
    {
        if (IsLoadedScriptId() && OnEquipItemFunction == null)
            return ExecuteEquip(player, item, onSlot, isCheck);
        else
            return OnEquipItemFunction?.Invoke(this, player, item, onSlot, isCheck) ?? false;
    }

    public bool ExecuteEquip(IPlayer player, IItem item, Slot onSlot, bool isCheck)
    {
        // onEquip(player, item, slot, isCheck)
        // onDeEquip(player, item, slot, isCheck)

        if (!GetScriptInterface().InternalReserveScriptEnv())
        {
            _logger.Error(@"[MoveEvent::ExecuteEquip - Player {}, item {}] 
                            Call stack overflow. Too many lua script calls being nested.",
                        player.Name, item.Name);
            return false;
        }

        var scriptInterface = GetScriptInterface();
        var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
        scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

        var L = GetScriptInterface().GetLuaState();
        GetScriptInterface().PushFunction(GetScriptId());

        LuaScriptInterface.PushUserdata(L, player);
        LuaScriptInterface.SetMetatable(L, -1, "Player");
        LuaScriptInterface.PushThing(L, item);
        Lua.PushNumber(L, (byte)onSlot);
        Lua.PushBoolean(L, isCheck);

        return GetScriptInterface().CallFunction(4);
    }

    #endregion

    //public uint StepInField(ICreature creature, IItem item, Location fromPosition)
    //{
    //    if (!creature)
    //    {
    //        _logger.Error("[MoveEvent::StepInField] - Creature is nullptr");
    //        return 0;
    //    }

    //    if (!item)
    //    {
    //        _logger.Error("[MoveEvent::StepInField] - Item is nullptr");
    //        return 0;
    //    }

    //    //todo: implement his
    //    //var field = item as IMagicField;
    //    //if (field is not null)
    //    //{
    //    //    field.onStepInField(creature);
    //    //    return 1;
    //    //}

    //    //return LUA_ERROR_ITEM_NOT_FOUND;
    //    return 0;

    //    //// onUse(player, item, fromPosition, target, toPosition, isHotkey)
    //    //if (!GetScriptInterface().InternalReserveScriptEnv())
    //    //{
    //    //    if (_logger == null)
    //    //        _logger = Server.Helpers.IoC.GetInstance<ILogger>();

    //    //    _logger.Error($"[Action::executeUse - Player {player.Name}, on item {item.Name}] " +
    //    //                  $"Call stack overflow. Too many lua script calls being nested. Script name {GetScriptInterface().GetLoadingScriptName()}");
    //    //    return false;
    //    //}

    //    //var scriptInterface = GetScriptInterface();
    //    //var scriptEnvironment = scriptInterface.InternalGetScriptEnv();
    //    //scriptEnvironment.SetScriptId(GetScriptId(), GetScriptInterface());

    //    //var L = GetScriptInterface().GetLuaState();
    //    //GetScriptInterface().PushFunction(GetScriptId());

    //    //LuaScriptInterface.PushUserdata(L, player);
    //    //LuaScriptInterface.SetMetatable(L, -1, "Player");

    //    //LuaScriptInterface.PushThing(L, item);
    //    //LuaScriptInterface.PushPosition(L, fromPosition);

    //    //LuaScriptInterface.PushThing(L, target);
    //    //LuaScriptInterface.PushPosition(L, toPosition);

    //    //LuaScriptInterface.PushBoolean(L, isHotkey);

    //    //return GetScriptInterface().CallFunction(6);
    //}

    public void SetItemIdsVector(ushort id)
    {
        ItemIdsVector.Add(id);
    }

    public void SetUniqueIdsVector(ushort id)
    {
        UniqueIdsVector.Add(id);
    }

    public void SetActionIdsVector(ushort id)
    {
        ActionIdsVector.Add(id);
    }

    public void SetPositionsVector(Location pos)
    {
        PositionsVector.Add(pos);
    }

    public bool HasPosition(Location position)
    {
        return PositionsVector.Exists(p => p.Equals(position));
    }

    public List<Location> GetPositions()
    {
        return PositionsVector;
    }

    public void SetPositions(Location pos)
    {
        PositionsVector.Add(pos);
    }

}