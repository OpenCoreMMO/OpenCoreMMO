using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Items.Items.Containers.Container.Queries;

namespace NeoServer.Domain.Items.Items.Containers.Container.Operations.Remove;

internal static class RemoveByItemTypeOperation
{
    public static void Remove(Container fromContainer, IItemType itemToRemove, byte amount) //todo: slow method
    {
        var slotsToRemove = FindItemByTypeQuery.Search(fromContainer.Items, itemToRemove, amount);

        while (slotsToRemove.TryPop(out var slot))
        {
            var (_, slotIndexToRemove, amountToRemove) = slot;

            fromContainer.RemoveItem(slotIndexToRemove, amountToRemove, out var itemRemoved);
        }
    }
}