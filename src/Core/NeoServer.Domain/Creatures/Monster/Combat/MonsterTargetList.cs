using NeoServer.Domain.Combat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Creatures.Monster.Combat;

public enum TargetSearchType
{
    Default,
    Nearest,
    Random,
    AttackRange
}

/// <summary>
/// Manages a monster's target list with efficient add/remove operations.
/// Uses a LinkedList for ordered storage and a Dictionary for O(1) lookups by creature ID.
/// </summary>
public class MonsterTargetList(IMonster monster)
{
    private readonly LinkedList<CombatTarget> _list = [];
    private readonly Dictionary<uint, LinkedListNode<CombatTarget>> _nodeMap = new();

    /// <summary>
    /// Adds a target to the list. Priority targets are placed at the front.
    /// </summary>
    /// <param name="target">The creature to add as a target.</param>
    /// <param name="hasPriority">Whether to prioritize this target.</param>
    public void Add(ICombatActor target, bool hasPriority = false)
    {
        if(target is null) return;
        if (_nodeMap.ContainsKey(target.CreatureId)) return; // Already tracking

        var combatTarget = new CombatTarget(target);

        var node = hasPriority ? _list.AddFirst(combatTarget) : _list.AddLast(combatTarget);

        _nodeMap[target.CreatureId] = node;
    }

    /// <summary>
    /// Removes a target from the list.
    /// </summary>
    /// <param name="target">The creature to remove from tracking.</param>
    public void Remove(ICombatActor target)
    {
        if(target is null) return;
        if (!_nodeMap.TryGetValue(target.CreatureId, out var node)) return;
        
        _list.Remove(node);
        _nodeMap.Remove(target.CreatureId);
    }


    /// <summary>
    /// Gets the first node for iteration.
    /// </summary>
    public LinkedListNode<CombatTarget> First => _list.First;

    /// <summary>
    /// Checks if the list contains any targets.
    /// </summary>
    public bool Any() => _list.Count != 0;

    /// <summary>
    /// Searches for a suitable target based on the specified search type.
    /// </summary>
    /// <param name="searchType">The type of search to perform.</param>
    /// <returns>The selected target or null if none found.</returns>
    public ICombatActor SearchTarget(TargetSearchType searchType = TargetSearchType.Default)
    {
        var candidates = new List<CombatTarget>();
        var myPos = monster.Location;

        // Build a list of valid candidates
        foreach (var combatTarget in _list)
        {
            var creature = combatTarget.Creature;
            if (monster.AutoAttackTargetId == creature.CreatureId || !IsTarget(creature))
                continue;

            if (searchType == TargetSearchType.Random || CanUseAttack(myPos, creature))
            {
                candidates.Add(combatTarget);
            }
        }

        CombatTarget selectedTarget = null;

        // Select a target based on a search type
        switch (searchType)
        {
            case TargetSearchType.Nearest:
                if (candidates.Count == 0)
                {
                    // Search all targets if no candidates
                    foreach (var combatTarget in _list)
                    {
                        if (IsTarget(combatTarget.Creature))
                        {
                            candidates.Add(combatTarget);
                        }
                    }
                }

                var minDistance = int.MaxValue;
                foreach (var candidate in candidates)
                {
                    var distance = Math.Max(Math.Abs(myPos.X - candidate.Creature.Location.X),
                                           Math.Abs(myPos.Y - candidate.Creature.Location.Y));
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        selectedTarget = candidate;
                    }
                }
                break;

            case TargetSearchType.Random:
            case TargetSearchType.Default:
            case TargetSearchType.AttackRange:
                if (candidates.Count > 0)
                {
                    selectedTarget = candidates[Random.Shared.Next(candidates.Count)];
                }
                break;
        }

        // Try to select the target
        if (selectedTarget != null && CanSelectTarget(selectedTarget.Creature))
        {
            return selectedTarget.Creature;
        }

        // Fallback: pick the first available target
        foreach (var combatTarget in _list)
        {
            if (monster.AutoAttackTargetId != combatTarget.Creature.CreatureId &&
                CanSelectTarget(combatTarget.Creature))
            {
                return combatTarget.Creature;
            }
        }

        return null;
    }

    private bool IsTarget(ICombatActor creature)
    {
        // Target must be alive and attackable
        return !creature.IsDead && creature.CanBeAttacked;
    }

    private bool CanUseAttack(Location myPos, ICombatActor creature)
    {
        // Check if monster can attack the creature (basic range check)
        var distance = myPos.GetSqmDistance(creature.Location);
        return distance <= 1; // Simplified range check
    }

    private bool CanSelectTarget(ICombatActor target)
    {
        if(!IsTarget(target)) return false;

        if (!HasTarget(target))
        {
            return false;
        }

        return monster.CanSee(target) && monster.CanSee(target.Location);
    }

    public void Clear()
    {
        _list.Clear();
        _nodeMap.Clear();
    }

    public bool HasTarget(ICreature player) => _nodeMap.ContainsKey(player.CreatureId);
}