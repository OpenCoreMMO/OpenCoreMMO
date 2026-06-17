using System;
using System.Collections.Generic;
using System.Text.Json;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;

namespace NeoServer.Data.Helpers.ConditionParsers;

public static class ConditionListParser
{
    public static string Serialize(IReadOnlyList<ICondition> conditions)
    {
        var serializedConditions = new List<string>();

        foreach (var condition in conditions)
        {
            if (condition.Type is ConditionType.ManaShield or ConditionType.Drunk)
            {
                serializedConditions.Add(ConditionParser.Serialize((Condition)condition));
            }
            
            if (ConditionDamageParser.CanHandle(condition.Type) && condition is ConditionDamage damageCondition)
            {
                serializedConditions.Add(ConditionDamageParser.Serialize(damageCondition));
            }
        }

        return $"[{string.Join(",", serializedConditions)}]";
    }
    
    public static List<ICondition> Deserialize(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        using var document = JsonDocument.Parse(json);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("Expected a JSON array from conditions list.", nameof(json));

        var conditions = new List<ICondition>();

        foreach (var element in document.RootElement.EnumerateArray())
        {
            if (!element.TryGetProperty("Type", out var typeProperty))
            {
                throw new Exception("Condition type not found.");
            }
            
            var conditionType = (ConditionType)typeProperty.GetUInt32();
            var conditionJson = element.GetRawText();

            if (conditionType is ConditionType.ManaShield or ConditionType.Drunk)
            {
                var condition = ConditionParser.Deserialize(conditionJson);
                conditions.Add(condition);
            }

            if (ConditionDamageParser.CanHandle(conditionType))
            {
                var condition = ConditionDamageParser.Deserialize(conditionJson);
                conditions.Add(condition);
            }
        }

        return conditions;
}
}