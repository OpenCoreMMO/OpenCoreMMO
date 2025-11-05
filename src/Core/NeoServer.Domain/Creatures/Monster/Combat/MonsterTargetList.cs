using NeoServer.Domain.Combat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Player;

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
    private readonly LinkedList<ICombatActor> _list = [];
    private readonly Dictionary<uint, LinkedListNode<ICombatActor>> _nodeMap = new();
    
    public int Count => _list.Count;

    /// <summary>
    /// Adds a target to the list. Priority targets are placed at the front.
    /// </summary>
    /// <param name="target">The creature to add as a target.</param>
    /// <param name="hasPriority">Whether to prioritize this target.</param>
    public void Add(ICombatActor target, bool hasPriority = false)
    {
        if(target is null) return;
        if (_nodeMap.ContainsKey(target.CreatureId)) return; // Already tracking
        
        var isPlayerOrPlayerSummon = target is IPlayer or Summon.Summon { Master: IPlayer };

        // Skip dead creatures and ignored players
        if(!isPlayerOrPlayerSummon || target.IsDead || target == monster || target is IPlayer player && player.Group.FlagIsEnabled(PlayerFlag.IgnoredByMonsters)) return;
        
        var node = hasPriority ? _list.AddFirst(target) : _list.AddLast(target);

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
    public LinkedListNode<ICombatActor> First => _list.First;

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
        var candidates = new List<ICombatActor>();
        var myPos = monster.Location;

        // Build a list of valid candidates
        foreach (var combatTarget in _list)
        {
            var creature = combatTarget;
            if (monster.AutoAttackTargetId == creature.CreatureId || !IsTarget(creature))
                continue;

            if (searchType == TargetSearchType.Random || CanUseAttack(myPos, creature))
            {
                candidates.Add(combatTarget);
            }
        }

        ICombatActor selectedTarget = null;

        // Select a target based on a search type
        switch (searchType)
        {
            case TargetSearchType.Nearest:
                if (candidates.Count == 0)
                {
                    // Search all targets if no candidates
                    foreach (var combatTarget in _list)
                    {
                        if (IsTarget(combatTarget))
                        {
                            candidates.Add(combatTarget);
                        }
                    }
                }

                var minDistance = int.MaxValue;
                foreach (var candidate in candidates)
                {
                    var distance = myPos.GetMaxSqmDistance(candidate.Location);
                    
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
                    selectedTarget = candidates[GameRandom.Random.Next(maxValue: candidates.Count)];
                }
                break;
        }

        // Try to select the target
        if (selectedTarget != null && CanSelectTarget(selectedTarget))
        {
            return selectedTarget;
        }

        // Fallback: pick the first available target
        foreach (var combatTarget in _list)
        {
            if ( CanSelectTarget(combatTarget))
            {
                return combatTarget;
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

        foreach (var attack in monster.Metadata.Attacks)
        {
            if (attack.CombatParameter.Range != 0 && distance <= attack.CombatParameter.Range)
            {
                
            }
        }
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