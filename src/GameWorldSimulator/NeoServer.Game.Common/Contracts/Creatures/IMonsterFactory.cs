using NeoServer.Game.Common.Contracts.World;

namespace NeoServer.Game.Common.Contracts.Creatures;

public interface IMonsterFactory
{
    IMonster Create(string name, ISpawnPoint spawn = null);
    IMonster CreateSummon(string name, ICreature master);
}