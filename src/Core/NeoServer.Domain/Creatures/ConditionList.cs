using System.Collections;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures;

/// <summary>
/// Represents a collection of conditions categorized by their type, allowing for
/// management, retrieval, and modification of conditions.
/// </summary>
public class ConditionList : IEnumerable<ICondition>
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
        if (!Conditions.TryGetValue(condition.Type, out var conditions))
        {
            conditions = [];
            Conditions[condition.Type] = conditions;
        }

        // Remove any existing non-persistent conditions of the same type
        if (!condition.IsPersistent)
        {
            ICondition toRemove = null;
            foreach (var existingCondition in conditions)
            {
                if (!existingCondition.IsPersistent)
                {
                    toRemove = existingCondition;
                    break;
                }
            }

            if (toRemove is not null)
            {
                toRemove.End();
                conditions.Remove(toRemove);
            }
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
            condition.End();
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

        if (endCondition)
        {
            foreach (var condition in conditions)
            {
                condition?.End();
            }
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
        Conditions.TryGetValue(conditionType, out var conditions) ? conditions : [];

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
    public ICondition GetFirstConditionOfType(ConditionType conditionType) =>
        GetByType(conditionType).FirstOrDefault();

    /// <summary>
    /// Checks if there is at least one condition of the specified condition type in the condition list. If such a condition exists, the method returns true and outputs the first found condition; otherwise, it returns false and outputs null.
    /// </summary>
    /// <param name="conditionType"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    public bool GetFirstConditionOfType(ConditionType conditionType, out ICondition condition)
    {
        condition = GetByType(conditionType).FirstOrDefault();
        return condition != null;
    }

    /// <summary>
    /// Ends all conditions of the specified condition type. For each condition of the specified type, the <see cref="ICondition.End"/> method is called. If the remove parameter is set to true (which is the default value), all conditions of the specified type are removed from the condition list after being ended; otherwise, they remain in the list but are considered ended. The cache of conditions is invalidated after the operation.
    /// </summary>
    /// <param name="conditionType"></param>
    /// <param name="remove"></param>
    public void EndConditions(ConditionType conditionType, bool remove = true)
    {
        if (!Conditions.TryGetValue(conditionType, out var conditions)) return;

        if (conditions.Count == 0) return;

        for (var i = 0; i < conditions.Count; i++)
        {
            var condition = conditions[i];
            condition?.End();
        }

        if (remove)
        {
            conditions.Clear();
            InvalidateCache();
        }
    }

    /// <summary>
    /// Disables all conditions of the specified condition type. For each condition of the specified type,
    /// the <see cref="ICondition.Disable"/> method is called.
    /// </summary>
    /// <param name="conditionType">The type of the conditions to be disabled.</param>
    public void DisableConditions(ConditionType conditionType)
    {
        var conditions = GetByType(conditionType);

        if (conditions.Count == 0) return;

        foreach (var condition in conditions)
        {
            condition?.Disable();
        }
    }

    /// <summary>
    /// Enables all conditions of the specified condition type. For each condition of the specified type,
    /// </summary>
    /// <param name="conditionType"></param>
    public void EnableConditions(ConditionType conditionType)
    {
        var conditions = GetByType(conditionType);

        if (conditions.Count == 0) return;

        foreach (var condition in conditions)
        {
            condition?.Enable();
        }
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
        condition = GetByType(conditionType).FirstOrDefault();
        return condition != null;
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

    private void InvalidateCache() => _conditionsCache = null;

    /// <summary>
    /// Clears all conditions from the condition list. For each condition in the condition list, the <see cref="ICondition.End"/> method is called to end the condition before it is removed from the list. After all conditions have been ended and removed, the cache of conditions is invalidated.
    /// </summary>
    public void Clear()
    {
        foreach (var conditions in Conditions.Values)
        {
            foreach (var condition in conditions)
            {
                condition?.End();
            }
        }

        Conditions.Clear();

        InvalidateCache();
    }

    IEnumerator<ICondition> IEnumerable<ICondition>.GetEnumerator() => GetAll().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetAll().GetEnumerator();
}