using NeoServer.Domain.Combat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location;

namespace NeoServer.Domain.Creatures.Monster.Services;

public class TargetDetectorService(IMapTool mapTool)
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
        // if the target is dead, we ignore it
        if (target.IsDead) return true;

        // if the target is in a protection zone, we ignore it
        if (target.Tile?.ProtectionZone ?? false)
        {
            return true;
        }
        
        // if the monster is in a protection zone, we ignore it
        if (monster.Tile?.ProtectionZone ?? false)
        {
            return true;
        }
        
        if (!monster.CanSee(target.Location)) return true;

        // if the target is in the same floor as the monster, we ignore it
        if (!monster.Location.SameFloorAs(target.Location)) return true;
        
        return false;
    }
}