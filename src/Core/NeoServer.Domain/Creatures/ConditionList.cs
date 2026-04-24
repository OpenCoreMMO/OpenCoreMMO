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
    private readonly List<ICondition> _conditionsCache = [];
    private readonly List<ICondition> _allConditions = [];
    
    private bool IsCacheValid => _conditionsCache.Count > 0;

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
            for (int i = 0; i < conditions.Count; i++)
            {
                var existingCondition = conditions[i];
                if (!existingCondition.IsPersistent)
                {
                    existingCondition.End();
                    conditions.RemoveAt(i);
                    break;
                }
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
            InvalidateCache();
        }
    }

    public void RemoveByType(ConditionType type)
    {
        if (!Conditions.TryGetValue(type, out var conditions)) return;

        if (conditions.Count == 0)
        {
            return;
        }

        conditions.Clear();
        InvalidateCache();
    }

    public IReadOnlyList<ICondition> GetByType(ConditionType conditionType) =>
        Conditions.TryGetValue(conditionType, out var conditions) ? conditions : [];

    public int GetCount()
    {
        var count = 0;
        
        foreach (var conditions in Conditions.Values)
        {
            count += conditions.Count;
        }

        return count;
    }

    public IReadOnlyList<ICondition> GetAll()
    {
        if (_conditionsCache.Count > 0)
        {
            return _conditionsCache.AsReadOnly();
        }

        foreach (var conditions in Conditions.Values)
        {
            foreach (var condition in conditions)
            {
                _conditionsCache.Add(condition);
            }
        }

        return _conditionsCache.AsReadOnly();
    }

    public ICondition GetFirstConditionOfType(ConditionType conditionType) =>
        GetByType(conditionType).FirstOrDefault();

    public bool GetFirstConditionOfType(ConditionType conditionType, out ICondition condition)
    {
        condition = GetByType(conditionType).FirstOrDefault();
        return condition != null;
    }

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

    public void EnableConditions(ConditionType conditionType)
    {
        var conditions = GetByType(conditionType);

        if (conditions.Count == 0) return;

        foreach (var condition in conditions)
        {
            condition?.Enable();
        }
    }

    public bool HasAnyConditionOf(ConditionType conditionType) => GetByType(conditionType).Count > 0;

    public bool HasAnyConditionOf(ConditionType conditionType, out ICondition condition)
    {
        condition = GetByType(conditionType).FirstOrDefault();
        return condition != null;
    }

    public bool HasAnyConditionOf(ConditionType conditionType, out IReadOnlyList<ICondition> conditions)
    {
        conditions = GetByType(conditionType);
        return conditions.Count > 0;
    }

    private void InvalidateCache() => _conditionsCache.Clear();
    public void Clear()
    {
        Conditions.Clear();
        InvalidateCache();
    }

    IEnumerator<ICondition> IEnumerable<ICondition>.GetEnumerator() => GetAll().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetAll().GetEnumerator();
}