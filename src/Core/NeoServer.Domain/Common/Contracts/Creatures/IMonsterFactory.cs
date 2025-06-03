using NeoServer.Domain.Common.Contracts.World;

namespace NeoServer.Domain.Common.Contracts.Creatures;

public interface IMonsterFactory
{
    IMonster Create(string name, ISpawnPoint spawn = null);
    IMonster CreateSummon(string name, ICreature master);
}