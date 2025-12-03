using NeoServer.Domain.Common.Contracts.Creatures;
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
///     Manages a monster's target list with efficient add/remove operations.
///     Uses a LinkedList for ordered storage and a Dictionary for O(1) lookups by creature ID.
/// </summary>
public class MonsterTargetList(IMonster monster)
{
    private readonly LinkedList<ICombatActor> _list = [];
    private readonly Dictionary<uint, LinkedListNode<ICombatActor>> _nodeMap = new();

    public int Count => _list.Count;


    /// <summary>
    ///     Gets the first node for iteration.
    /// </summary>
    public LinkedListNode<ICombatActor> First => _list.First;

    /// <summary>
    ///     Adds a target to the list. Priority targets are placed at the front.
    /// </summary>
    /// <param name="target">The creature to add as a target.</param>
    /// <param name="hasPriority">Whether to prioritize this target.</param>
    public void Add(ICombatActor target, bool hasPriority = false)
    {
        if (target is null) return;
        if (_nodeMap.ContainsKey(target.CreatureId)) return; // Already tracking

        //summon cannot add his own master to the target list
        if (monster is Summon.Summon summon && Equals(summon.Master, target))
        {
            return;
        }
        
        var isPlayerOrPlayerSummon = target is IPlayer or Summon.Summon { Master: IPlayer };

        // Skip dead creatures and ignored players
        if (!isPlayerOrPlayerSummon || target.IsDead || target == monster || (target is IPlayer player &&
                                                                              player.Group.FlagIsEnabled(PlayerFlag
                                                                                  .IgnoredByMonsters))) return;

        var node = hasPriority ? _list.AddFirst(target) : _list.AddLast(target);

        _nodeMap[target.CreatureId] = node;
    }

    /// <summary>
    ///     Removes a target from the list.
    /// </summary>
    /// <param name="target">The creature to remove from tracking.</param>
    public void Remove(ICombatActor target)
    {
        if (target is null) return;
        if (!_nodeMap.TryGetValue(target.CreatureId, out var node)) return;

        _list.Remove(node);
        _nodeMap.Remove(target.CreatureId);
    }

    /// <summary>
    ///     Checks if the list contains any targets.
    /// </summary>
    public bool Any()
    {
        return _list.Count != 0;
    }

    public void Clear()
    {
        _list.Clear();
        _nodeMap.Clear();
    }

    public bool HasTarget(ICreature player)
    {
        return _nodeMap.ContainsKey(player.CreatureId);
    }

    internal IEnumerable<ICombatActor> Enumerate()
    {
        foreach (var target in _list) yield return target;
    }
}