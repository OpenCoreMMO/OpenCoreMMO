using System;
using System.Text.Json;
using FluentAssertions;
using NeoServer.Data.Helpers.ConditionParsers;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using Xunit;

namespace NeoServer.Server.Tests.Data;

public class HasteConditionParserTest
{
    #region CanHandle

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CanHandle_returns_true_for_haste()
    {
        HasteConditionParser.CanHandle(ConditionType.Haste).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void CanHandle_returns_false_for_non_haste_condition_types()
    {
        var nonHasteTypes = new[]
        {
            ConditionType.None,
            ConditionType.Poisoned,
            ConditionType.Burning,
            ConditionType.Paralyze,
            ConditionType.Outfit,
            ConditionType.Invisible,
            ConditionType.Light,
            ConditionType.ManaShield,
            ConditionType.LogoutBlock,
            ConditionType.Drunk,
            ConditionType.Regeneration,
            ConditionType.Pacified,
            ConditionType.Hungry
        };

        foreach (var type in nonHasteTypes)
            HasteConditionParser.CanHandle(type).Should().BeFalse($"because {type} is not Haste");
    }

    #endregion

    #region Serialization

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Serialize_creates_valid_json_with_all_state_fields()
    {
        // Arrange - build a condition with known state via Restore
        var state = new HasteConditionState(
            ConditionType.Haste,
            EffectT.RingsGreen,
            SpeedBoost: 150,
            RemainingTimeMilliseconds: 30000,
            Duration: 60000 * TimeSpan.TicksPerMillisecond,
            new FormulaValues
            {
                FormulaType = FormulaType.Damage,
                MinA = 1.0,
                MinB = 50,
                MaxA = 1.0,
                MaxB = 150
            });

        var condition = HasteCondition.Restore(state);

        // Act
        var json = HasteConditionParser.Serialize(condition);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("Type").GetUInt32().Should().Be((uint)ConditionType.Haste);
        doc.RootElement.GetProperty("Effect").GetUInt32().Should().Be((byte)EffectT.RingsGreen);
        doc.RootElement.GetProperty("SpeedBoost").GetUInt32().Should().Be(150u);
        doc.RootElement.GetProperty("RemainingTimeMilliseconds").GetInt64().Should().BeGreaterThan(0);
        doc.RootElement.GetProperty("Duration").GetInt64().Should().Be(30000 * TimeSpan.TicksPerMillisecond);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Serialize_contains_formula_values()
    {
        // Arrange
        var state = new HasteConditionState(
            ConditionType.Haste,
            EffectT.None,
            SpeedBoost: 100,
            RemainingTimeMilliseconds: 10000,
            Duration: 30000 * TimeSpan.TicksPerMillisecond,
            new FormulaValues
            {
                FormulaType = FormulaType.Skill,
                MinA = 0.8,
                MinB = 10,
                MaxA = 1.2,
                MaxB = 20
            });

        var condition = HasteCondition.Restore(state);

        // Act
        var json = HasteConditionParser.Serialize(condition);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("\"MinA\":0.8");
        json.Should().Contain("\"MinB\":10");
        json.Should().Contain("\"MaxA\":1.2");
        json.Should().Contain("\"MaxB\":20");
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Serialize_handles_zero_speed_boost()
    {
        // Arrange
        var state = new HasteConditionState(
            ConditionType.Haste,
            EffectT.None,
            SpeedBoost: 0,
            RemainingTimeMilliseconds: 5000,
            Duration: 30000 * TimeSpan.TicksPerMillisecond,
            new FormulaValues
            {
                FormulaType = FormulaType.None,
                MinA = 0,
                MinB = 0,
                MaxA = 0,
                MaxB = 0
            });

        var condition = HasteCondition.Restore(state);

        // Act
        var json = HasteConditionParser.Serialize(condition);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("SpeedBoost").GetUInt32().Should().Be(0u);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Restore_returns_null_when_remaining_time_is_zero()
    {
        // Arrange - when RemainingTimeMilliseconds is 0, Restore returns null
        var state = new HasteConditionState(
            ConditionType.Haste,
            EffectT.None,
            SpeedBoost: 75,
            RemainingTimeMilliseconds: 0,
            Duration: 0,
            new FormulaValues());

        // Act
        var condition = HasteCondition.Restore(state);

        // Assert
        condition.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Serialize_preserves_default_effect_as_none()
    {
        // Arrange
        var state = new HasteConditionState(
            ConditionType.Haste,
            EffectT.None,
            SpeedBoost: 100,
            RemainingTimeMilliseconds: 15000,
            Duration: 30000 * TimeSpan.TicksPerMillisecond,
            new FormulaValues());

        var condition = HasteCondition.Restore(state);

        // Act
        var json = HasteConditionParser.Serialize(condition);

        // Assert
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("Effect").GetUInt32().Should().Be((byte)EffectT.None);
    }

    #endregion

    #region Deserialization

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Deserialize_reconstructs_haste_condition()
    {
        // Arrange
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Haste}},
                "Effect": {{(byte)EffectT.RingsGreen}},
                "SpeedBoost": 150,
                "RemainingTimeMilliseconds": 30000,
                "Duration": {{60000 * TimeSpan.TicksPerMillisecond}},
                "FormulaValues": {
                    "FormulaType": {{(int)FormulaType.Damage}},
                    "MinA": 1.0,
                    "MinB": 50,
                    "MaxA": 1.0,
                    "MaxB": 150
                }
            }
            """;

        // Act
        var condition = HasteConditionParser.Deserialize(json);

        // Assert
        condition.Type.Should().Be(ConditionType.Haste);
        condition.Effect.Should().Be(EffectT.RingsGreen);

        var captured = condition.CaptureState();
        captured.SpeedBoost.Should().Be(150);
        captured.RemainingTimeMilliseconds.Should().Be(30000);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Deserialize_reconstructs_haste_condition_with_default_formula()
    {
        // Arrange
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Haste}},
                "Effect": 0,
                "SpeedBoost": 100,
                "RemainingTimeMilliseconds": 10000,
                "Duration": 300000000,
                "FormulaValues": {
                    "FormulaType": 0,
                    "MinA": 0,
                    "MinB": 0,
                    "MaxA": 0,
                    "MaxB": 0
                }
            }
            """;

        // Act
        var condition = HasteConditionParser.Deserialize(json);

        // Assert
        condition.Type.Should().Be(ConditionType.Haste);
        var captured = condition.CaptureState();
        captured.SpeedBoost.Should().Be(100);
        captured.FormulaValues.FormulaType.Should().Be(FormulaType.None);
        captured.FormulaValues.MinA.Should().Be(0);
        captured.FormulaValues.MaxB.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Deserialize_returns_null_for_expired_haste()
    {
        // Arrange - expired condition with zero remaining time.
        // The parser must not throw; it should silently skip expired
        // records so player materialisation does not fail.
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Haste}},
                "Effect": 0,
                "SpeedBoost": 50,
                "RemainingTimeMilliseconds": 0,
                "Duration": {{10000 * TimeSpan.TicksPerMillisecond}},
                "FormulaValues": {
                    "FormulaType": 0,
                    "MinA": 0,
                    "MinB": 0,
                    "MaxA": 0,
                    "MaxB": 0
                }
            }
            """;

        // Act
        var condition = HasteConditionParser.Deserialize(json);

        // Assert
        condition.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Deserialize_with_none_effect_reconstructs_correctly()
    {
        // Arrange - EffectT.None = 255
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Haste}},
                "Effect": 255,
                "SpeedBoost": 200,
                "RemainingTimeMilliseconds": 5000,
                "Duration": {{30000 * TimeSpan.TicksPerMillisecond}},
                "FormulaValues": {
                    "FormulaType": 0,
                    "MinA": 0,
                    "MinB": 0,
                    "MaxA": 0,
                    "MaxB": 0
                }
            }
            """;

        // Act
        var condition = HasteConditionParser.Deserialize(json);

        // Assert
        condition.Effect.Should().Be(EffectT.None);
        var captured = condition.CaptureState();
        captured.SpeedBoost.Should().Be(200);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_null_json()
    {
        // Act
        Action act = () => HasteConditionParser.Deserialize(null);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_empty_json()
    {
        // Act
        Action act = () => HasteConditionParser.Deserialize(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_whitespace_json()
    {
        // Act
        Action act = () => HasteConditionParser.Deserialize("   ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_malformed_json()
    {
        // Act
        Action act = () => HasteConditionParser.Deserialize("{{{{{invalid}}}}");

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_does_not_throw_when_type_field_is_missing()
    {
        // Arrange — JSON without a Type property; the state record defaults to
        // None, but HasteCondition.Type is always Haste at runtime
        var json = $$"""
            {
                "Effect": 0,
                "SpeedBoost": 100,
                "RemainingTimeMilliseconds": 10000,
                "Duration": 300000000
            }
            """;

        // Act
        var condition = HasteConditionParser.Deserialize(json);

        // Assert
        condition.Type.Should().Be(ConditionType.Haste);
    }

    #endregion

    #region Round-Trip

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Round_trip_preserves_all_state_for_haste_condition()
    {
        // Arrange
        var originalState = new HasteConditionState(
            ConditionType.Haste,
            EffectT.SparkYellow,
            SpeedBoost: 180,
            RemainingTimeMilliseconds: 45000,
            Duration: 60000 * TimeSpan.TicksPerMillisecond,
            new FormulaValues
            {
                FormulaType = FormulaType.Damage,
                MinA = 1.0,
                MinB = 60,
                MaxA = 1.0,
                MaxB = 180
            });

        var condition = HasteCondition.Restore(originalState);

        // Act
        var json = HasteConditionParser.Serialize(condition);
        var restored = HasteConditionParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        // Assert
        restoredState.Type.Should().Be(originalState.Type);
        restoredState.Effect.Should().Be(originalState.Effect);
        restoredState.SpeedBoost.Should().Be(originalState.SpeedBoost);
        restoredState.RemainingTimeMilliseconds.Should().Be(originalState.RemainingTimeMilliseconds);
        restoredState.FormulaValues.FormulaType.Should().Be(originalState.FormulaValues.FormulaType);
        restoredState.FormulaValues.MinA.Should().Be(originalState.FormulaValues.MinA);
        restoredState.FormulaValues.MinB.Should().Be(originalState.FormulaValues.MinB);
        restoredState.FormulaValues.MaxA.Should().Be(originalState.FormulaValues.MaxA);
        restoredState.FormulaValues.MaxB.Should().Be(originalState.FormulaValues.MaxB);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Round_trip_preserves_non_default_effect()
    {
        // Arrange
        var originalState = new HasteConditionState(
            ConditionType.Haste,
            EffectT.RingsBlue,
            SpeedBoost: 120,
            RemainingTimeMilliseconds: 20000,
            Duration: 40000 * TimeSpan.TicksPerMillisecond,
            new FormulaValues
            {
                FormulaType = FormulaType.None,
                MinA = 0,
                MinB = 0,
                MaxA = 0,
                MaxB = 0
            });

        var condition = HasteCondition.Restore(originalState);

        // Act
        var json = HasteConditionParser.Serialize(condition);
        var restored = HasteConditionParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        // Assert
        restoredState.Effect.Should().Be(EffectT.RingsBlue);
        restoredState.SpeedBoost.Should().Be(120);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Round_trip_preserves_zero_speed_boost()
    {
        // Arrange
        var state = new HasteConditionState(
            ConditionType.Haste,
            EffectT.None,
            SpeedBoost: 0,
            RemainingTimeMilliseconds: 10000,
            Duration: 30000 * TimeSpan.TicksPerMillisecond,
            new FormulaValues());

        var condition = HasteCondition.Restore(state);

        // Act
        var json = HasteConditionParser.Serialize(condition);
        var restored = HasteConditionParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        // Assert
        restoredState.SpeedBoost.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Round_trip_preserves_formula_values_with_precision()
    {
        // Arrange
        var state = new HasteConditionState(
            ConditionType.Haste,
            EffectT.None,
            SpeedBoost: 90,
            RemainingTimeMilliseconds: 25000,
            Duration: 50000 * TimeSpan.TicksPerMillisecond,
            new FormulaValues
            {
                FormulaType = FormulaType.MagicLevel,
                MinA = 0.75,
                MinB = 15,
                MaxA = 1.5,
                MaxB = 35
            });

        var condition = HasteCondition.Restore(state);

        // Act
        var json = HasteConditionParser.Serialize(condition);
        var restored = HasteConditionParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        // Assert
        restoredState.FormulaValues.FormulaType.Should().Be(FormulaType.MagicLevel);
        restoredState.FormulaValues.MinA.Should().BeApproximately(0.75, 0.001);
        restoredState.FormulaValues.MinB.Should().Be(15);
        restoredState.FormulaValues.MaxA.Should().BeApproximately(1.5, 0.001);
        restoredState.FormulaValues.MaxB.Should().Be(35);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Round_trip_preserves_remaining_time_with_small_tolerance()
    {
        // Arrange
        var state = new HasteConditionState(
            ConditionType.Haste,
            EffectT.None,
            SpeedBoost: 100,
            RemainingTimeMilliseconds: 4950, // slightly consumed
            Duration: 10000 * TimeSpan.TicksPerMillisecond,
            new FormulaValues());

        var condition = HasteCondition.Restore(state);

        // Capture the expected value immediately after Restore, before any
        // serialization overhead adds wall-clock drift.
        var expectedRemaining = condition.CaptureState().RemainingTimeMilliseconds;

        // Act
        var json = HasteConditionParser.Serialize(condition);
        using var doc = JsonDocument.Parse(json);
        var serializedRemaining = doc.RootElement.GetProperty("RemainingTimeMilliseconds").GetInt64();

        // Assert — the serialized value should match the captured value
        // within a generous tolerance. Both are read from the same live
        // object, so the gap is only the time between CaptureState() and
        // JsonSerializer.Serialize() — well under 50ms.
        serializedRemaining.Should().BeCloseTo(expectedRemaining, 50);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Round_trip_preserves_max_remaining_time()
    {
        // Arrange — use a large remaining time
        var state = new HasteConditionState(
            ConditionType.Haste,
            EffectT.None,
            SpeedBoost: 250,
            RemainingTimeMilliseconds: int.MaxValue,
            Duration: uint.MaxValue * TimeSpan.TicksPerMillisecond,
            new FormulaValues());

        var condition = HasteCondition.Restore(state);

        // Act
        var json = HasteConditionParser.Serialize(condition);
        var restored = HasteConditionParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        // Assert
        restoredState.RemainingTimeMilliseconds.Should().Be(int.MaxValue);
        restoredState.SpeedBoost.Should().Be(250);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Round_trip_preserves_minimal_remaining_time()
    {
        // Arrange — minimal positive remaining time (1ms)
        var state = new HasteConditionState(
            ConditionType.Haste,
            EffectT.None,
            SpeedBoost: 5,
            RemainingTimeMilliseconds: 1,
            Duration: 1000 * TimeSpan.TicksPerMillisecond,
            new FormulaValues());

        var condition = HasteCondition.Restore(state);

        // Act
        var json = HasteConditionParser.Serialize(condition);
        var restored = HasteConditionParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        // Assert
        restoredState.RemainingTimeMilliseconds.Should().Be(1);
        restoredState.SpeedBoost.Should().Be(5);
    }

    #endregion

    #region Validation

    [Fact]
    [Trait("Category", "Validation")]
    public void Serialize_throws_on_null_condition()
    {
        // Act
        Action act = () => HasteConditionParser.Serialize(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Serialize_with_minimal_interval_produces_valid_json()
    {
        // Arrange — interval of 1ms via RemainingTimeMilliseconds = 1
        var state = new HasteConditionState(
            ConditionType.Haste,
            EffectT.None,
            SpeedBoost: 10,
            RemainingTimeMilliseconds: 1,
            Duration: 1000 * TimeSpan.TicksPerMillisecond,
            new FormulaValues());

        var condition = HasteCondition.Restore(state);

        // Act
        var json = HasteConditionParser.Serialize(condition);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("RemainingTimeMilliseconds").GetInt64().Should().Be(1);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Serialize_with_large_speed_boost_produces_valid_json()
    {
        // Arrange — speed boost at the upper bound of ushort
        var state = new HasteConditionState(
            ConditionType.Haste,
            EffectT.None,
            SpeedBoost: ushort.MaxValue,
            RemainingTimeMilliseconds: 30000,
            Duration: 60000 * TimeSpan.TicksPerMillisecond,
            new FormulaValues());

        var condition = HasteCondition.Restore(state);

        // Act
        var json = HasteConditionParser.Serialize(condition);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("SpeedBoost").GetUInt32().Should().Be(ushort.MaxValue);
    }

    #endregion
}
