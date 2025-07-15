using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Creatures.Player.Inventory.Rules;

public static class AddToBackpackSlotRule
{
    internal static Result CanAddToBackpackSlot(this Inventory inventory, IItem item)
    {
        if (item is IContainer container &&
            container.IsPickupable &&
            !inventory.InventoryMap.HasItemOnSlot(Slot.Backpack) &&
            item.Metadata.Attributes.GetAttribute(ItemTypeAttribute.BodyPosition) == "backpack")
            return Result.Success;

        return inventory.InventoryMap.HasItemOnSlot(Slot.Backpack)
            ? Result.Success
            : Result.Fail(InvalidOperation.CannotDress);
    }
}