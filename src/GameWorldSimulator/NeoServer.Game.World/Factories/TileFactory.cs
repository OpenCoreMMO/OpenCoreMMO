using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Location.Structs.Helpers;
using NeoServer.Game.World.Models.Tiles;

namespace NeoServer.Game.World.Factories;

public class TileFactory : ITileFactory
{
    private readonly IDictionary<string, IStaticTile> _tileCache = new Dictionary<string, IStaticTile>();

    public ITile CreateTile(Coordinate coordinate, TileFlag flag, IItem[] items, bool useCache = true)
    {
        var hash = GetTileHash(items);
        
        if (useCache && _tileCache.TryGetValue(hash, out var tile))
        {
            return tile;
        }
        
        var hasUnpassableItem = false;
        var hasMoveableItem = false;
        var hasTransformableItem = false;
        var hasHeight = false;
        IGround ground = null;

        var topItems = new List<IItem>();
        var downItems = new List<IItem>();

        foreach (var item in items)
        {
            if (item is null) continue;

            if (item.IsBlockeable) hasUnpassableItem = true;

            if (item.CanBeMoved) hasMoveableItem = true;

            if (item.IsTransformable) hasTransformableItem = true;

            if (item.Metadata.HasFlag(ItemFlag.HasHeight)) hasHeight = true;

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

        if (hasUnpassableItem &&
            !hasMoveableItem &&
            !hasTransformableItem && !hasHeight)
        {
            var staticTile = new StaticTile(new Coordinate(), items);
            _tileCache.TryAdd(hash, staticTile);
            return staticTile;
        }

        return new DynamicTile(coordinate, flag, ground, topItems.ToArray(), downItems.ToArray());
    }

    public ITile GetTileFromCache(Coordinate coordinate, ref Span<byte> clientIds)
    {
        var hash = GetTileHash(ref clientIds);

        return _tileCache.TryGetValue(hash, out var tile) ? tile : null;
    }

    public ITile CreateDynamicTile(Coordinate coordinate, TileFlag flag, IItem[] items)
    {
        IGround ground = null;

        var topItems = new List<IItem>();
        var downItems = new List<IItem>();

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

    private static string GetTileHash(IItem[] items)
    {
        Span<byte> raw = stackalloc byte[items.Length * sizeof(ushort)];
        var index = 0;
        foreach (var item in items)
        {
            if (item is null) continue;

            raw[index++] = (byte)(item.ClientId & 0xFF);
            raw[index++] = (byte)((item.ClientId >> 8) & 0xFF);
        }

        return HashHelper.ComputeContentHash(ref raw);
    }

    private static string GetTileHash(ref Span<byte> clientIds)
    {
        return HashHelper.ComputeContentHash(ref clientIds);
    }
}