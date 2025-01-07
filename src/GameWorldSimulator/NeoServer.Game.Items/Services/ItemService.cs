using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Game.Items.Services;

public class ItemService : IItemService
{
    private readonly IItemFactory _itemFactory;
    private readonly IMap _map;
    private readonly IStaticToDynamicTileService _staticToDynamicTileService;

    public ItemService(IItemFactory itemFactory,
        IStaticToDynamicTileService staticToDynamicTileService,
        IMap map)
    {
        _itemFactory = itemFactory;

        _staticToDynamicTileService = staticToDynamicTileService;
        _map = map;
    }

    public IItem Transform(ITile tile, ushort fromItemId, ushort toItemId)
    { 
        if (tile is null) return null;

        tile = _staticToDynamicTileService.TransformIntoDynamicTile(tile);

        if (tile is not IDynamicTile dynamicTile) return null;

        var newItem = _itemFactory.Create(toItemId, tile.Location, new Dictionary<ItemAttribute, IConvertible>());

        dynamicTile.ReplaceItem(fromItemId, newItem);

        return newItem;
    }

    public IItem Transform(Location location, ushort fromItemId, ushort toItemId)
    {
        var tile = _map.GetTile(location);
        if (tile is null) return null;

        tile = tile is IStaticTile staticTile ? staticTile.CreateClone(location) : tile;
        
        return Transform(tile, fromItemId, toItemId);
    }

    public IItem Create(Location location, ushort id)
    {
        var tile = _map.GetTile(location);

        if (tile is null) return null;

        tile = tile is IStaticTile staticTile ? staticTile.CreateClone(location) : tile;

        tile = _staticToDynamicTileService.TransformIntoDynamicTile(tile);

        if (tile is not IDynamicTile dynamicTile) return null;

        var newItem = _itemFactory.Create(id, tile.Location, new Dictionary<ItemAttribute, IConvertible>());

        dynamicTile.AddItem(newItem);

        return newItem;
    }
}