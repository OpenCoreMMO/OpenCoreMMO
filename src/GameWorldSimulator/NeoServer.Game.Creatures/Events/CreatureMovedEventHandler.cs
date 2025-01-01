using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Game.Creatures.Events;

public class CreatureMovedEventHandler : IGameEventHandler
{
    public void Execute(ICreature creature, Location fromLocation, Location toLocation,
        ICylinderSpectator[] spectators)
    {
        if (creature is IPlayer player && player.Group.FlagIsEnabled(PlayerFlag.IgnoredByMonsters)) return;

        foreach (var cylinderSpectator in spectators)
        {
            var spectator = cylinderSpectator.Spectator;
            if (creature == spectator) continue;

            if (spectator is ICombatActor { IsDead: true }) continue;

            if (CreatureOrSpectatorAreNpcs(creature, spectator)) continue;
            if (CreatureAndSpectatorAreBothPlayers(creature, spectator)) continue;
            if (CreatureAndSpectatorAreBothMonsters(creature, spectator)) continue;

            if (SpectatorMonsterIsNotHostile(creature, spectator)) continue;

            if (MonsterIsNotHostile(creature, spectator)) continue;

            if (SpectatorAndCreatureNotInSameFloor(creature, spectator)) continue;

            SetCreatureAndSpectatorAsEnemies(creature, spectator);
        }

        if (creature is ICombatActor combatActor) combatActor.Tile.MagicField?.CauseDamage(combatActor);
    }

    private static void SetCreatureAndSpectatorAsEnemies(ICreature creature, ICreature spectator)
    {
        if (spectator is not ICombatActor spectatorActor) return;
        if (creature is not ICombatActor actor) return;

        if (actor.IsDead || spectatorActor.IsDead) return;

        spectatorActor.SetAsEnemy(creature);
        actor.SetAsEnemy(spectator);
    }

    private static bool CreatureOrSpectatorAreNpcs(ICreature creature, ICreature spectator)
    {
        if (creature is INpc || spectator is INpc) return true;
        return false;
    }

    private static bool CreatureAndSpectatorAreBothPlayers(ICreature creature, ICreature spectator)
    {
        if (creature is IPlayer && spectator is IPlayer) return true;
        return false;
    }

    private static bool CreatureAndSpectatorAreBothMonsters(ICreature creature, ICreature spectator)
    {
        if (creature is IMonster && spectator is IMonster) return true;
        return false;
    }

    private static bool SpectatorMonsterIsNotHostile(ICreature creature, ICreature spectator)
    {
        if (spectator is IMonster { IsHostile: false } spectatorMonster)
        {
            spectatorMonster.SetAsEnemy(creature);
            return true;
        }

        return false;
    }

    private static bool MonsterIsNotHostile(ICreature creature, ICreature spectator)
    {
        if (creature is IMonster { IsHostile: false } monster)
        {
            monster.SetAsEnemy(spectator);
            return true;
        }

        return false;
    }

    private static bool SpectatorAndCreatureNotInSameFloor(ICreature creature, ICreature spectator)
    {
        if (!spectator.Location.SameFloorAs(creature.Location))
        {
            if (spectator is IMonster spectatorMonster) spectatorMonster.SetAsEnemy(creature);
            if (creature is IMonster monster) monster.SetAsEnemy(monster);
            return true;
        }

        return false;
    }
}