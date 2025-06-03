using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Common.Contracts.Services;

public interface ILootService
{
    ILoot GenerateLoot(IMonster monster, decimal lootRate = 0);
    ILoot GenerateLoot(ICreature creature, decimal lootRate = 0);
    ILootContainer CreateLootContainer(ICreature deadCreature, decimal lootRate = 0);
}