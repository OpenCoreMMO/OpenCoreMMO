using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Results;
using NeoServer.Game.Creatures.Player.Inventory.Rules;

namespace NeoServer.Game.Creatures.Player.Inventory.Operations;

public abstract class AddToSlotOperation
{
    public static Result<IItem> Add(Inventory inventory, Slot slot, IItem item)
    {
        var result = inventory.CanAddItem(slot, item, item.Amount);

        if (result.Failed) return Result<IItem>.Fail(result.Reason);

        if (SwapRule.ShouldSwap(inventory, item, slot)) return SwapOperation.SwapItem(inventory, slot, item);

        if (slot is Slot.Backpack) return AddToBackpackOperation.Add(inventory, item);

        if (item is ICumulative cumulative)
        {
            var addCumulativeResult = AddCumulativeItemOperation.Add(inventory, cumulative, slot);

            if (result.Failed) return Result<IItem>.Fail(addCumulativeResult.Reason);
        }

        inventory.InventoryMap.Add(slot, item, item.ClientId);

        item.SetNewLocation(Location.Inventory(slot));

        if (item is IDressable dressable) dressable.DressedIn(inventory.Owner);

        item.SetParent(inventory.Owner);

        return new Result<IItem>();
    }
}