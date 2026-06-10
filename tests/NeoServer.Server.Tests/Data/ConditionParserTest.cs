using System;
using System.Collections.Generic;
using NeoServer.Data.Helpers.ConditionParsers;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using Xunit;
using FluentAssertions;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Common.Combat.Structs;
using System.Reflection;
using System.Text.Json;
using NeoServer.Domain.Creatures.Conditions;

namespace NeoServer.Server.Tests.Data;

public class ConditionParserTest
{
    #region Serialization

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ConditionParser_serializes_basic_condition_to_json()
    {
        // Arrange
        var condition = new Condition(ConditionType.Burning, 5000);

        // Act
        var json = ConditionParser.Serialize(condition);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("\"Type\":2"); // Burning = 1 << 1
        json.Should().Contain("\"RemainingTime\":5000");
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ConditionParser_serializes_condition_with_formula_values()
    {
        // Arrange
        var condition = new Condition(ConditionType.Poisoned, 10000)
        {
            FormulaValues = new FormulaValues
            {
                FormulaType = FormulaType.Damage,
                MinA = 1.5,
                MinB = 10,
                MaxA = 3.0,
                MaxB = 25
            }
        };

        // Act
        var json = ConditionParser.Serialize(condition);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("\"FormulaType\":3"); // Damage = 3
        json.Should().Contain("\"MinA\":1.5");
        json.Should().Contain("\"MaxB\":25");
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ConditionParser_serializes_condition_with_parameters()
    {
        // Arrange
        var condition = new Condition(ConditionType.Haste, 30000)
        {
            Parameters = new Dictionary<ConditionParamType, uint>
            {
                { ConditionParamType.Speed, 500 },
                { ConditionParamType.Ticks, 30000 }
            }
        };

        // Act
        var json = ConditionParser.Serialize(condition);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("\"Speed\":500");
        json.Should().Contain("\"Ticks\":30000");
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ConditionParser_serializes_persistent_condition()
    {
        // Arrange
        var condition = new Condition(ConditionType.ManaShield);

        // Act
        var json = ConditionParser.Serialize(condition);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("\"RemainingTime\":0");
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ConditionParser_serializes_condition_with_multiple_parameters()
    {
        // Arrange
        var condition = new Condition(ConditionType.Regeneration, 60000)
        {
            FormulaValues = new FormulaValues
            {
                FormulaType = FormulaType.MagicLevel,
                MinA = 0.5,
                MinB = 0,
                MaxA = 2.0,
                MaxB = 0
            },
            Parameters = new Dictionary<ConditionParamType, uint>
            {
                { ConditionParamType.HealthGain, 50 },
                { ConditionParamType.HealthTicks, 3000 },
                { ConditionParamType.ManaGain, 25 },
                { ConditionParamType.ManaTicks, 3000 }
            }
        };

        // Act
        var json = ConditionParser.Serialize(condition);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("\"Type\":8192"); // Regeneration = 1 << 13
        json.Should().Contain("\"FormulaType\":1"); // MagicLevel = 1
        json.Should().Contain("\"HealthGain\":50");
        json.Should().Contain("\"ManaGain\":25");
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ConditionParser_serializes_with_different_condition_types()
    {
        // Arrange
        var types = new[]
        {
            ConditionType.Burning,
            ConditionType.Poisoned,
            ConditionType.Haste,
            ConditionType.Paralyze,
            ConditionType.Invisible,
            ConditionType.LogoutBlock,
            ConditionType.Drunk,
            ConditionType.ProtectionZoneBlock
        };

        foreach (var type in types)
        {
            // Act
            var condition = new Condition(type, 1000);
            var json = ConditionParser.Serialize(condition);

            // Assert — System.Text.Json serializes enums as their underlying integer value
            var expectedTypeValue = (uint)type;
            json.Should().Contain($"\"Type\":{expectedTypeValue}");
        }
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ConditionParser_serializes_started_condition_with_remaining_time()
    {
        // Arrange
        var condition = new Condition(ConditionType.Burning, 5000);
        var startedAt = DateTime.UtcNow.Ticks - (100 * TimeSpan.TicksPerMillisecond);
        SetStartedAt(condition, startedAt);

        // Act
        var json = ConditionParser.Serialize(condition);

        // Assert
        var doc = JsonDocument.Parse(json);
        var remainingTime = doc.RootElement.GetProperty("RemainingTime").GetUInt32();
        remainingTime.Should().BeLessThan(5000u);
        remainingTime.Should().BeGreaterThan(0);
        remainingTime.Should().BeCloseTo(4900u, 100u);
    }

    #endregion

    #region Deserialization

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ConditionParser_deserializes_json_to_condition()
    {
        // Arrange
        var original = new Condition(ConditionType.Burning, 5000);
        var json = ConditionParser.Serialize(original);

        // Act
        var result = ConditionParser.Deserialize(json);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(ConditionType.Burning);
        result.Duration.Should().Be(5000 * TimeSpan.TicksPerMillisecond);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ConditionParser_deserializes_condition_with_parameters()
    {
        // Arrange
        var original = new Condition(ConditionType.Haste, 30000)
        {
            Parameters = new Dictionary<ConditionParamType, uint>
            {
                { ConditionParamType.Speed, 500 },
                { ConditionParamType.Ticks, 30000 },
                { ConditionParamType.SubId, 1 }
            }
        };
        var json = ConditionParser.Serialize(original);

        // Act
        var result = ConditionParser.Deserialize(json);

        // Assert
        result.Parameters.Should().ContainKey(ConditionParamType.Speed);
        result.Parameters[ConditionParamType.Speed].Should().Be(500);
        result.Parameters.Should().ContainKey(ConditionParamType.Ticks);
        result.Parameters[ConditionParamType.Ticks].Should().Be(30000);
        result.Parameters.Should().ContainKey(ConditionParamType.SubId);
        result.Parameters[ConditionParamType.SubId].Should().Be(1);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ConditionParser_deserializes_condition_with_formula_values()
    {
        // Arrange
        var original = new Condition(ConditionType.Poisoned, 10000)
        {
            FormulaValues = new FormulaValues
            {
                FormulaType = FormulaType.Damage,
                MinA = 1.5,
                MinB = 10,
                MaxA = 3.0,
                MaxB = 25
            }
        };
        var json = ConditionParser.Serialize(original);

        // Act
        var result = ConditionParser.Deserialize(json);

        // Assert
        result.FormulaValues.FormulaType.Should().Be(FormulaType.Damage);
        result.FormulaValues.MinA.Should().Be(1.5);
        result.FormulaValues.MinB.Should().Be(10);
        result.FormulaValues.MaxA.Should().Be(3.0);
        result.FormulaValues.MaxB.Should().Be(25);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ConditionParser_deserializes_persistent_condition()
    {
        // Arrange
        var original = new Condition(ConditionType.ManaShield);
        var json = ConditionParser.Serialize(original);

        // Act
        var result = ConditionParser.Deserialize(json);

        // Assert
        result.Type.Should().Be(ConditionType.ManaShield);
        result.Duration.Should().Be(0);
        result.IsPersistent.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ConditionParser_deserializes_condition_with_stored_remaining_time()
    {
        // Arrange — craft JSON with a RemainingTime of 900ms
        var json = $$"""
            {
                "Type": 2,
                "RemainingTime": 900,
                "FormulaValues": {
                    "FormulaType": 0,
                    "MinA": 0,
                    "MinB": 0,
                    "MaxA": 0,
                    "MaxB": 0
                },
                "Parameters": {}
            }
            """;

        // Act
        var result = ConditionParser.Deserialize(json);

        // Assert — remaining duration should be exactly the stored 900ms
        var remainingMs = result.Duration / TimeSpan.TicksPerMillisecond;
        remainingMs.Should().Be(900u);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ConditionParser_deserializes_expired_condition_returns_zero_duration()
    {
        // Arrange — craft JSON with RemainingTime = 0
        var json = $$"""
            {
                "Type": 2,
                "RemainingTime": 0,
                "FormulaValues": {
                    "FormulaType": 0,
                    "MinA": 0,
                    "MinB": 0,
                    "MaxA": 0,
                    "MaxB": 0
                },
                "Parameters": {}
            }
            """;

        // Act
        var result = ConditionParser.Deserialize(json);

        // Assert
        result.Type.Should().Be(ConditionType.Burning);
        result.Duration.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ConditionParser_deserializes_none_type_condition()
    {
        // Arrange
        var original = new Condition(ConditionType.None, 1000);
        var json = ConditionParser.Serialize(original);

        // Act
        var result = ConditionParser.Deserialize(json);

        // Assert
        result.Type.Should().Be(ConditionType.None);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void ConditionParser_deserializes_condition_with_minimal_duration()
    {
        // Arrange
        var original = new Condition(ConditionType.Burning, 1);
        var json = ConditionParser.Serialize(original);

        // Act
        var result = ConditionParser.Deserialize(json);

        // Assert
        result.Duration.Should().Be(1 * TimeSpan.TicksPerMillisecond);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void ConditionParser_deserializes_condition_with_partially_expired_time()
    {
        // Arrange — craft JSON with RemainingTime set to 500ms
        var json = $$"""
            {
                "Type": 4,
                "RemainingTime": 500,
                "FormulaValues": {
                    "FormulaType": 0,
                    "MinA": 0,
                    "MinB": 0,
                    "MaxA": 0,
                    "MaxB": 0
                },
                "Parameters": {}
            }
            """;

        // Act
        var result = ConditionParser.Deserialize(json);

        // Assert — exactly 500ms remaining
        var remainingMs = result.Duration / TimeSpan.TicksPerMillisecond;
        remainingMs.Should().Be(500u);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void ConditionParser_deserialize_throws_on_null_json()
    {
        // Act
        Action act = () => ConditionParser.Deserialize(null);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void ConditionParser_deserialize_throws_on_empty_json()
    {
        // Act
        Action act = () => ConditionParser.Deserialize(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void ConditionParser_deserialize_throws_on_malformed_json()
    {
        // Act
        Action act = () => ConditionParser.Deserialize("{{{{{invalid}}}}");

        // Assert
        act.Should().Throw<JsonException>();
    }



    #endregion

    #region Round-Trip

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ConditionParser_round_trip_preserves_all_properties()
    {
        // Arrange
        var condition = new Condition(ConditionType.Electrified, 15000)
        {
            FormulaValues = new FormulaValues
            {
                FormulaType = FormulaType.Skill,
                MinA = 0.8,
                MinB = 5,
                MaxA = 1.2,
                MaxB = 15
            },
            Parameters = new Dictionary<ConditionParamType, uint>
            {
                { ConditionParamType.Owner, 1001 },
                { ConditionParamType.TickInterval, 2000 },
                { ConditionParamType.MinValue, 10 },
                { ConditionParamType.MaxValue, 50 },
                { ConditionParamType.StartValue, 10 }
            }
        };

        // Act
        var json = ConditionParser.Serialize(condition);
        var result = ConditionParser.Deserialize(json);

        // Assert
        result.Type.Should().Be(ConditionType.Electrified);

        // Duration: the full duration is preserved exactly (condition was not started)
        var expectedDurationTicks = 15000u * TimeSpan.TicksPerMillisecond;
        result.Duration.Should().Be(expectedDurationTicks);

        result.FormulaValues.FormulaType.Should().Be(FormulaType.Skill);
        result.FormulaValues.MinA.Should().Be(0.8);
        result.FormulaValues.MinB.Should().Be(5);
        result.FormulaValues.MaxA.Should().Be(1.2);
        result.FormulaValues.MaxB.Should().Be(15);

        result.Parameters[ConditionParamType.Owner].Should().Be(1001);
        result.Parameters[ConditionParamType.TickInterval].Should().Be(2000);
        result.Parameters[ConditionParamType.MinValue].Should().Be(10);
        result.Parameters[ConditionParamType.MaxValue].Should().Be(50);
        result.Parameters[ConditionParamType.StartValue].Should().Be(10);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ConditionParser_round_trip_preserves_empty_parameters_dictionary()
    {
        // Arrange
        var condition = new Condition(ConditionType.Bleeding, 8000)
        {
            Parameters = new Dictionary<ConditionParamType, uint>()
        };

        // Act
        var json = ConditionParser.Serialize(condition);
        var result = ConditionParser.Deserialize(json);

        // Assert
        result.Parameters.Should().NotBeNull();
        result.Parameters.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ConditionParser_round_trip_preserves_default_formula_values()
    {
        // Arrange
        var condition = new Condition(ConditionType.Drunk, 20000)
        {
            FormulaValues = new FormulaValues() // default struct
        };

        // Act
        var json = ConditionParser.Serialize(condition);
        var result = ConditionParser.Deserialize(json);

        // Assert
        result.FormulaValues.FormulaType.Should().Be(FormulaType.None);
        result.FormulaValues.MinA.Should().Be(0);
        result.FormulaValues.MinB.Should().Be(0);
        result.FormulaValues.MaxA.Should().Be(0);
        result.FormulaValues.MaxB.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ConditionParser_round_trip_adjusts_remaining_duration_for_started_condition()
    {
        // Arrange
        var condition = new Condition(ConditionType.Burning, 5000);
        var startedAt = DateTime.UtcNow.Ticks - (100 * TimeSpan.TicksPerMillisecond);
        SetStartedAt(condition, startedAt);

        // Act
        var json = ConditionParser.Serialize(condition);
        var result = ConditionParser.Deserialize(json);

        // Assert — remaining duration should be ~4900ms (5000 - ~100)
        var remainingMs = result.Duration / TimeSpan.TicksPerMillisecond;
        remainingMs.Should().BeLessThan(5000u);
        remainingMs.Should().BeGreaterThan(0);
        remainingMs.Should().BeCloseTo(4900u, 100u);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ConditionParser_round_trip_preserves_max_duration()
    {
        // Arrange
        var condition = new Condition(ConditionType.Burning, uint.MaxValue);

        // Act
        var json = ConditionParser.Serialize(condition);
        var result = ConditionParser.Deserialize(json);

        // Assert
        result.Duration.Should().Be(uint.MaxValue * TimeSpan.TicksPerMillisecond);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ConditionParser_round_trip_preserves_explicit_zero_duration()
    {
        // Arrange
        var condition = new Condition(ConditionType.Burning, 0);

        // Act
        var json = ConditionParser.Serialize(condition);
        var result = ConditionParser.Deserialize(json);

        // Assert
        result.Duration.Should().Be(0);
        result.IsPersistent.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ConditionParser_round_trip_preserves_protection_zone_block_type()
    {
        // Arrange — ProtectionZoneBlock breaks the bit-shift pattern (value = 1 << 28 + 1)
        var condition = new Condition(ConditionType.ProtectionZoneBlock, 5000);

        // Act
        var json = ConditionParser.Serialize(condition);
        var result = ConditionParser.Deserialize(json);

        // Assert
        result.Type.Should().Be(ConditionType.ProtectionZoneBlock);
    }

    #endregion

    #region Helpers

    private static void SetStartedAt(Condition condition, long ticks)
    {
        var property = typeof(BaseCondition).GetProperty(nameof(BaseCondition.StartedAt));
        property!.SetValue(condition, ticks);
    }

    #endregion
}
