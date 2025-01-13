using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Helpers;

namespace NeoServer.Game.Creatures.Monster.Loot;

public record Loot(ILootItem[] Items, HashSet<ICreature> Owners = null) : ILoot;