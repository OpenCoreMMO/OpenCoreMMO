using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Enums;

namespace NeoServer.Scripts.LuaJIT.Interfaces;

public interface IActions
{
    public bool RegisterLuaItemEvent(Action action);

    public bool RegisterLuaUniqueEvent(Action action);

    public bool RegisterLuaActionEvent(Action action);

    public bool RegisterLuaPositionEvent(Action action);

    public bool RegisterLuaEvent(Action action);

    public bool UseItem(IPlayer player, Location pos, byte index, IItem item, bool isHotkey);

    public bool UseItemEx(IPlayer player, Location fromPos, Location toPos, byte toStackPos, IItem item, bool isHotkey,
        ICreature creature = null);

    public ReturnValueType CanUse(IPlayer player, Location pos);

    public ReturnValueType CanUse(IPlayer player, Location pos, IItem item);

    public ReturnValueType CanUseFar(ICreature creature, Location toPos, bool checkLineOfSight, bool checkFloor);

    public Action GetAction(IItem item);

    public ReturnValueType InternalUseItem(IPlayer player, Location pos, byte index, IItem item, bool isHotkey);

    public void ShowUseHotkeyMessage(IPlayer player, IItem item, uint count);

    public bool HasPosition(Location position);

    public Dictionary<Location, Action> GetPositionsMap();

    public void SetPosition(Location position, Action action);

    public bool HasItemId(ushort itemId);

    public void SetItemId(ushort itemId, Action action);

    public bool HasUniqueId(uint uniqueId);

    public void SetUniqueId(uint uniqueId, Action action);

    public bool HasActionId(ushort actionId);

    public void SetActionId(ushort actionId, Action action);

    public void Clear();

    public Action GetAction(ushort id);
}