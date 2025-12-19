using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
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

        var itemsId = staticTile.AllClientIdItems;

        var items = new List<IItem>(itemsId.Length);

        foreach (var clientId in itemsId)
        {
            if (!itemClientServerIdMapStore.TryGetValue(clientId, out var serverId)) continue;

            var item = itemFactory.Create(serverId, tile.Location, new Dictionary<ItemTypeAttribute, IConvertible>());
            items.Add(item);
        }

        var dynamicTile = tileFactory.CreateDynamicTile(new Coordinate(tile.Location), TileFlag.None, items.ToArray());

        world.ReplaceTile(dynamicTile);
        return dynamicTile;
    }
}