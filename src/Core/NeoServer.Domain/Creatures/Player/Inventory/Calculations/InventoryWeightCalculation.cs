namespace NeoServer.Domain.Creatures.Player.Inventory.Calculations;

internal static class InventoryWeightCalculation
{
    internal static float CalculateTotalWeight(this InventoryMap inventoryMap)
    {
        var sum = 0F;
        foreach (var (item, _) in inventoryMap.Items) sum += item.Weight;
        return sum;
    }
}