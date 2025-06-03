using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.Items.Types.Containers;
using NeoServer.Domain.Common.Creatures.Players;

namespace NeoServer.Domain.Creatures.Player.Inventory.Calculations;

internal static class PossibleAmountToAddCalculation
{
    public static uint Calculate(Inventory inventory, IItem item, byte? toPosition = null)
    {
        if (toPosition is null) return 0;

        var slot = (Slot)toPosition;

        if (slot == Slot.Backpack)
        {
            if (inventory[slot] is null) return 1;
            if (inventory[slot] is IContainer container) return container.PossibleAmountToAdd(item);
        }

        if (slot != Slot.Left && slot != Slot.Ammo) return 1;

        if (item is not ICumulative) return 1;
        if (item is ICumulative c1 && inventory[slot] is { } i && c1.ClientId != i.ClientId) return 100;
        if (inventory[slot] is null) return 100;

        return (uint)(100 - inventory[slot].Amount);
    }
}