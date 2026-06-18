using System;
using System.Text.Json;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;

namespace NeoServer.Data.Helpers.ConditionParsers;

public static class HasteConditionParser
{
    public static string Serialize(HasteCondition condition)
    {
        ArgumentNullException.ThrowIfNull(condition);

        var state = condition.CaptureState();
        return JsonSerializer.Serialize(state);
    }

    public static HasteCondition Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var state = JsonSerializer.Deserialize<HasteConditionState>(json);

        if (state is null)
            throw new InvalidOperationException("Failed to deserialize haste condition: JSON was null.");
        
        return HasteCondition.Restore(state);
    }

    public static bool CanHandle(ConditionType type)
    {
        return type is ConditionType.Haste;
    }
}
