using System.Collections.Generic;

namespace NeoServer.Game.Common.Contracts.Creatures;

public interface ILoot
{
    ILootItem[] Items { get; }
    HashSet<ICreature> Owners { get; }
}