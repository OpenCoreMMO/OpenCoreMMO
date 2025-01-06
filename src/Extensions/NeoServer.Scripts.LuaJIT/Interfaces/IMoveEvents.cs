using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Enums;

namespace NeoServer.Scripts.LuaJIT.Interfaces;

public interface IMoveEvents
{
    public void Clear();
    bool RegisterLuaItemEvent(MoveEvent moveEvent);
    bool RegisterLuaEvent(MoveEvent moveEvent);
    MoveEvent GetEvent(IItem item, MoveEventType eventType);
    MoveEvent GetEvent(Location location, MoveEventType eventType);
    void OnCreatureMove(ICreature creature, Location fromLocation, Location toLocation, MoveEventType eventType);
    void OnPlayerEquip(IPlayer player, IItem item, Slot slot, bool isCheck);
    void OnPlayerDeEquip(IPlayer player, IItem item, Slot slot, bool isCheck);
    void OnItemMove(IItem item, ITile tile, bool isAdd);

    #region Functions

    bool StepIn(ICreature creature, IItem item, Location fromLocation, Location toLocation);

    bool StepOut(ICreature creature, IItem item, Location fromLocation, Location toLocation);

    bool EquipItem(MoveEvent moveEvent, IPlayer player, IItem item, Slot onSlot, bool isCheck);

    bool DeEquipItem(MoveEvent moveEvent, IPlayer player, IItem item, Slot onSlot, bool isCheck);

    bool AddItem(IItem item, Location position, IItem tileitem = null);

    bool RemoveItem(IItem item, Location position, IItem tileitem = null);

    #endregion

}
