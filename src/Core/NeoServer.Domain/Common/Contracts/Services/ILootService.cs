using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Creatures.Monster.Loot;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface ILootService
{
    Loot GenerateLoot(IMonster monster, decimal lootRate = 0);
    Loot GenerateLoot(ICreature creature, decimal lootRate = 0);
    ILootContainer CreateLootContainer(ICreature deadCreature, IThing killer, decimal lootRate = 0);
}