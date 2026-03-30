using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Items.Services.ItemTransform.Operations;

namespace NeoServer.Domain.Items.Services.ItemTransform;

public class ItemTransformService(
    IItemFactory itemFactory,
    IMap map,
    IItemTypeStore itemTypeStore,
    IStaticToDynamicTileService staticToDynamicTileService,
    ReplaceGroundOperation replaceGroundOperation)
    : IItemTransformService
{
    public Result<IItem> Transform(IPlayer by, IItem fromItem, ushort toItem)
    {
        itemTypeStore.TryGetValue(toItem, out var toItemType);

        Result<IItem> result;

        switch (fromItem.Location.Type)
        {
            case LocationType.Container:
                result = ReplaceItemOnContainerOperation.Execute(by, itemFactory, fromItem, toItemType);
                if (!result.IsNotApplicable) return result;
                break;
            case LocationType.Slot:
                result = ReplaceItemOnInventoryOperation.Execute(itemFactory, fromItem, toItemType);
                if (!result.IsNotApplicable) return result;
                break;
            case LocationType.Ground:
                result =
                    ReplaceItemFromGroundOperation.Execute(map, staticToDynamicTileService, itemFactory, fromItem,
                        toItemType);
                if (!result.IsNotApplicable) return result;

                var createdItem = itemFactory.Create(toItem, fromItem.Location, null);
                result = replaceGroundOperation.Execute(fromItem, createdItem);
                if (!result.IsNotApplicable) return result;
                break;
        }

        return Result<IItem>.Ok(null);
    }

    public Result<IItem> Transform(IItem fromItem, ushort toItem)
    {
        return Transform(null, fromItem, toItem);
    }
}