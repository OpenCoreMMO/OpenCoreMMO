using System.Collections;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures;

/// <summary>
/// Represents a collection of conditions categorized by their type, allowing for
/// management, retrieval, and modification of conditions.
/// </summary>
internal class ConditionList : IEnumerable<ICondition>
{
    private Dictionary<ConditionType, List<ICondition>> Conditions { get; } = new();
    private IReadOnlyList<ICondition> _conditionsCache;

    private bool IsCacheValid => _conditionsCache is not null;

    public int Count => IsCacheValid ? _conditionsCache.Count : GetCount();

    /// <summary>
    /// Adds the specified condition to the condition list. If the condition type has not been encountered before,
    /// a new collection entry is created for that type in the condition list. Non-persistent conditions of the same type
    /// are removed before the addition. The condition cache is marked as invalid after the addition.
    /// </summary>
    /// <param name="condition">The condition to be added to the condition list.</param>
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
    /// Removes the specified condition from the condition list if it exists.
    /// The cache of conditions is invalidated after the removal.
    /// </summary>
    /// <param name="condition">The condition to be removed from the condition list.</param>
    public void Remove(ICondition condition)
    {
        if (Conditions.TryGetValue(condition.Type, out var conditions) && conditions.Remove(condition))
        {
            InvalidateCache();
        }
    }

    /// <summary>
    /// Removes all conditions of the specified condition type from the condition list.
    /// If there are no conditions of the specified type, the method does nothing.
    /// The cache of conditions is invalidated after the removal.
    /// </summary>
    /// <param name="type">The type of conditions to remove from the condition list.</param>
    /// <param name="endCondition">
    /// <see langword="true"/> to invoke <see cref="ICondition.End()"/> for each removed condition before clearing them;
    /// otherwise, <see langword="false"/> to remove the conditions without invoking their end actions.
    /// </param>
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
    /// Retrieves all conditions of the specified condition type from the condition list. If there are no conditions of the specified type, the method returns an empty list.
    /// </summary>
    /// <param name="conditionType"></param>
    /// <returns></returns>
    public IReadOnlyList<ICondition> GetByType(ConditionType conditionType) =>
        Conditions.TryGetValue(conditionType, out var conditions) ? conditions.AsReadOnly() : [];

    /// <summary>
    /// Calculates the total number of conditions in the condition list by iterating through all collections of conditions by type and summing their counts. This method is used when the cache of conditions is not valid to provide an accurate count of conditions without relying on the cache.
    /// </summary>
    /// <returns></returns>
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
    /// Retrieves all conditions from the condition list. The method returns a read-only list of all conditions currently present in the condition list. If the cache of conditions is valid, it is returned; otherwise, a new list is created by iterating through all collections of conditions by type, and the cache is updated with this new list before returning it.
    /// </summary>
    /// <returns></returns>
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
    /// Retrieves the first condition of the specified condition type from the condition list. If there are no conditions of the specified type, the method returns null.
    /// </summary>
    /// <param name="conditionType"></param>
    /// <returns></returns>
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
    /// Checks if there is at least one condition of the specified condition type in the condition list. If such a condition exists, the method returns true and outputs the first found condition; otherwise, it returns false and outputs null.
    /// </summary>
    /// <param name="conditionType"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
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
    /// Checks if there is at least one condition of the specified condition type in the condition list. If such a condition exists, the method returns true; otherwise, it returns false.
    /// </summary>
    /// <param name="conditionType"></param>
    /// <returns></returns>
    public bool HasAnyConditionOf(ConditionType conditionType) => GetByType(conditionType).Count > 0;

    /// <summary>
    /// Checks if there is at least one condition of the specified condition type in the condition list. If such a condition exists, the method returns true and outputs the first found condition; otherwise, it returns false and outputs null.
    /// </summary>
    /// <param name="conditionType"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
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
    /// Checks if there is at least one condition of the specified condition type in the condition list. If such conditions exist, the method returns true and outputs the list of conditions; otherwise, it returns false and outputs an empty list.
    /// </summary>
    /// <param name="conditionType"></param>
    /// <param name="conditions"></param>
    /// <returns></returns>
    public bool HasAnyConditionOf(ConditionType conditionType, out IReadOnlyList<ICondition> conditions)
    {
        conditions = GetByType(conditionType);
        return conditions.Count > 0;
    }

    /// <summary>
    /// Checks if there is at least one enabled condition of the specified condition type in the condition list. If such a condition exists, the method returns true; otherwise, it returns false.
    /// </summary>
    /// <param name="conditionType"></param>
    /// <returns></returns>
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
    /// Checks if there is at least one enabled condition of the specified condition type in the condition list. If such a condition exists,
    /// </summary>
    /// <param name="conditionType"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
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
    /// Clears all conditions from the condition list. For each condition in the condition list, the <see cref="ICondition.End"/> method is called to end the condition before it is removed from the list. After all conditions have been ended and removed, the cache of conditions is invalidated.
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