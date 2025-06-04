using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Containers;
using NeoServer.Domain.Common.Creatures.Players;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Creatures.Player.Inventory.Operations;

public static class AddToBackpackOperation
{
    public static Result<IItem> Add(Inventory inventory, IItem item)
    {
        if (!item.IsPickupable) return Result<IItem>.Fail(InvalidOperation.CannotDress);

        if (inventory.InventoryMap.GetItem<IContainer>(Slot.Backpack) is { } backpack)
            return new Result<IItem>(null, backpack.AddItem(item).Error);

        AddBackpackParent(inventory, item);

        inventory.InventoryMap.Add(Slot.Backpack, item, item.ClientId);

        item.SetNewLocation(Location.Inventory(Slot.Backpack));

        return Result<IItem>.Success;
    }

    private static void AddBackpackParent(Inventory inventory, IItem item)
    {
        if (item is not IContainer container) return;
        if (item is IContainer { IsPickupable: false }) return;

        container.SetParent(inventory.Owner);
        container.SubscribeToWeightChangeEvent(inventory.ContainerOnOnWeightChanged);
    }
}