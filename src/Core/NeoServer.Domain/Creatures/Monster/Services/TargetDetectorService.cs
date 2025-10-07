using NeoServer.Domain.Combat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location;

namespace NeoServer.Domain.Creatures.Monster.Services;

public class MonsterTargetListService(IMap map)
{
    /// <summary>
    /// Maintains the monster's combat focus by cleaning up its target list.
    /// Removes targets that are no longer viable threats, such as dead creatures or those outside the monster's perception range.
    /// This ensures the monster only pursues active, reachable enemies.
    /// </summary>
    /// <param name="monster">The monster whose target priorities need updating.</param>
    public void Update(Monster monster)
    {
        // Begin checking from the monster's first tracked target
        var current = monster.MonsterTargets.First;

        // Review each target in the monster's list
        while (current != null)
        {
            var target = current.Value;

            // Determine if this target is still a valid threat
            if (target.Creature.IsDead || !monster.CanSee(target.Creature) || !monster.CanSee(target.Creature.Location))
            {
                var next = current.Next; // Remember the next target before removing this one
                monster.MonsterTargets.Remove(target.Creature); // Remove the invalid target from tracking
                current = next;
                continue; // Proceed to check the next target
            }

            current = current.Next;
        }   
        
        // Scan nearby creatures to find new potential targets
        var spectators = map.GetSpectators(monster.Location);

        foreach (var spectator in spectators)
        {
            // Only consider players and their summons as valid targets
            if (spectator is not ICombatActor target) continue;
            var isPlayerOrPlayerSummon = spectator is IPlayer or Summon.Summon { Master: IPlayer };

            if (!isPlayerOrPlayerSummon) continue;

            // Skip dead creatures
            if (target.IsDead) continue;

            // Must be visible to the monster
            if (!monster.CanSee(target.Location)) continue;
            if (!monster.CanSee(target)) continue;

            // Skip creatures in protection zones
            if (target.Tile?.ProtectionZone ?? false) continue;

            // Must be on the same floor
            if (!monster.Location.SameFloorAs(target.Location)) continue;

            // Add as a new target (MonsterTargetList handles duplicates)
            monster.MonsterTargets.Add(target, false);
        }
    }
}

public class TargetDetectorService(IMapTool mapTool, IMap map)
{
    /// <summary>
    /// Updates the monster's targets by checking their reachability and sight clearance.
    /// </summary>
    /// <param name="monster"></param>
    public void UpdateTargets(Monster monster)
    {
        if (monster.Targets.IsNull()) return;

        var nearest = ushort.MaxValue;
        var nearestSightClear = ushort.MaxValue;

        monster.Targets.NearestTarget = null;
        monster.Targets.NearestSightClearTarget = null;

        foreach (CombatTarget target in monster.Targets)
        {
            target.ResetFlags();

            if (target.Creature.IsDead)
            {
                monster.Targets.RemoveTarget(target.Creature);
                continue;
            }

            if (mapTool.SightClearChecker.Invoke(monster.Location, target.Creature.Location, true) &&
                target.IsInRange(monster))
            {
                target.SetAsHasSightClear();

                var offsetSightClear = monster.Location.GetSqmDistance(target.Creature.Location);

                if (offsetSightClear >= nearestSightClear) continue;
                nearestSightClear = offsetSightClear;
                monster.Targets.NearestSightClearTarget = target;
            }

            var targetIsUnreachable = IsTargetUnreachable(monster, target);
            if (targetIsUnreachable.Unreachable) continue;
            target.SetAsReachable(targetIsUnreachable.Directions);
            var offset = monster.Location.GetSqmDistance(target.Creature.Location);

            if (offset >= nearest) continue;

            nearest = offset;
            monster.Targets.NearestTarget = target;
        }
    }

    private (bool Unreachable, Direction[] Directions) IsTargetUnreachable(Monster monster, CombatTarget target)
    {
        var result = mapTool.PathFinder.Find(monster, target.Creature.Location, monster.PathSearchParams,
            monster.TileEnterRule);

        if (!result.Found) return (true, []);

        if (IgnoreTarget(monster, target.Creature))
            return (true, []);

        if (target.Creature.IsInvisible && !monster.CanSeeInvisible) return (true, []);

        return (false, result.Directions);
    }

    private static bool IgnoreTarget(Monster monster, ICombatActor target)
    {
        if (target is null) return true;

        // if the target is dead, we ignore it
        if (target.IsDead) return true;

        // if the target is in a protection zone, we ignore it
        if (target.Tile?.ProtectionZone ?? false)
        {
            return true;
        }

        if (!monster.CanSee(target.Location)) return true;

        // if the target is in the same floor as the monster, we ignore it
        if (!monster.Location.SameFloorAs(target.Location)) return true;

        return false;
    }

    /// <summary>
    /// Gets the best target for the monster based on detection logic.
    /// </summary>
    /// <param name="monster"></param>
    /// <returns>The selected target or null if no valid target found.</returns>
    public ICombatActor GetTarget(Monster monster)
    {
        var spectators = map.GetSpectators(monster.Location, false, false,
            -(int)MapViewPort.MaxClientViewPortX, (int)MapViewPort.MaxClientViewPortX,
            -(int)MapViewPort.MaxClientViewPortY, (int)MapViewPort.MaxClientViewPortY);

        if (spectators.Count == 0) return null;

        var targets = new List<ICombatActor>();

        foreach (var spectator in spectators)
        {
            var isPlayerOrPlayerSummon = spectator is IPlayer or Summon.Summon { Master: IPlayer };

            if (!isPlayerOrPlayerSummon) continue;

            if (!monster.CanSeeInvisible && spectator.IsInvisible) continue;

            if (IgnoreTarget(monster, spectator as ICombatActor)) continue;

            targets.Add(spectator as ICombatActor);
        }

        if (targets.Count == 1)
        {
            return targets[0];
        }

        ICombatActor priorityTarget = null;
        var priorityValue = 0;

        foreach (var target in targets)
        {
            var targetPriorityValue = 0;

            var result = mapTool.PathFinder.Find(monster, target.Location, monster.PathSearchParams,
                monster.TileEnterRule);

            if (result.Found)
            {
                targetPriorityValue += 1;
            }

            if (monster.PathSearchParams.KeepDistance)
            {
                var isClearSight = mapTool.IsClearSight(monster.Location, target.Location, checkFloor: false);
                if (isClearSight)
                {
                    targetPriorityValue += 1;
                }
            }
            else
            {
                targetPriorityValue += 1;
            }

            if (targetPriorityValue == 2) return target;

            if (targetPriorityValue > priorityValue)
            {
                priorityTarget = target;
                priorityValue = targetPriorityValue;
            }
        }

        return priorityTarget;
    }
}