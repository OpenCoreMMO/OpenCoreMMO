using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Scripts.Services;

namespace NeoServer.Scripts.LuaJIT.ScriptServices;

public class LuaMoveEventsScriptService(IMoveEvents moveEvents) : IMoveEventsScriptService
{
    #region Public Methods

    public void CreatureMove(ICreature creature, Location fromLocation, Location toLocation)
        => moveEvents.OnCreatureMove(creature, fromLocation, toLocation);
    
    public void ItemMove(IItem item, ITile tile, bool isAdd)
        => moveEvents.OnItemMove(item, tile, isAdd);
    
    public bool? EquipItem(IPlayer player, IItem item, Slot slot, bool isChecks)
        => moveEvents.OnEquipItem(player, item, slot, isChecks);

    public bool? DeEquipItem(IPlayer player, IItem item, Slot slot, bool isChecks)
        => moveEvents.OnDeEquipItem(player, item, slot, isChecks);

    #endregion
}