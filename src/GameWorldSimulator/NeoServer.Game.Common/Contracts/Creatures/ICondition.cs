using NeoServer.Game.Common.Creatures;

namespace NeoServer.Game.Common.Contracts.Creatures;

public interface ICondition
{
    ConditionIcon Icons { get; }
    ConditionType Type { get; }

    bool HasExpired { get; }

    /// <summary>
    ///     Remaining time in milliseconds
    /// </summary>
    long RemainingTime { get; }

    long StartedAt { get; }

    bool IsDisabled { get; }

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
}