using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Location.Structs.Helpers;
using NeoServer.Domain.World.Models.Tiles;
using Serilog;

namespace NeoServer.Domain.World.Factories;

public class TileFactory(ILogger logger) : ITileFactory
{
    private readonly Dictionary<(ulong Low, ulong High), IStaticTile> _tileCache = new();

    public ITile CreateTile(Coordinate coordinate, TileFlag flag, IItem[] items, bool useCache = true,
        uint? houseId = null)
    {
        var isHouseTile = houseId is > 0;

        // House tiles must never be cached as StaticTile — they are always DynamicTile.
        if (isHouseTile) useCache = false;

        (ulong Low, ulong High) tileHash = default;
        if (useCache)
        {
            tileHash = GetTileHash(items);
            if (_tileCache.TryGetValue(tileHash, out var tile)) return tile;
        }

        var hasUnpassableItem = false;
        var hasMoveableItem = false;
        var hasTransformableItem = false;
        var hasHeight = false;
        var hasTrashHolder = false;
        var hasDoor = false;
        var hasMapAttributes = false;
        IGround ground = null;

        var topItems = new List<IItem>(items.Length);
        var downItems = new List<IItem>(items.Length);

        foreach (var item in items)
        {
            if (item is null) continue;

            if (item.IsBlockeable) hasUnpassableItem = true;

            if (item.CanBeMoved && isHouseTile)
            {
                logger.Warning("Item {ItemClientId} is moveable and is on a house tile. This is not allowed",
                    item.ClientId);
                continue;
            }

            if (item.CanBeMoved && !isHouseTile) hasMoveableItem = true;

            if (item.IsTransformable) hasTransformableItem = true;

            if (item.IsDoor) hasDoor = true;

            // ActionId/UniqueId are per-instance; static tile cache keys only ClientIds, so these
            // must stay on DynamicTile or map attrs would be shared across tiles.
            if (item.ActionId != 0 || item.UniqueId != 0) hasMapAttributes = true;

            if (item.Metadata.HasFlag(ItemFlag.HasHeight)) hasHeight = true;

            if (item.IsAlwaysOnTop)
            {
                topItems.Add(item);
                continue;
            }

            if (item.Metadata.IsTrashHolder())
            {
                hasTrashHolder = true;
            }

            if (item is IGround groundItem)
            {
                ground = groundItem;
                continue;
            }

            downItems.Add(item);
        }

        if (!isHouseTile &&
            hasUnpassableItem &&
            !hasMoveableItem &&
            !hasTransformableItem &&
            !hasHeight &&
            !hasTrashHolder &&
            !hasDoor &&
            !hasMapAttributes)
        {
            var staticTile = new StaticTile(new Coordinate(), (uint)flag, items);

            if (useCache) _tileCache.TryAdd(tileHash, staticTile);

            return staticTile;
        }

        return new DynamicTile(coordinate, flag, ground, topItems.ToArray(), downItems.ToArray(), houseId);
    }

    public ITile GetTileFromCache(Coordinate coordinate, ref Span<byte> clientIds)
    {
        var hash = GetTileHash(ref clientIds);

        return _tileCache.GetValueOrDefault(hash);
    }

    public ITile CreateDynamicTile(Coordinate coordinate, TileFlag flag, IItem[] items)
    {
        IGround ground = null;

        var topItems = new List<IItem>(items.Length);
        var downItems = new List<IItem>(items.Length);

        foreach (var item in items)
        {
            if (item is null) continue;

            if (item.IsAlwaysOnTop)
            {
                topItems.Add(item);
                continue;
            }

            if (item is IGround groundItem)
            {
                ground = groundItem;
                continue;
            }

            downItems.Add(item);
        }

        return new DynamicTile(coordinate, flag, ground, topItems.ToArray(), downItems.ToArray());
    }

    private static (ulong Low, ulong High) GetTileHash(IItem[] items)
    {
        Span<byte> raw = stackalloc byte[items.Length * sizeof(ushort)];
        var index = 0;
        foreach (var item in items)
        {
            if (item is null) continue;

            raw[index++] = (byte)(item.ClientId & 0xFF);
            raw[index++] = (byte)((item.ClientId >> 8) & 0xFF);
        }

        var written = raw[..index];
        return HashHelper.ComputeContentHash(ref written);
    }

    private static (ulong Low, ulong High) GetTileHash(ref Span<byte> clientIds)
    {
        return HashHelper.ComputeContentHash(ref clientIds);
    }
}