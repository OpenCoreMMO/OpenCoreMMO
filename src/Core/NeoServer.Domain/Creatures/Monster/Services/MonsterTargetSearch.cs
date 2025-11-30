using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Monster.Combat;

namespace NeoServer.Domain.Creatures.Monster.Services;

public interface IMonsterTargetSearch
{
    ICombatActor Search(Monster monster, TargetSearchType searchType);
}

/// <summary>
///     Evaluates the current target list and selects the best candidate according to the requested search strategy.
/// </summary>
public class MonsterTargetSearch(IMapTool mapTool) : IMonsterTargetSearch
{
    public ICombatActor Search(Monster monster, TargetSearchType searchType)
    {
        if (monster is null) return null;

        var candidates = new List<ICombatActor>();
        var monsterPosition = monster.Location;

        foreach (var combatTarget in monster.Targets.Enumerate())
        {
            if (monster.AutoAttackTargetId == combatTarget.CreatureId || !IsTarget(monster, combatTarget)) continue;

            if (searchType == TargetSearchType.Random || CanUseAttack(monster, monsterPosition, combatTarget))
                candidates.Add(combatTarget);
        }

        var selectedTarget = SelectByStrategy(monster, monsterPosition, candidates, searchType);

        if (selectedTarget is not null && CanSelectTarget(monster, selectedTarget)) return selectedTarget;

        foreach (var fallbackTarget in monster.Targets.Enumerate())
        {
            if (CanSelectTarget(monster, fallbackTarget))
            {
                return fallbackTarget;
            }
        }

        return null;
    }

    private static ICombatActor SelectByStrategy(
        Monster monster,
        Location monsterPosition,
        IList<ICombatActor> candidates,
        TargetSearchType searchType)
    {
        if (candidates.Count == 0 && searchType == TargetSearchType.Nearest)
        {
            foreach (var target in monster.Targets.Enumerate())
            {
                if (IsTarget(monster, target))
                {
                    candidates.Add(target);
                }
            }
        }

        return searchType switch
        {
            TargetSearchType.Nearest => SelectNearest(monsterPosition, candidates),
            TargetSearchType.Random or TargetSearchType.Default or TargetSearchType.AttackRange =>
                SelectRandom(candidates),
            _ => null
        };
    }

    private static ICombatActor SelectNearest(Location origin, IList<ICombatActor> candidates)
    {
        if (candidates.Count == 0) return null;

        var minDistance = int.MaxValue;
        ICombatActor nearest = null;

        foreach (var candidate in candidates)
        {
            var distance = origin.GetMaxSqmDistance(candidate.Location);
            if (distance < minDistance)
            {
                nearest = candidate;
                minDistance = distance;
            }
        }

        return nearest;
    }

    private static ICombatActor SelectRandom(IList<ICombatActor> candidates)
    {
        if (candidates.Count == 0) return null;
        return candidates[GameRandom.Random.Next(maxValue: candidates.Count)];
    }

    private static bool CanSelectTarget(Monster monster, ICombatActor target)
    {
        if (!IsTarget(monster, target)) return false;
        if (!monster.Targets.HasTarget(target)) return false;

        return monster.CanSee(target) && monster.CanSee(target.Location);
    }

    private static bool IsTarget(Monster monster, ICombatActor creature)
    {
        return !creature.IsDead && creature.CanBeAttacked && monster.CanSee(creature) && monster.Location.SameFloorAs(creature.Location);
    }

    private bool CanUseAttack(Monster monster, Location monsterPosition, ICombatActor target)
    {
        var distance = monsterPosition.GetSqmDistance(target.Location);

        if (!monster.IsHostile) return true;

        foreach (var attack in monster.Metadata.Attacks)
        {
            if (attack.CombatParameter.Range != 0 && distance <= attack.CombatParameter.Range)
                return mapTool.IsClearSight(monsterPosition, target.Location, true);
        }

        return false;
    }
}