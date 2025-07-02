using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Creatures.Player.Inventory.Operations;

public static class SwapOperation
{
    public static Result<IItem> SwapItem(Inventory inventory, Slot slot, IItem newItem)
    {
        var (existingItem, _) = inventory.InventoryMap.GetItem(slot);

        if (TryJoinCumulativeItem(newItem, existingItem, out var swappedItem))
            return Result<IItem>.Ok(swappedItem);

        int totalAmount = newItem.Amount + existingItem.Amount;
        byte amountToRemove = totalAmount > 100
            ? (byte)(totalAmount - 100)
            : (byte)0;

        if (newItem.ClientId != existingItem.ClientId)
        {
            amountToRemove = existingItem.Amount;
        }

        var result = RemoveFromSlotOperation.Remove(inventory, slot, amountToRemove);

        if (result.Failed) return result;

        var removedItem = result.Value;

        var addResult = AddToSlotOperation.Add(inventory, slot, newItem);

        return addResult.Failed ? result : Result<IItem>.Ok(removedItem);
    }

    private static bool TryJoinCumulativeItem(IItem newItem, IItem existingItem,
        out ICumulative swappedItem)
    {
        swappedItem = null;

        if (existingItem.ClientId != newItem.ClientId) return false;
        if (existingItem is not ICumulative existingCumulativeItem) return false;
        if (newItem is not ICumulative newCumulativeItem) return false;

        if (newItem.Amount + existingItem.Amount <= 100) return false;

        existingCumulativeItem.TryJoin(ref newCumulativeItem);

        if (newCumulativeItem?.Amount > 0) swappedItem = newCumulativeItem;

        return true;
    }
}