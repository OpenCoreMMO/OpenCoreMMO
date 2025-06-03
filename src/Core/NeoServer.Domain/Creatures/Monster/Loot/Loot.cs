using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Monster.Loot;

public record Loot(ILootItem[] Items, HashSet<ICreature> Owners = null) : ILoot;