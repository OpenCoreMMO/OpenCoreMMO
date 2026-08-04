#nullable enable
using System;
using System.Text.Json;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;

namespace NeoServer.Data.Helpers.ConditionParsers;

public static class ParalyzeConditionParser
{
    public static string Serialize(ParalyzeCondition condition)
    {
        ArgumentNullException.ThrowIfNull(condition);

        var state = condition.CaptureState();
        return JsonSerializer.Serialize(state);
    }

    public static ParalyzeCondition? Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var state = JsonSerializer.Deserialize<ParalyzeConditionState>(json);

        if (state is null)
            throw new InvalidOperationException("Failed to deserialize paralyze condition: JSON was null.");

        // Restore returns null when the condition has expired (RemainingTimeMilliseconds <= 0).
        // The caller (ConditionListParser) skips null entries, so expired paralyze records
        // are silently ignored rather than failing player materialization.
        return ParalyzeCondition.Restore(state);
    }

    public static bool CanHandle(ConditionType type)
    {
        return type is ConditionType.Paralyze;
    }
}
