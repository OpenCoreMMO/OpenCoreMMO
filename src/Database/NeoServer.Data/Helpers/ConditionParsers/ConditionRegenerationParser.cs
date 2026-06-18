#nullable enable
using System;
using System.Text.Json;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;

namespace NeoServer.Data.Helpers.ConditionParsers;

public static class ConditionRegenerationParser
{
    public static string Serialize(ConditionRegeneration condition)
    {
        ArgumentNullException.ThrowIfNull(condition);

        var state = condition.CaptureState();
        return JsonSerializer.Serialize(state);
    }

    public static ConditionRegeneration? Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var state = JsonSerializer.Deserialize<ConditionRegenerationState>(json);

        if (state is null)
            throw new InvalidOperationException("Failed to deserialize regeneration condition: JSON was null.");

        return ConditionRegeneration.Restore(state);
    }

    public static bool CanHandle(ConditionType type)
    {
        return type is ConditionType.Regeneration;
    }
}
