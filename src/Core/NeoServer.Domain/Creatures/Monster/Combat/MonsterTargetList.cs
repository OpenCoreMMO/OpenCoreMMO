using NeoServer.Domain.Combat;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Monster.Combat;

/// <summary>
/// Manages a monster's target list with efficient add/remove operations.
/// Uses a LinkedList for ordered storage and a Dictionary for O(1) lookups by creature ID.
/// </summary>
public class MonsterTargetList
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
}