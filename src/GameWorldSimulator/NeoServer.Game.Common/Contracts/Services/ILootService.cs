using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Common.Contracts.Services;

public interface ILootService
{
    ILoot DropLoot(IMonster monster, decimal lootRate = 0);
    ILoot DropLoot(ICreature creature,  decimal lootRate = 0);
}