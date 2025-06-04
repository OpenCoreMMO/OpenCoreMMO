using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Monster.Loot;

public record Loot(LootItem[] Items, HashSet<ICreature> Owners = null);