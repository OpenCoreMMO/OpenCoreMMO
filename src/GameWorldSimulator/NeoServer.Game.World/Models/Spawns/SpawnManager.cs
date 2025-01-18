using System;
using System.Linq;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;

namespace NeoServer.Game.World.Models.Spawns;

public class SpawnManager
{
    private readonly ICreatureFactory _creatureFactory;
    private readonly ICreatureGameInstance _creatureGameInstance;
    private readonly IMap _map;
    private readonly World _world;

    public SpawnManager(World world, IMap map, ICreatureGameInstance creatureGameInstance,
        ICreatureFactory creatureFactory)
    {
        _world = world;

        _map = map;
        _creatureGameInstance = creatureGameInstance;
        _creatureFactory = creatureFactory;
    }

    public void Respawn()
    {
        foreach (var respawn in _creatureGameInstance.AllKilledMonsters())
        {
            var monster = respawn.Item1;
            var deathTime = respawn.Item2;

            if (monster.Spawn is not null)
            {
                var spawnTime = TimeSpan.FromSeconds(monster.Spawn.SpawnTime);

                if (DateTime.Now.TimeOfDay < deathTime + spawnTime) continue;
                if (_map.ArePlayersAround(monster.Location)) continue;
            }

            if (_creatureGameInstance.TryRemoveFromKilledMonsters(monster.CreatureId)) monster.Reborn(); // TODO: NTN - create RespawnMonsterUseCase
        }
    }

    public void StartSpawn()
    {
        var monsters = _world.Spawns
            .Where(x => x.Monsters is not null)
            .SelectMany(x => x.Monsters)
            .ToList();

        var npcs = _world.Spawns
            .Where(x => x.Npcs is not null)
            .SelectMany(x => x.Npcs)
            .ToList();

        foreach (var monsterToSpawn in monsters)
        {
            var monster = _creatureFactory.CreateMonster(monsterToSpawn.Name, monsterToSpawn.Spawn);  // TODO: NTN - create SpawnMonsterUseCase

            if (monster == null) continue;
            PlaceCreature(monsterToSpawn, monster);
        }

        foreach (var npcToSpawn in npcs)
        {
            var npc = _creatureFactory.CreateNpc(npcToSpawn.Name, npcToSpawn.Spawn);
            if (npc is null) continue;

            _creatureGameInstance.Add(npc);
            PlaceCreature(npcToSpawn, npc);
        }
    }

    private void PlaceCreature(ISpawn.ICreature monsterToSpawn, ICreature creature)
    {
        creature.SetNewLocation(monsterToSpawn.Spawn.Location);
        _map.PlaceCreature(creature);
    }
}