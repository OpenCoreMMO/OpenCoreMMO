using System.Collections;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures;

/// <summary>
/// A collection of conditions grouped by <see cref="ConditionType"/> with an internal read cache.
/// </summary>
internal class ConditionList : IEnumerable<ICondition>
{
    private Dictionary<ConditionType, List<ICondition>> Conditions { get; } = new();
    private IReadOnlyList<ICondition> _conditionsCache;

    private bool IsCacheValid => _conditionsCache is not null;

    public int Count => IsCacheValid ? _conditionsCache.Count : GetCount();

    /// <summary>
    /// Adds a condition to the per-type list and invalidates the read cache.
    /// </summary>
    /// <param name="condition">The condition to add.</param>
    public void Add(ICondition condition)
    {
        ArgumentNullException.ThrowIfNull(condition);

        if (!Conditions.TryGetValue(condition.Type, out var conditions))
        {
            conditions = [];
            Conditions[condition.Type] = conditions;
        }

        conditions.Add(condition);

        InvalidateCache();
    }

    /// <summary>
    /// Removes a specific condition instance from the list and invalidates the cache.
    /// </summary>
    /// <param name="condition">The condition to remove.</param>
    public void Remove(ICondition condition)
    {
        if (Conditions.TryGetValue(condition.Type, out var conditions) && conditions.Remove(condition))
        {
            InvalidateCache();
        }
    }

    /// <summary>
    /// Removes all conditions of the given type and invalidates the cache.
    /// </summary>
    /// <param name="type">The condition type to clear.</param>
    public void RemoveByType(ConditionType type, bool endCondition = true)
    {
        if (!Conditions.TryGetValue(type, out var conditions)) return;

        if (conditions.Count == 0)
        {
            return;
        }
        
        conditions.Clear();
        InvalidateCache();
    }

    /// <summary>
    /// Returns all conditions of the given type, or an empty list if none exist.
    /// </summary>
    /// <param name="conditionType">The condition type to look up.</param>
    public IReadOnlyList<ICondition> GetByType(ConditionType conditionType) =>
        Conditions.TryGetValue(conditionType, out var conditions) ? conditions.AsReadOnly() : [];

    /// <summary>
    /// Calculates the total number of conditions across all types, used when the cache is invalid.
    /// </summary>
    public int GetCount()
    {
        var count = 0;

        foreach (var conditions in Conditions.Values)
        {
            count += conditions.Count;
        }

        return count;
    }

    /// <summary>
    /// Returns all conditions as a read-only list, using the cache if valid.
    /// </summary>
    public IReadOnlyList<ICondition> GetAll()
    {
        if (_conditionsCache is not null)
        {
            return _conditionsCache;
        }

        var list = new List<ICondition>();

        foreach (var conditions in Conditions.Values)
        {
            foreach (var condition in conditions)
            {
                list.Add(condition);
            }
        }

        _conditionsCache = list.AsReadOnly();
        return _conditionsCache;
    }

    /// <summary>
    /// Returns the first non-null condition of the given type, or null if none exist.
    /// </summary>
    /// <param name="conditionType">The condition type to look up.</param>
    public ICondition GetFirstConditionOfType(ConditionType conditionType)
    {
        if (Conditions.TryGetValue(conditionType, out var conditions))
        {
            for (var i = 0; i < conditions.Count; i++)
            {
                var condition = conditions[i];
                if (condition is not null) return condition;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks if there is at least one condition of the given type and outputs the first one found.
    /// </summary>
    /// <param name="conditionType">The condition type to look up.</param>
    /// <param name="condition">The first condition found, or null.</param>
    /// <returns>True if a condition exists.</returns>
    public bool GetFirstConditionOfType(ConditionType conditionType, out ICondition condition)
    {
        if (Conditions.TryGetValue(conditionType, out var conditions))
        {
            for (var i = 0; i < conditions.Count; i++)
            {
                var c = conditions[i];
                if (c is not null)
                {
                    condition = c;
                    return true;
                }
            }
        }

        condition = null;
        return false;
    }
    
    /// <summary>
    /// Checks if at least one condition of the given type exists.
    /// </summary>
    /// <param name="conditionType">The condition type to check.</param>
    /// <returns>True if any condition of that type is present.</returns>
    public bool HasAnyConditionOf(ConditionType conditionType) => GetByType(conditionType).Count > 0;

    /// <summary>
    /// Checks if at least one condition of the given type exists and outputs the first one found.
    /// </summary>
    /// <param name="conditionType">The condition type to check.</param>
    /// <param name="condition">The first condition found, or null.</param>
    /// <returns>True if a condition exists.</returns>
    public bool HasAnyConditionOf(ConditionType conditionType, out ICondition condition)
    {
        if (Conditions.TryGetValue(conditionType, out var conditions))
        {
            for (var i = 0; i < conditions.Count; i++)
            {
                var c = conditions[i];
                if (c is not null)
                {
                    condition = c;
                    return true;
                }
            }
        }

        condition = null;
        return false;
    }

    /// <summary>
    /// Checks if at least one condition of the given type exists and outputs all of them.
    /// </summary>
    /// <param name="conditionType">The condition type to check.</param>
    /// <param name="conditions">All conditions of that type, or an empty list.</param>
    /// <returns>True if any condition of that type exists.</returns>
    public bool HasAnyConditionOf(ConditionType conditionType, out IReadOnlyList<ICondition> conditions)
    {
        conditions = GetByType(conditionType);
        return conditions.Count > 0;
    }

    /// <summary>
    /// Checks if at least one enabled condition of the given type exists.
    /// </summary>
    /// <param name="conditionType">The condition type to check.</param>
    /// <returns>True if a non-disabled condition exists.</returns>
    public bool HasAnyEnabledConditionOf(ConditionType conditionType)
    {
        if (!Conditions.TryGetValue(conditionType, out var conditions)) return false;

        foreach (var condition in conditions)
        {
            if (!condition.IsDisabled)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if there is at least one enabled condition of the given type and outputs the first one found.
    /// </summary>
    /// <param name="conditionType">The condition type to check.</param>
    /// <param name="condition">The first enabled condition found, or null.</param>
    /// <returns>True if an enabled condition exists.</returns>
    public bool HasAnyEnabledConditionOf(ConditionType conditionType, out ICondition condition)
    {
        condition = null;
        if (!Conditions.TryGetValue(conditionType, out var conditions)) return false;

        foreach (var c in conditions)
        {
            if (!c.IsDisabled)
            {
                condition = c;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes all non-persistent conditions of the specified type from the condition list.
    /// Persistent conditions are left unchanged. The cache is invalidated if any removal occurs.
    /// </summary>
    public ICondition RemoveNonPersistentByType(ConditionType type)
    {
        if (!Conditions.TryGetValue(type, out var conditions)) return null;

        var removed = false;
        ICondition removedCondition = null;
        for (var i = conditions.Count - 1; i >= 0; i--)
        {
            if (!conditions[i].IsPersistent)
            {
                removedCondition = conditions[i];
                conditions.RemoveAt(i);
                removed = true;
            }
        }

        if (removed) InvalidateCache();
        return removedCondition;
    }

    private void InvalidateCache() => _conditionsCache = null;

    /// <summary>
    /// Removes all conditions from every type and invalidates the cache.
    /// </summary>
    public void Clear()
    {
        var total = 0;
        foreach (var list in Conditions.Values)
        {
            total += list.Count;
        }

        var snapshot = new List<ICondition>(total);
        foreach (var list in Conditions.Values)
        {
            foreach (var condition in list)
            {
                snapshot.Add(condition);
            }
        }

        Conditions.Clear();
        InvalidateCache();
    }

    IEnumerator<ICondition> IEnumerable<ICondition>.GetEnumerator() => GetAll().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetAll().GetEnumerator();
}