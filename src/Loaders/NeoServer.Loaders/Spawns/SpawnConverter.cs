using System.Collections.Generic;
using System.Linq;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.World.Models.Spawns;

namespace NeoServer.Loaders.Spawns;

public static class SpawnConverter
{
    public static Spawn Convert(SpawnData spawnData)
    {
        var spawn = new Spawn
        {
            Location = new Location(spawnData.CenterX, spawnData.CenterY, spawnData.CenterZ),
            Radius = spawnData.Radius
        };

        AddNpcToSpawn(spawnData.Npcs?.ToList(), spawn);
        AddMonsterToSpawn(spawnData.Monsters?.ToList(), spawn);

        return spawn;
    }

    private static void AddNpcToSpawn(IList<SpawnData.Creature> creatures, ISpawn spawn)
    {
        if (creatures is null) return;

        spawn.Npcs = new ISpawn.ICreature[creatures.Count];

        var i = 0;
        foreach (var creature in creatures)
            spawn.Npcs[i++] = CreateCreature(spawn, creature);
    }

    private static void AddMonsterToSpawn(IList<SpawnData.Creature> creatures, ISpawn spawn)
    {
        if (creatures is null) return;

        spawn.Monsters = new ISpawn.ICreature[creatures.Count];

        var i = 0;

        foreach (var creature in creatures)
            spawn.Monsters[i++] = CreateCreature(spawn, creature);
    }

    private static Spawn.Creature CreateCreature(ISpawn spawn, SpawnData.Creature creature)
    {
        return new Spawn.Creature
        {
            Name = creature.Name,
            Spawn = new SpawnPoint(
                new Location((ushort)(creature.X + spawn.Location.X),
                    (ushort)(creature.Y + spawn.Location.Y), creature.Z), creature.SpawnTime, creature.Direction)
        };
    }
}