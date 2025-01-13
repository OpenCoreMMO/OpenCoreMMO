using System;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Game.Creatures.Monster.Loot;

public record LootItem(IItemType ItemType, byte Amount, uint Chance, ILootItem[] Items) : ILootItem;