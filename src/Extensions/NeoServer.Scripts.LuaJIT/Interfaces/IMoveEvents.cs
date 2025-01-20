using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World.Tiles;
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
    void OnItemMove(IItem item, ITile tile, bool isAdd);
    void OnCreatureMove(ICreature creature, Location fromLocation, Location toLocation);

    #region Functions

    bool StepIn(ICreature creature, IItem item, Location fromLocation, Location toLocation);

    bool StepOut(ICreature creature, IItem item, Location fromLocation, Location toLocation);

    bool AddItem(IItem item, Location position, IItem tileitem = null);

    bool RemoveItem(IItem item, Location position, IItem tileitem = null);

    #endregion

}
