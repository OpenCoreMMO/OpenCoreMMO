using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Common.Contracts.Creatures;

public interface ILootItem
{
    public IItemType ItemType { get; }
    byte Amount { get; }
    uint Chance { get; }
    ILootItem[] Items { get; }
}