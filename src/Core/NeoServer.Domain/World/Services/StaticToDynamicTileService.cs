using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.World.Services;

public class StaticToDynamicTileService(
    IItemClientServerIdMapStore itemClientServerIdMapStore,
    IItemFactory itemFactory,
    ITileFactory tileFactory,
    World world)
    : IStaticToDynamicTileService
{
    public ITile TransformIntoDynamicTile(ITile tile)
    {
        if (!tile.Location) return tile;
        if (tile is not IStaticTile staticTile) return tile;

        // Prefer cloning existing item instances so ActionId/UniqueId survive the conversion.
        // Fall back to ClientId rebuild only when AllItems is unavailable.
        var items = staticTile.AllItems is { Length: > 0 }
            ? CloneItems(staticTile.AllItems, tile.Location)
            : RecreateFromClientIds(staticTile, tile.Location);

        var dynamicTile = tileFactory.CreateDynamicTile(new Coordinate(tile.Location), TileFlag.None, items);

        world.ReplaceTile(dynamicTile);
        return dynamicTile;
    }

    private IItem[] CloneItems(IItem[] originals, Location location)
    {
        var items = new List<IItem>(originals.Length);

        foreach (var original in originals)
        {
            if (original is null) continue;

            var created = itemFactory.Create(original.ServerId, location,
                new Dictionary<ItemTypeAttribute, IConvertible>());
            if (created is null) continue;

            if (original.ActionId != 0)
            {
                created.Attributes.SetAttribute(ItemAttribute.ActionId, original.ActionId);
            }

            if (original.UniqueId != 0)
            {
                created.Attributes.SetAttribute(ItemAttribute.UniqueId, original.UniqueId);
            }

            items.Add(created);
        }

        return items.ToArray();
    }

    private IItem[] RecreateFromClientIds(IStaticTile staticTile, Location location)
    {
        var itemsId = staticTile.AllClientIdItems;
        var items = new List<IItem>(itemsId.Length);

        foreach (var clientId in itemsId)
        {
            if (!itemClientServerIdMapStore.TryGetValue(clientId, out var serverId)) continue;

            var item = itemFactory.Create(serverId, location, new Dictionary<ItemTypeAttribute, IConvertible>());
            if (item is not null)
            {
                items.Add(item);
            }
        }

        return items.ToArray();
    }
}
