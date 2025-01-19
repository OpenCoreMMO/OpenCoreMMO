using System;
using System.Linq;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.UseCase.Monster;
using NeoServer.Game.Common.Contracts.World;

namespace NeoServer.Game.World.Models.Spawns;

public class SpawnManager(
    World world,
    IMap map,
    ICreatureGameInstance creatureGameInstance,
    ICreatureFactory creatureFactory,
    ICreateMonsterOrSummonUseCase createMonsterOrSummonUseCase)
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
            createMonsterOrSummonUseCase.Execute(monster.Name, monster.Spawn);
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
            createMonsterOrSummonUseCase.Execute(monsterToSpawn.Name, monsterToSpawn.Spawn);
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