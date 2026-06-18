#nullable enable
using System;
using System.Text.Json;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;

namespace NeoServer.Data.Helpers.ConditionParsers;

public static class ConditionInvisibleParser
{
    public static string Serialize(ConditionInvisible condition)
    {
        ArgumentNullException.ThrowIfNull(condition);

        var state = condition.CaptureState();
        return JsonSerializer.Serialize(state);
    }

    public static ConditionInvisible? Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var state = JsonSerializer.Deserialize<ConditionInvisibleState>(json);

        if (state is null)
            throw new InvalidOperationException("Failed to deserialize invisible condition: JSON was null.");

        // Restore returns null when the condition has expired (RemainingTimeMilliseconds <= 0).
        // The caller (ConditionListParser) skips null entries, so expired invisible records
        // are silently ignored rather than failing player materialization.
        return ConditionInvisible.Restore(state);
    }

    public static bool CanHandle(ConditionType type)
    {
        return type is ConditionType.Invisible;
    }
}
