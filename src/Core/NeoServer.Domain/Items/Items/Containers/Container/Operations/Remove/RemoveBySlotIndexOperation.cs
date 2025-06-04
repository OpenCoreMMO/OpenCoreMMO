using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Items.Items.Containers.Container.Operations.Update;

namespace NeoServer.Domain.Items.Items.Containers.Container.Operations.Remove;

internal static class RemoveBySlotIndexOperation
{
    public static IItem Remove(Container fromContainer, byte slotIndex, byte amount)
    {
        var result = RemoveItem(fromContainer, slotIndex, amount);

        if (result.Failed) return null;

        if (result.Operation is Operation.Updated) fromContainer.InvokeItemUpdatedEvent(slotIndex, (sbyte)-amount);

        if (result.Operation is Operation.Removed)
        {
            fromContainer.SlotsUsed--;
            ItemsLocationOperation.Update(fromContainer);
            fromContainer.InvokeItemRemovedEvent(slotIndex, result.Value, amount);
        }

        return result.Value;
    }

    private static OperationResult<IItem> RemoveItem(Container fromContainer, byte slotIndex, byte amount)
    {
        var item = fromContainer.Items[slotIndex];

        var amountToReduce = Math.Min(item.Amount, amount);

        if (item is ICumulative cumulative && amountToReduce != item.Amount) return ReduceItem(cumulative, amount);

        fromContainer.Items.RemoveAt(slotIndex);

        RemoveParentIfContainer(item);

        UnsubscribeIfCumulativeItem(fromContainer, item);

        item.OnItemRemoved(fromContainer.Owner);

        return OperationResult<IItem>.Ok(item, Operation.Removed);
    }

    private static OperationResult<IItem> ReduceItem(ICumulative cumulative, byte amount)
    {
        var itemSplit = cumulative.Split(amount);

        return OperationResult<IItem>.Ok(itemSplit, Operation.Updated);
    }

    private static void UnsubscribeIfCumulativeItem(Container fromContainer, IItem item)
    {
        if (item is not ICumulative cumulative) return;
        cumulative.OnReduced -= fromContainer.OnItemReduced;
    }

    private static void RemoveParentIfContainer(IItem item)
    {
        if (item is not IContainer container) return;
        container.SetParent(null);
    }
}