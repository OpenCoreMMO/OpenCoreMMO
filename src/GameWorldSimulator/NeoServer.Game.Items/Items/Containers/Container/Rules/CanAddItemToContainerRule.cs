using System;
using System.Collections.Generic;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Contracts.Items.Types.Containers;
using NeoServer.Game.Common.Results;
using NeoServer.Game.Items.Items.Containers.Container.Queries;

namespace NeoServer.Game.Items.Items.Containers.Container.Rules;

public static class CanAddItemToContainerRule
{
    public static Result<uint> CanAdd(IContainer toContainer, IItemType itemType)
    {
        if (!ICumulative.IsApplicable(itemType) && toContainer.TotalOfFreeSlots > 0)
            return Result<uint>.Ok(toContainer.TotalOfFreeSlots);

        var containers = new Queue<IContainer>();

        containers.Enqueue(toContainer);

        var amountPossibleToAdd = CalculateAmountPossibleToAdd(toContainer, itemType);

        return amountPossibleToAdd > 0
            ? Result<uint>.Ok(amountPossibleToAdd)
            : Result<uint>.Fail(InvalidOperation.NotEnoughRoom);
    }

    public static Result CanAdd(IContainer toContainer, IItem item, byte? slot = null)
    {
        if (item == toContainer) return Result.Fail(InvalidOperation.Impossible);

        if (slot is null && item is not ICumulative && toContainer.IsFull) return Result.Fail(InvalidOperation.IsFull);

        if (slot is not null && toContainer.GetContainerAt(slot.Value, out var container))
            return container.CanAddItem(item, slot: slot);

        if (item is ICumulative cumulative && toContainer.IsFull &&
            FindSlotOfFirstItemNotFullyQuery.Find(toContainer, cumulative) == -1)
            return Result.Fail(InvalidOperation.IsFull);

        return Result.Success;
    }

    private static uint CalculateAmountPossibleToAdd(IContainer toContainer, IItemType itemType)
    {
        var amountPossibleToAdd = toContainer.TotalOfFreeSlots * 100;

        if (!toContainer.Map.TryGetValue(itemType.ServerId, out var totalAmount)) return amountPossibleToAdd;

        var next100 = Math.Ceiling((decimal)totalAmount / 100) * 100;
        amountPossibleToAdd += (uint)(next100 - totalAmount);

        return amountPossibleToAdd;
    }
}