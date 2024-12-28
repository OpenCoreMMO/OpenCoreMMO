using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Containers;

namespace NeoServer.Game.Items.Items.Containers.Container.Queries;

internal static class FindItemByTypeQuery
{
    public static Stack<(IItem Item, byte Slot, byte Amount)> Search(List<IItem> items, IItemType itemToRemove,
        byte amount) //todo: slow method
    {
        sbyte slotIndex = -1;
        var slotsToRemove = new Stack<(IItem Item, byte Slot, byte Amount)>();

        foreach (var item in items)
        {
            SearchOnInnerContainer(itemToRemove, amount, item);

            slotIndex++;

            if (item.Metadata.ServerId != itemToRemove.ServerId) continue;
            if (amount == 0) break;

            slotsToRemove.Push((item, (byte)slotIndex, Math.Min(item.Amount, amount)));

            if (item.Amount > amount) break;

            amount -= item.Amount;
        }

        return slotsToRemove;
    }

    private static void SearchOnInnerContainer(IItemType itemToRemove, byte amount, IItem item)
    {
        if (item is not IContainer innerContainer) return;
        innerContainer.RemoveItem(itemToRemove, amount);
    }
}