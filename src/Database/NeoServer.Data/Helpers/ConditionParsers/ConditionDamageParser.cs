using System;
using System.Text.Json;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;

namespace NeoServer.Data.Helpers.ConditionParsers;

public static class ConditionDamageParser
{
    public static string Serialize(ConditionDamage condition)
    {
        ArgumentNullException.ThrowIfNull(condition);

        var state = condition.CaptureState();
        return JsonSerializer.Serialize(state);
    }

    public static ConditionDamage Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var state = JsonSerializer.Deserialize<ConditionDamageState>(json);

        if (state is null)
            throw new InvalidOperationException("Failed to deserialize condition damage: JSON was null.");

        return ConditionDamage.Restore(state);
    }

    public static bool CanHandle(ConditionType type)
    {
        return type is ConditionType.Poisoned
            or ConditionType.Burning
            or ConditionType.Electrified
            or ConditionType.Bleeding
            or ConditionType.Freezing
            or ConditionType.Dazzled
            or ConditionType.Cursed
            or ConditionType.Drowning;
    }
}
