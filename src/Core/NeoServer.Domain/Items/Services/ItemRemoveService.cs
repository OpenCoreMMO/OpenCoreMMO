using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;

namespace NeoServer.Domain.Items.Services;

// This class provides functionality to remove an item from the world,
// either from a player's inventory or from the ground or a container.
public class ItemRemoveService : IItemRemoveService
{
    private readonly IMap _map;

    public ItemRemoveService(IMap map)
    {
        _map = map;
    }

    public void Remove(IItem item)
    {
        if (RemoveFromMap(item)) return;
        if (RemoveFromContainer(item)) return;

        RemoveFromInventory(item);
    }

    private static bool RemoveFromContainer(IItem item)
    {
        if (item.Parent is not IContainer container) return false;
        container.RemoveItem(item, item.Amount);
        return true;
    }

    private bool RemoveFromMap(IItem item)
    {
        if (item.Location.Type is not LocationType.Ground) return false;

        var tile = _map[item.Location];
        if (tile is not IDynamicTile fromTile) return true;

        fromTile.RemoveItem(item);

        return true;
    }

    private void RemoveFromInventory(IItem item)
    {
        if (item.Location.Type is not LocationType.Slot) return;

        if (item.Parent is not IPlayer player) return;

        player.Inventory.RemoveItem(item, item.Amount, (byte)item.Location.Slot, out _);
    }
}