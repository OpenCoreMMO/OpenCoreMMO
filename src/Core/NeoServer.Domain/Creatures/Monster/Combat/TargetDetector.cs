using NeoServer.Domain.Combat;
using NeoServer.Domain.Combat.Validation;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location;

namespace NeoServer.Domain.Creatures.Monster.Combat;

internal static class TargetDetector
{
    /// <summary>
    ///     Updates monster target list
    /// </summary>
    public static void UpdateTargets(Monster monster, IMapTool mapTool)
    {
        if (monster.Targets.IsNull()) return;

        var nearest = ushort.MaxValue;
        var nearestSightClear = ushort.MaxValue;

        monster.Targets.NearestTarget = null;
        monster.Targets.NearestSightClearTarget = null;

        var targetsToRemove = new List<ICombatActor>();
        var targetsToAdd = new List<ICombatActor>();

        foreach (CombatTarget target in monster.Targets)
        {
            target.ResetFlags();

            var combatTarget = target;

            var creature = target.Creature;

            if (creature is IPlayer player && player.Summons.Count > 0)
            {
                creature = player.Summons[0];

                if (monster.CurrentTarget != null && monster.CurrentTarget.CreatureId == creature.CreatureId)
                    return;

                targetsToRemove.Add(player);
                targetsToAdd.Add(creature);
                combatTarget = new CombatTarget(creature);
            }

            if (creature.IsDead)
            {
                targetsToRemove.Add(creature);
                continue;
            }

            if (mapTool.SightClearChecker.Invoke(monster.Location, creature.Location, true) &&
                combatTarget.IsInRange(monster))
            {
                combatTarget.SetAsHasSightClear();

                var offsetSightClear = monster.Location.GetSqmDistance(creature.Location);

                if (offsetSightClear < nearestSightClear)
                {
                    nearestSightClear = offsetSightClear;
                    monster.Targets.NearestSightClearTarget = combatTarget;
                }
            }

            var targetIsUnreachable = IsTargetUnreachable(monster, combatTarget, mapTool);
            if (targetIsUnreachable.Unreachable) continue;

            combatTarget.SetAsReachable(targetIsUnreachable.Directions);

            var offset = monster.Location.GetSqmDistance(creature.Location);

            if (offset < nearest)
            {
                nearest = offset;
                monster.Targets.NearestTarget = combatTarget;
            }
        }

        foreach (var target in targetsToAdd)
            monster.Targets.AddTarget(target);

        foreach (var target in targetsToRemove)
            monster.Targets.RemoveTarget(target);
    }

    private static (bool Unreachable, Direction[] Directions) IsTargetUnreachable(Monster monster, CombatTarget target,
        IMapTool mapTool)
    {
        var result = mapTool.PathFinder.Find(monster, target.Creature.Location, monster.PathSearchParams,
            monster.TileEnterRule);

        if (!result.Found) return (true, []);

        if (AttackValidation.CanAttack(monster, target.Creature).Failed) return (true, []);

        if (target.Creature.IsInvisible && !monster.CanSeeInvisible) return (true, []);

        return (false, result.Directions);
    }
}