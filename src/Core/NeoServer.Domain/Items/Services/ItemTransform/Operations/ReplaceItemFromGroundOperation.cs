using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Items.Services.ItemTransform.Operations;

internal static class ReplaceItemFromGroundOperation
{
    public static Result<IItem> Execute(
        IMap map,
        IStaticToDynamicTileService staticToDynamicTileService,
        IItemFactory itemFactory,
        IItem fromItem,
        IItemType toItemType)
    {
        if (fromItem.Location.Type != LocationType.Ground) return Result<IItem>.NotApplicable;

        IDynamicTile tile;

        if (map[fromItem.Location] is IStaticTile staticTile)
        {
            var clonedTile = staticTile.CreateClone(fromItem.Location);
            tile = staticToDynamicTileService.TransformIntoDynamicTile(clonedTile) as IDynamicTile;

            // Static→dynamic recreates item instances. Resolve the matching item on the new tile
            // and keep ActionId/UniqueId from the original (map attrs are lost on ClientId rebuild).
            fromItem = ResolveItemOnTile(tile, fromItem) ?? fromItem;
        }
        else
        {
            tile = map[fromItem.Location] as IDynamicTile;
        }

        if (tile is null) return Result<IItem>.NotApplicable;
        if (fromItem is IGround) return Result<IItem>.NotApplicable;
        if (toItemType is null) fromItem.MarkAsDeleted();

        var result = tile.UpdateItemType(fromItem, toItemType);
        if (result) return Result<IItem>.Ok(fromItem);

        var createdItem = toItemType is null ? null : itemFactory.Create(toItemType, fromItem.Location);
        if (createdItem is not null)
        {
            CopyMapAttributes(fromItem, createdItem);
        }

        tile.ReplaceItem(fromItem, createdItem);
        return Result<IItem>.Ok(createdItem);
    }

    private static IItem ResolveItemOnTile(IDynamicTile tile, IItem original)
    {
        if (tile?.AllItems is null) return null;

        foreach (var item in tile.AllItems)
        {
            if (item is null) continue;
            if (ReferenceEquals(item, original)) return item;
        }

        foreach (var item in tile.AllItems)
        {
            if (item is null) continue;
            if (item.ServerId != original.ServerId) continue;

            CopyMapAttributes(original, item);
            return item;
        }

        return null;
    }

    private static void CopyMapAttributes(IItem from, IItem to)
    {
        if (from.ActionId != 0 && to.ActionId == 0)
        {
            to.Attributes.SetAttribute(ItemAttribute.ActionId, from.ActionId);
        }

        if (from.UniqueId != 0 && to.UniqueId == 0)
        {
            to.Attributes.SetAttribute(ItemAttribute.UniqueId, from.UniqueId);
        }
    }
}
