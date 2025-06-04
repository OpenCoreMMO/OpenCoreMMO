using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Containers;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Items.Services.ItemTransform.Operations;

internal static class ReplaceItemOnContainerOperation
{
    public static Result<IItem> Execute(IPlayer by, IItem fromItem, IItem createdItem)
    {
        if (fromItem.Location.Type != LocationType.Container) return Result<IItem>.NotApplicable;

        var container = by?.Containers[fromItem.Location.ContainerId];

        container ??= fromItem.CanBeMoved && fromItem.Owner is IContainer owner ? owner : null;

        if (container is null) return Result<IItem>.NotApplicable;

        container.RemoveItem(fromItem, fromItem.Amount);

        if (createdItem is null) return Result<IItem>.Ok(null);

        var result = container.AddItem(createdItem, true);

        if (result.Succeeded) return Result<IItem>.Ok(createdItem);

        var tileResult = by?.Tile?.AddItem(createdItem);

        if (!tileResult.HasValue) return Result<IItem>.Ok(null);

        return tileResult.Value.Succeeded
            ? Result<IItem>.Ok(createdItem)
            : Result<IItem>.Fail(tileResult.Value.Error);
    }
}