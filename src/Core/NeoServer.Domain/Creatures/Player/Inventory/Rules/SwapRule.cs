using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;

namespace NeoServer.Domain.Creatures.Player.Inventory.Rules;

public abstract class SwapRule
{
    public static bool ShouldSwap(Inventory inventory, IItem itemToAdd, Slot slotDestination)
    {
        if (slotDestination == Slot.Backpack) return false;

        if (inventory.InventoryMap.GetItem<IItem>(slotDestination) is not { } itemOnSlot) return false;

        if (itemToAdd is ICumulative cumulative && itemOnSlot.ClientId == cumulative.ClientId &&
            itemOnSlot.Amount + itemToAdd.Amount <= 100)
            //will join
            return false;

        return true;
    }
}