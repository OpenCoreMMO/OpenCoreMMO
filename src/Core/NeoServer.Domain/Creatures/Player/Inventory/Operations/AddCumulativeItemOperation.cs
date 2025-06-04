using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Creatures.Players;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Creatures.Player.Inventory.Operations;

public static class AddCumulativeItemOperation
{
    public static Result Add(Inventory inventory, ICumulative cumulative, Slot slot)
    {
        if (inventory.InventoryMap.GetItem<ICumulative>(slot) is { } item)
            return item.TryJoin(ref cumulative) ? Result.Success : Result.Fail(InvalidOperation.NotEnoughRoom);

        inventory.InventoryMap.Add(slot, cumulative, cumulative.ClientId);

        cumulative.OnReduced += (itemReduced, amount) => inventory.OnItemReduced(itemReduced, slot, amount);

        return Result.Success;
    }
}