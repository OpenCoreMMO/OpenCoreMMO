using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Creatures.Monster.Loot;

public record LootItem(IItemType ItemType, byte Amount, uint Chance, LootItem[] Items);