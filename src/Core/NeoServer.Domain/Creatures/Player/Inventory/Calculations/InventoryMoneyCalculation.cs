using NeoServer.Domain.Common.Contracts.DataStores;

namespace NeoServer.Domain.Creatures.Player.Inventory.Calculations;

public static class InventoryMoneyCalculation
{
    internal static ulong CalculateTotalMoney(this Inventory inventory, ICoinTypeStore coinTypeStore)
    {
        uint total = 0;

        var inventoryMap = inventory.BackpackSlot?.Map;
        var coinTypes = coinTypeStore.All;

        if (coinTypes is null) return total;
        if (inventoryMap is null) return total;

        foreach (var coinType in coinTypes)
        {
            if (coinType is null) continue;
            if (!inventoryMap.TryGetValue(coinType.ServerId, out var coinAmount)) continue;

            var worthMultiplier = coinType?.Attributes?.GetAttribute<uint>(ItemTypeAttribute.Worth) ?? 0;
            total += worthMultiplier * coinAmount;
        }

        return total;
    }
}