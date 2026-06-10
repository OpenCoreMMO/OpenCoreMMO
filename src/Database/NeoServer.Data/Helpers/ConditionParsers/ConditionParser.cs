using System;
using System.Collections.Generic;
using System.Text.Json;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;

namespace NeoServer.Data.Helpers.ConditionParsers;

public static class ConditionParser
{
    public static string Serialize(Condition condition)
    {
        ArgumentNullException.ThrowIfNull(condition);
        var record = new ConditionRecord
        {
            Type = condition.Type,
            Duration = (uint)(condition.Duration / TimeSpan.TicksPerMillisecond),
            StartedAt = condition.StartedAt,
            FormulaValues = condition.FormulaValues,
            Parameters = condition.Parameters
        };

        return JsonSerializer.Serialize(record);
    }

    public static Condition Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var record = JsonSerializer.Deserialize<ConditionRecord>(json);

        if (record is null)
            throw new InvalidOperationException("Failed to deserialize condition: JSON was null.");

        var duration = record.Duration;

        // If the condition had already started, adjust the duration
        // to reflect the remaining time since serialization
        if (record.StartedAt > 0)
        {
            var elapsedTicks = DateTime.UtcNow.Ticks - record.StartedAt;

            // Guard against system clock moving backward
            if (elapsedTicks < 0) elapsedTicks = 0;

            var remainingMs =
                (record.Duration * TimeSpan.TicksPerMillisecond - elapsedTicks) / TimeSpan.TicksPerMillisecond;

            duration = remainingMs > 0 ? (uint)remainingMs : 0u;
        }

        var condition = new Condition(record.Type, duration)
        {
            FormulaValues = record.FormulaValues,
            Parameters = record.Parameters
        };

        return condition;
    }

    private sealed record ConditionRecord
    {
        public ConditionType Type { get; init; }
        public uint Duration { get; init; }
        public long StartedAt { get; init; }
        public FormulaValues FormulaValues { get; init; }
        public Dictionary<ConditionParamType, uint> Parameters { get; init; } = new();
    }
}
