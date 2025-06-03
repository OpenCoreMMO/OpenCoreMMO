using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;

namespace NeoServer.Domain.Items.Items.Containers.Container.Calculations;

internal static class PossibleAmountToAddCalculation
{
    public static uint Calculate(Container container, IItem item)
    {
        if (item is not ICumulative) return container.IsFull ? 0 : container.FreeSlotsCount;

        var possibleAmountToAdd = container.FreeSlotsCount * 100;

        foreach (var i in container.Items)
            if (i is ICumulative c && i.ClientId == item.ClientId)
                possibleAmountToAdd += c.AmountToComplete;

        return possibleAmountToAdd;
    }
}