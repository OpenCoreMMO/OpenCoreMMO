using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Common.Contracts.Creatures;

public interface ICondition
{
    ConditionIconType Icons { get; }
    ConditionType Type { get; }

    public FormulaValues FormulaValues { get; set; }

    public Dictionary<ConditionParamType, uint> Parameters { get; set; }

    bool HasExpired { get; }

    /// <summary>
    ///     Remaining time in milliseconds
    /// </summary>
    long RemainingTime { get; }

    long StartedAt { get; }

    bool IsDisabled { get; }
    bool IsPersistent { get; }
    int PersistentCounter { get; }
    long Duration { get; }

    bool Start(ICreature creature);
    void End();

    /// <summary>
    ///     Extends condition duration in milliseconds
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="maxDuration"></param>
    void Extend(uint duration, uint maxDuration = uint.MaxValue);

    void Disable();
    void Enable();
    void IncreasePersistentCounter();
    void ReducePersistentCounter();

    /// <summary>
    /// Updates the duration of the condition by setting a new value.
    /// </summary>
    /// <param name="duration">The new duration in milliseconds. This value is converted to ticks internally.</param>
    void SetNewDuration(uint duration);

    /// <summary>
    /// Updates the duration of the condition by setting a new value.
    /// </summary>
    /// <param name="duration">The new duration in ticks.</param>
    void SetNewDuration(long duration);
}