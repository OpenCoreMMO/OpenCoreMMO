using LuaNET;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class MoveEventFunctions : LuaScriptInterface, IMoveEventFunctions
{
    private static ILogger _logger;
    private static IMoveEvents _moveEvents;
    private static IScripts _scripts;
    private static IVocationStore _vocationStore;

    public MoveEventFunctions(
        ILogger logger,
        IMoveEvents moveEvents,
        IScripts scripts,
        IVocationStore vocationStore) : base(nameof(MoveEventFunctions))
    {
        _logger = logger;
        _moveEvents = moveEvents;
        _scripts = scripts;
        _vocationStore = vocationStore;
    }

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "MoveEvent", "", LuaCreateMoveEvent);
        RegisterMethod(L, "MoveEvent", "type", LuaMoveEventType);
        RegisterMethod(L, "MoveEvent", "register", LuaMoveEventRegister);
        RegisterMethod(L, "MoveEvent", "level", LuaMoveEventLevel);
        RegisterMethod(L, "MoveEvent", "magicLevel", LuaMoveEventMagicLevel);
        RegisterMethod(L, "MoveEvent", "slot", LuaMoveEventSlot);
        RegisterMethod(L, "MoveEvent", "id", LuaMoveEventItemId);
        RegisterMethod(L, "MoveEvent", "idRange", LuaMoveEventItemIdRange);
        RegisterMethod(L, "MoveEvent", "aid", LuaMoveEventItemActionId);
        RegisterMethod(L, "MoveEvent", "uid", LuaMoveEventItemUniqueId);
        RegisterMethod(L, "MoveEvent", "position", LuaMoveEventPosition);
        RegisterMethod(L, "MoveEvent", "premium", LuaMoveEventPremium);
        RegisterMethod(L, "MoveEvent", "vocation", LuaMoveEventVocation);
        RegisterMethod(L, "MoveEvent", "onEquip", LuaMoveEventOnCallback);
        RegisterMethod(L, "MoveEvent", "onDeEquip", LuaMoveEventOnCallback);
        RegisterMethod(L, "MoveEvent", "onStepIn", LuaMoveEventOnCallback);
        RegisterMethod(L, "MoveEvent", "onStepOut", LuaMoveEventOnCallback);
        RegisterMethod(L, "MoveEvent", "onAddItem", LuaMoveEventOnCallback);
        RegisterMethod(L, "MoveEvent", "onRemoveItem", LuaMoveEventOnCallback);
    }

    private static int LuaCreateMoveEvent(LuaState L)
    {
        var moveEvent = new MoveEvent(GetScriptEnv().GetScriptInterface(), _logger, _scripts);
        PushUserdata(L, moveEvent);
        SetMetatable(L, -1, "MoveEvent");
        return 1;
    }

    private static int LuaMoveEventType(LuaState L)
    {
        // moveEvent:type(callback)
        var moveEvent = GetUserdata<MoveEvent>(L, 1);
        if (moveEvent != null)
        {
            string typeName = GetString(L, 2);
            string tmpStr = typeName.ToLower();
            if (tmpStr == "stepin")
            {
                moveEvent.EventType = MoveEventType.MOVE_EVENT_STEP_IN;
                moveEvent.OnStepFunction = _moveEvents.StepIn;
            }
            else if (tmpStr == "stepout")
            {
                moveEvent.EventType = MoveEventType.MOVE_EVENT_STEP_OUT;
                moveEvent.OnStepFunction = _moveEvents.StepOut;
            }
            else if (tmpStr == "equip")
            {
                moveEvent.EventType = MoveEventType.MOVE_EVENT_EQUIP;
                moveEvent.OnEquipItemFunction = _moveEvents.EquipItem;
            }
            else if (tmpStr == "deequip")
            {
                moveEvent.EventType = MoveEventType.MOVE_EVENT_DEEQUIP;
                moveEvent.OnEquipItemFunction = _moveEvents.DeEquipItem;
            }
            else if (tmpStr == "additem")
            {
                moveEvent.EventType = MoveEventType.MOVE_EVENT_ADD_ITEM_ITEMTILE;
                moveEvent.OnMoveItemFunction = _moveEvents.AddItem;
            }
            else if (tmpStr == "removeitem")
            {
                moveEvent.EventType = MoveEventType.MOVE_EVENT_ADD_ITEM_ITEMTILE;
                moveEvent.OnMoveItemFunction = _moveEvents.RemoveItem;
            }
            else
            {
                _logger.Error("[MoveEventFunctions::LuaMoveEventType] - Invalid type for Move event: {}");
                PushBoolean(L, false);
            }

            moveEvent.Loaded = true;
            PushBoolean(L, true);
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

    private static int LuaMoveEventRegister(LuaState L)
    {
        // moveEvent:register() 
        var MoveEvent = GetUserdata<MoveEvent>(L, 1);
        if (MoveEvent != null)
        {
            if (!MoveEvent.IsLoadedScriptId())
            {
                PushBoolean(L, false);
                return 1;
            }
            
            PushBoolean(L, _moveEvents.RegisterLuaEvent(MoveEvent));
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

    private static int LuaMoveEventOnCallback(LuaState L)
    {
        // moveevent:onEquip / deEquip / etc. (callback)
        var MoveEvent = GetUserdata<MoveEvent>(L, 1);
        if (MoveEvent != null)
        {
            if (!MoveEvent.LoadScriptId())
            {
                PushBoolean(L, false);
                return 1;
            }
            PushBoolean(L, true);
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

    public static int LuaMoveEventLevel(LuaState L)
    {
        // moveEvent:level(level)
        var moveEvent = GetUserdata<MoveEvent>(L, 1);
        if (moveEvent != null)
        {
            moveEvent.RequiredMinLevel = GetNumber<uint>(L, 2);
            moveEvent.WieldInfo = WieldInfoType.WIELDINFO_LEVEL;
            PushBoolean(L, true);
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

    public static int LuaMoveEventMagicLevel(LuaState L)
    {
        // moveEvent:magicLevel(magicLevel)
        var moveEvent = GetUserdata<MoveEvent>(L, 1);
        if (moveEvent != null)
        {
            moveEvent.RequiredMinMagicLevel = GetNumber<uint>(L, 2);
            moveEvent.WieldInfo = WieldInfoType.WIELDINFO_MAGLV;
            PushBoolean(L, true);
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

    public static int LuaMoveEventSlot(LuaState L)
    {
        // moveEvent:slot(slot)
        var moveEvent = GetUserdata<MoveEvent>(L, 1);
        if (moveEvent is null)
        {
           Lua.PushNil(L);
           return 1;
        }

        if (moveEvent.EventType == MoveEventType.MOVE_EVENT_EQUIP || moveEvent.EventType == MoveEventType.MOVE_EVENT_DEEQUIP)
        {
            var slotName = GetString(L, 2).ToLowerInvariant();
            if (slotName == "head")
            {
                moveEvent.Slot = Slot.Head;
            }
            else if (slotName == "necklace")
            {
                moveEvent.Slot = Slot.Necklace;
            }
            else if (slotName == "backpack")
            {
                moveEvent.Slot = Slot.Backpack;
            }
            else if (slotName == "armor" || slotName == "body")
            {
                moveEvent.Slot = Slot.Body;
            }
            else if (slotName == "right-hand" || slotName == "right")
            {
                moveEvent.Slot = Slot.Right;
            }
            else if (slotName == "left-hand" || slotName == "left")
            {
                moveEvent.Slot = Slot.Left;
            }
            else if (slotName == "hand" || slotName == "shield")
            {
                moveEvent.Slot = Slot.Hand;
            }
            else if (slotName == "legs")
            {
                moveEvent.Slot = Slot.Legs;
            }
            else if (slotName == "feet")
            {
                moveEvent.Slot = Slot.Feet;
            }
            else if (slotName == "ring")
            {
                moveEvent.Slot = Slot.Ring;
            }
            else if (slotName == "ammo")
            {
                moveEvent.Slot = Slot.Ammo;
            }
            else
            {
                _logger.Warning("[MoveEventFunctions::luaMoveEventSlot] - " +
                                "Unknown slot type: {}",
                                slotName);

                PushBoolean(L, false);
                return 1;
            }
        }

        PushBoolean(L, true);
        return 1;
    }

    public static int LuaMoveEventPremium(LuaState L)
    {
        // moveEvent:premium(bool)
        var moveEvent = GetUserdata<MoveEvent>(L, 1);
        if (moveEvent != null)
        {
            moveEvent.RequirePremium = GetBoolean(L, 2);
            moveEvent.WieldInfo = WieldInfoType.WIELDINFO_PREMIUM;
            PushBoolean(L, true);
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

    public static int LuaMoveEventVocation(LuaState L)
    {
        // moveEvent:vocation(vocName, showInDescription = false, lastVoc = false)
        var moveEvent = GetUserdata<MoveEvent>(L, 1);
        if (moveEvent != null)
        {
            var vocation = _vocationStore.GetByName(GetString(L, 2));
            moveEvent.RequiredVocations.Add(vocation.Id);
            moveEvent.WieldInfo = WieldInfoType.WIELDINFO_VOCREQ;

            bool showInDescription = false;
            bool lastVoc = false;

            if (GetBoolean(L, 3))
                showInDescription = GetBoolean(L, 3);

            if (GetBoolean(L, 4))
                lastVoc = GetBoolean(L, 4);

            if (showInDescription)
            {
                if (string.IsNullOrEmpty(moveEvent.VocationsDescription))
                {
                    moveEvent.VocationsDescription = GetString(L, 2).ToLowerInvariant();
                    moveEvent.VocationsDescription += "s";
                }
                else
                {
                    if (lastVoc)
                        moveEvent.VocationsDescription += " and ";
                    else
                        moveEvent.VocationsDescription += ", ";

                    moveEvent.VocationsDescription += GetString(L, 2).ToLowerInvariant();
                    moveEvent.VocationsDescription += "s";
                }
            }
            PushBoolean(L, true);
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

    public static int LuaMoveEventItemId(LuaState L)
    {
        // moveEvent:id(ids)
        var moveEvent = GetUserdata<MoveEvent>(L, 1);
        if (moveEvent != null)
        {
            var parameters = Lua.GetTop(L) - 1; // - 1 because self is a parameter aswell, which we want to skip ofc
            if (parameters > 1)
                for (var i = 0; i < parameters; ++i)
                    moveEvent.SetItemIdsVector(GetNumber<ushort>(L, 2 + i));
            else
                moveEvent.SetItemIdsVector(GetNumber<ushort>(L, 2));
            PushBoolean(L, true);
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

    public static int LuaMoveEventItemIdRange(LuaState L)
    {
        // moveEvent:id(fromId, toId)
        var moveEvent = GetUserdata<MoveEvent>(L, 1);
        var parameters = Lua.GetTop(L) - 1; // - 1 because self is a parameter aswell, which we want to skip ofc

        var fromId = GetNumber<ushort>(L, 2, 0);
        var toId = GetNumber<ushort>(L, 3, 0);

        if (moveEvent != null && parameters == 2 && fromId < toId)
        {
            for (var id = fromId; id <= toId; ++id)
                moveEvent.SetItemIdsVector(id);
          
            PushBoolean(L, true);
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

    public static int LuaMoveEventItemActionId(LuaState L)
    {
        // moveEvent:aid(ids)
        var moveEvent = GetUserdata<MoveEvent>(L, 1);
        if (moveEvent != null)
        {
            var parameters = Lua.GetTop(L) - 1; // - 1 because self is a parameter aswell, which we want to skip ofc
            if (parameters > 1)
                for (var i = 0; i < parameters; ++i)
                    moveEvent.SetActionIdsVector(GetNumber<ushort>(L, 2 + i));
            else
                moveEvent.SetActionIdsVector(GetNumber<ushort>(L, 2));
            PushBoolean(L, true);
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

    public static int LuaMoveEventItemUniqueId(LuaState L)
    {
        // moveEvent:uid(ids)
        var moveEvent = GetUserdata<MoveEvent>(L, 1);
        if (moveEvent != null)
        {
            var parameters = Lua.GetTop(L) - 1; // - 1 because self is a parameter aswell, which we want to skip ofc
            if (parameters > 1)
                for (var i = 0; i < parameters; ++i)
                    moveEvent.SetUniqueIdsVector(GetNumber<ushort>(L, 2 + i));
            else
                moveEvent.SetUniqueIdsVector(GetNumber<ushort>(L, 2));
            PushBoolean(L, true);
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

    public static int LuaMoveEventPosition(LuaState L)
    {
        // moveevent:position(positions)
        var moveEvent = GetUserdata<MoveEvent>(L, 1);
        if (moveEvent != null)
        {
            var parameters = Lua.GetTop(L) - 1; // - 1 because self is a parameter aswell, which we want to skip ofc
            if (parameters > 1)
                for (var i = 0; i < parameters; ++i)
                    moveEvent.SetPositions(GetPosition(L, 2 + i));
            else
                moveEvent.SetPositions(GetPosition(L, 2));
            PushBoolean(L, true);
        }
        else
        {
            Lua.PushNil(L);
        }
        return 1;
    }

}
