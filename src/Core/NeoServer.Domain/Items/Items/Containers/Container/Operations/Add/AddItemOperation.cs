using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Items.Items.Containers.Container.Operations.Update;
using NeoServer.Domain.Items.Items.Containers.Container.Queries;
using NeoServer.Domain.Items.Items.Containers.Container.Rules;

namespace NeoServer.Domain.Items.Items.Containers.Container.Operations.Add;

internal static class AddItemOperation
{
    public static Result TryAddItem(Container toContainer, IItem item, byte? position = null)
    {
        if (item is null) return Result.NotPossible;

        if (position.HasValue && toContainer.Capacity <= position) position = null;

        var validation = CanAddItemToContainerRule.CanAdd(toContainer, item, position);
        if (!validation.Succeeded) return validation;

        position ??= toContainer.LastFreeSlot;

        return AddItem(toContainer, item, position);
    }

    public static void AddChildren(Container container, IEnumerable<IItem> children)
    {
        if (children is null) return;

        foreach (var item in children.Reverse()) TryAddItem(container, item);
    }

    private static Result AddItem(Container toContainer, IItem item, byte? position)
    {
        var result = toContainer.GetContainerAt(position.Value, out var container)
            ? container.AddItem(item).ResultValue
            : AddItem(toContainer, item, position.Value);

        item.SetParent(container ?? toContainer);

        return result;
    }

    private static Result AddItem(Container toContainer, IItem item, byte position)
    {
        if (item is null) return Result.NotPossible;
        if (toContainer.Capacity <= position) throw new ArgumentOutOfRangeException(nameof(toContainer));

        if (item is not ICumulative cumulativeItem) return AddItemToFrontOperation.Add(toContainer, item);

        var itemToJoinSlot = FindSlotOfFirstItemNotFullyQuery.Find(toContainer, cumulativeItem);

        if (itemToJoinSlot >= 0 && cumulativeItem is { } cumulative)
            return JoinCumulativeItemOperation.Join(toContainer, cumulative, (byte)itemToJoinSlot);

        return AddItemToFrontOperation.Add(toContainer, cumulativeItem);
    }
}