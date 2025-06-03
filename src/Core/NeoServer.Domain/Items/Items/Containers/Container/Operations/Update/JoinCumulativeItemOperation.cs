using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Items.Items.Containers.Container.Operations.Add;

namespace NeoServer.Domain.Items.Items.Containers.Container.Operations.Update;

internal static class JoinCumulativeItemOperation
{
    public static Result Join(Container container, ICumulative item, byte itemToJoinSlot)
    {
        if (item is null) return Result.NotPossible;

        var amountToAdd = item.Amount;

        if (container.Items[itemToJoinSlot] is not ICumulative itemToUpdate) return Result.Fail(InvalidOperation.None);

        if (itemToUpdate.Amount == 100) return AddItemToFrontOperation.Add(container, item);

        item = JoinItem(container, item, itemToJoinSlot, itemToUpdate, amountToAdd);

        return AddRemainingItem(container, item);
    }

    private static Result AddRemainingItem(Container container, ICumulative item)
    {
        if (item != null) //item was joined on first item and remains with an amount
            return AddItemToFrontOperation.Add(container, item);

        return Result.Success;
    }

    private static ICumulative JoinItem(Container container, ICumulative item, byte itemToJoinSlot,
        ICumulative itemToUpdate, byte amountToAdd)
    {
        itemToUpdate.TryJoin(ref item);

        var amountUpdated = (sbyte)(amountToAdd - (item?.Amount ?? 0));
        container.InvokeItemUpdatedEvent(itemToJoinSlot, amountUpdated); //send update notification of the source item
        return item;
    }
}