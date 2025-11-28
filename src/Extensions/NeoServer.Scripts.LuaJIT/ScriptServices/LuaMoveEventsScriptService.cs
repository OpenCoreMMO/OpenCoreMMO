using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Scripts.Services;

namespace NeoServer.Scripts.LuaJIT.ScriptServices;

public class LuaMoveEventsScriptService(IMoveEvents moveEvents) : IMoveEventsScriptService
{
    #region Public Methods

    public void ItemMove(IItem item, ITile tile, bool isAdd)
    {
        moveEvents.OnItemMove(item, tile, isAdd);
    }

    public bool? EquipItem(IPlayer player, IItem item, Slot slot, bool isChecks)
    {
        return moveEvents.OnEquipItem(player, item, slot, isChecks);
    }

    public bool? DeEquipItem(IPlayer player, IItem item, Slot slot, bool isChecks)
    {
        return moveEvents.OnDeEquipItem(player, item, slot, isChecks);
    }

    #endregion
}