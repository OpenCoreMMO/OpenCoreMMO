using System;
using System.Linq;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;

namespace NeoServer.Game.World.Models.Spawns;

public class SpawnManager(
    World world,
    IMap map,
    ICreatureGameInstance creatureGameInstance,
    ICreatureFactory creatureFactory)
{
    public void Respawn()
    {
        foreach (var (monster, deathTime) in creatureGameInstance.AllKilledMonsters())
        {
            if (monster.Spawn is not null)
            {
                var spawnTime = TimeSpan.FromSeconds(monster.Spawn.SpawnTime);

                if (DateTime.Now.TimeOfDay < deathTime + spawnTime) continue;
                if (map.ArePlayersAround(monster.Location)) continue;
            }

            if (!creatureGameInstance.TryRemoveFromKilledMonsters(monster.CreatureId)) continue;
            monster.Reborn();
            creatureGameInstance.Add(monster);
            map.PlaceCreature(monster);
            // TODO: NTN - create RespawnMonsterUseCase?
        }
    }

    public void StartSpawn()
    {
        var monsters = world.Spawns
            .Where(x => x.Monsters is not null)
            .SelectMany(x => x.Monsters)
            .ToList();

        var npcs = world.Spawns
            .Where(x => x.Npcs is not null)
            .SelectMany(x => x.Npcs)
            .ToList();

        foreach (var monsterToSpawn in monsters)
        {
            var monster = creatureFactory.CreateMonster(monsterToSpawn.Name, monsterToSpawn.Spawn); // TODO: NTN - create SpawnMonsterUseCase?

            if (monster is null) continue;
            
            monster.Born(monsterToSpawn.Spawn.Location);
            creatureGameInstance.Add(monster);
            map.PlaceCreature(monster);
        }

        foreach (var npcToSpawn in npcs)
        {
            var npc = creatureFactory.CreateNpc(npcToSpawn.Name, npcToSpawn.Spawn); // TODO: NTN - create SpawnNpcUseCase?
            if (npc is null) continue;

            creatureGameInstance.Add(npc);
            npc.SetNewLocation(npcToSpawn.Spawn.Location);
            map.PlaceCreature(npc);
        }
    }
}