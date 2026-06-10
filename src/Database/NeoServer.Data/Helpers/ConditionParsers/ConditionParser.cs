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

        var remainingTime = condition.StartedAt > 0
            ? (uint)Math.Max(0, (condition.StartedAt + condition.Duration - DateTime.UtcNow.Ticks) / TimeSpan.TicksPerMillisecond)
            : (uint)(condition.Duration / TimeSpan.TicksPerMillisecond);

        var record = new ConditionRecord
        {
            Type = condition.Type,
            RemainingTime = remainingTime,
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

        var condition = new Condition(record.Type, record.RemainingTime)
        {
            FormulaValues = record.FormulaValues,
            Parameters = record.Parameters
        };

        return condition;
    }
}

public sealed record ConditionRecord
{
    public ConditionType Type { get; init; }
    public uint RemainingTime { get; init; }
    public FormulaValues FormulaValues { get; init; }
    public Dictionary<ConditionParamType, uint> Parameters { get; init; } = new();
}