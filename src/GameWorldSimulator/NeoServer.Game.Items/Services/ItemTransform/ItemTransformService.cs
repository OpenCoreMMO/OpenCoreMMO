using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Results;
using NeoServer.Game.Items.Services.ItemTransform.Operations;

namespace NeoServer.Game.Items.Services.ItemTransform;

public class ItemTransformService : IItemTransformService
{
    private readonly IItemFactory _itemFactory;
    private readonly IItemTypeStore _itemTypeStore;
    private readonly IMap _map;
    private readonly IMapService _mapService;
    private readonly IStaticToDynamicTileService _staticToDynamicTileService;

    public ItemTransformService(
        IItemFactory itemFactory,
        IMap map,
        IMapService mapService,
        IItemTypeStore itemTypeStore,
        IStaticToDynamicTileService staticToDynamicTileService)
    {
        _itemFactory = itemFactory;
        _map = map;
        _mapService = mapService;
        _itemTypeStore = itemTypeStore;
        _staticToDynamicTileService = staticToDynamicTileService;
    }

    public Result<IItem> Transform(IPlayer by, IItem fromItem, ushort toItem)
    {
        var createdItem = _itemFactory.Create(toItem, fromItem.Location, null);

        _itemTypeStore.TryGetValue(toItem, out var toItemType);

        var result = ReplaceItemFromGroundOperation.Execute(_map, _staticToDynamicTileService, _itemFactory, fromItem, toItemType);
        if (!result.IsNotApplicable) return result;

        result = ReplaceItemOnInventoryOperation.Execute(_itemFactory, fromItem, toItemType);
        if (!result.IsNotApplicable) return result;

        result = ReplaceGroundOperation.Execute(_map, _mapService, fromItem, createdItem);
        if (!result.IsNotApplicable) return result;

        result = ReplaceItemOnContainerOperation.Execute(by, fromItem, createdItem);
        if (!result.IsNotApplicable) return result;

        return Result<IItem>.Ok(null);
    }

    public Result<IItem> Transform(IItem fromItem, ushort toItem)
    {
        return Transform(null, fromItem, toItem);
    }
}