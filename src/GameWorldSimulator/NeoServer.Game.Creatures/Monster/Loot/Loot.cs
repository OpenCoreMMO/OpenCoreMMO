using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Creatures.Monster.Loot;

public record Loot(ILootItem[] Items, HashSet<ICreature> Owners = null) : ILoot;