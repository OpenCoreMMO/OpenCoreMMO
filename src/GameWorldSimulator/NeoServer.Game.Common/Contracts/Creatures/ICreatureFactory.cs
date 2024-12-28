using NeoServer.Game.Common.Contracts.World;

namespace NeoServer.Game.Common.Contracts.Creatures;

public delegate void CreatureCreated(ICreature creature);

public interface ICreatureFactory
{
    IMonster CreateMonster(string name, ISpawnPoint spawn = null);
    INpc CreateNpc(string name, ISpawnPoint spawn = null);
    IPlayer CreatePlayer(IPlayer playerModel);
    IMonster CreateSummon(string name, ICreature master);
    event CreatureCreated OnCreatureCreated;
}