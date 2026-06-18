using System;
using System.Text.Json;
using FluentAssertions;
using NeoServer.Data.Helpers.ConditionParsers;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using Xunit;

namespace NeoServer.Server.Tests.Data;

public class ParalyzeConditionParserTest
{
    #region CanHandle

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CanHandle_returns_true_for_paralyze()
    {
        ParalyzeConditionParser.CanHandle(ConditionType.Paralyze).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void CanHandle_returns_false_for_non_paralyze_condition_types()
    {
        var nonParalyzeTypes = new[]
        {
            ConditionType.None,
            ConditionType.Poisoned,
            ConditionType.Burning,
            ConditionType.Haste,
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

        foreach (var type in nonParalyzeTypes)
            ParalyzeConditionParser.CanHandle(type).Should().BeFalse($"because {type} is not Paralyze");
    }

    #endregion

    #region Serialization

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Serialize_creates_valid_json_with_all_state_fields()
    {
        // Arrange - build a condition with known state via Restore
        var state = new ParalyzeConditionState(
            ConditionType.Paralyze,
            SpeedReduction: 120,
            RemainingTimeMilliseconds: 30_000);

        var condition = ParalyzeCondition.Restore(state);

        // Act
        var json = ParalyzeConditionParser.Serialize(condition!);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("Type").GetUInt32().Should().Be((uint)ConditionType.Paralyze);
        doc.RootElement.GetProperty("SpeedReduction").GetUInt32().Should().Be(120u);
        doc.RootElement.GetProperty("RemainingTimeMilliseconds").GetInt64().Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Serialize_uses_remaining_time_not_full_duration()
    {
        // Arrange
        var state = new ParalyzeConditionState(
            ConditionType.Paralyze,
            SpeedReduction: 120,
            RemainingTimeMilliseconds: 25_000);

        var condition = ParalyzeCondition.Restore(state);

        // Act
        var json = ParalyzeConditionParser.Serialize(condition!);

        // Assert — serialized RemainingTimeMilliseconds should be close to 25_000, not 60_000
        using var doc = JsonDocument.Parse(json);
        var remaining = doc.RootElement.GetProperty("RemainingTimeMilliseconds").GetInt64();
        remaining.Should().BeInRange(24_000, 25_001);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Serialize_handles_max_speed_reduction()
    {
        // Arrange — speed reduction at the upper bound of ushort
        var state = new ParalyzeConditionState(
            ConditionType.Paralyze,
            SpeedReduction: ushort.MaxValue,
            RemainingTimeMilliseconds: 30_000);

        var condition = ParalyzeCondition.Restore(state);

        // Act
        var json = ParalyzeConditionParser.Serialize(condition!);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("SpeedReduction").GetUInt32().Should().Be(ushort.MaxValue);
    }

    #endregion

    #region Deserialization

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Deserialize_reconstructs_paralyze_condition()
    {
        // Arrange
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Paralyze}},
                "SpeedReduction": 120,
                "RemainingTimeMilliseconds": 30000
            }
            """;

        // Act
        var condition = ParalyzeConditionParser.Deserialize(json);

        // Assert
        condition!.Type.Should().Be(ConditionType.Paralyze);

        var captured = condition!.CaptureState();
        captured.SpeedReduction.Should().Be(120);
        captured.RemainingTimeMilliseconds.Should().Be(30000);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Deserialize_reconstructs_paralyze_condition_with_different_values()
    {
        // Arrange
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Paralyze}},
                "SpeedReduction": 75,
                "RemainingTimeMilliseconds": 15000
            }
            """;

        // Act
        var condition = ParalyzeConditionParser.Deserialize(json);

        // Assert
        condition!.Type.Should().Be(ConditionType.Paralyze);
        var captured = condition!.CaptureState();
        captured.SpeedReduction.Should().Be(75);
        captured.RemainingTimeMilliseconds.Should().Be(15000);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Deserialize_returns_null_for_expired_paralyze()
    {
        // Arrange — expired condition with zero remaining time.
        // The parser must not throw; it should silently skip expired
        // records so player materialisation does not fail.
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Paralyze}},
                "SpeedReduction": 120,
                "RemainingTimeMilliseconds": 0,
                "Duration": 0
            }
            """;

        // Act
        var condition = ParalyzeConditionParser.Deserialize(json);

        // Assert
        condition.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Deserialize_returns_null_for_negative_remaining_time()
    {
        // Arrange — expired condition with negative remaining time
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Paralyze}},
                "SpeedReduction": 120,
                "RemainingTimeMilliseconds": -1
            }
            """;

        // Act
        var condition = ParalyzeConditionParser.Deserialize(json);

        // Assert
        condition.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_null_json()
    {
        // Act
        Action act = () => ParalyzeConditionParser.Deserialize(null);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_empty_json()
    {
        // Act
        Action act = () => ParalyzeConditionParser.Deserialize(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_whitespace_json()
    {
        // Act
        Action act = () => ParalyzeConditionParser.Deserialize("   ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_malformed_json()
    {
        // Act
        Action act = () => ParalyzeConditionParser.Deserialize("{{{{{invalid}}}}");

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Deserialize_does_not_throw_when_type_field_is_missing()
    {
        // Arrange — JSON without a Type property; the state record defaults to
        // None, but ParalyzeCondition.Type is always Paralyze at runtime
        var json = """
            {
                "SpeedReduction": 120,
                "RemainingTimeMilliseconds": 10000
            }
            """;

        // Act
        var condition = ParalyzeConditionParser.Deserialize(json);

        // Assert
        condition!.Type.Should().Be(ConditionType.Paralyze);
    }

    #endregion

    #region Round-Trip

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Round_trip_preserves_all_state_for_paralyze_condition()
    {
        // Arrange
        var originalState = new ParalyzeConditionState(
            ConditionType.Paralyze,
            SpeedReduction: 120,
            RemainingTimeMilliseconds: 45_000);

        var condition = ParalyzeCondition.Restore(originalState);

        // Act
        var json = ParalyzeConditionParser.Serialize(condition!);
        var restored = ParalyzeConditionParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        // Assert
        restoredState.Type.Should().Be(originalState.Type);
        restoredState.SpeedReduction.Should().Be(originalState.SpeedReduction);
        restoredState.RemainingTimeMilliseconds.Should().Be(originalState.RemainingTimeMilliseconds);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Round_trip_preserves_remaining_time_with_small_tolerance()
    {
        // Arrange
        var state = new ParalyzeConditionState(
            ConditionType.Paralyze,
            SpeedReduction: 120,
            RemainingTimeMilliseconds: 4950);

        var condition = ParalyzeCondition.Restore(state);

        // Capture the expected value immediately after Restore, before any
        // serialization overhead adds wall-clock drift.
        var expectedRemaining = condition!.CaptureState().RemainingTimeMilliseconds;

        // Act
        var json = ParalyzeConditionParser.Serialize(condition);
        using var doc = JsonDocument.Parse(json);
        var serializedRemaining = doc.RootElement.GetProperty("RemainingTimeMilliseconds").GetInt64();

        // Assert — the serialized value should match the captured value
        // within a generous tolerance. Both are read from the same live
        // object, so the gap is only the time between CaptureState() and
        // JsonSerializer.Serialize() — well under 50ms.
        serializedRemaining.Should().BeCloseTo(expectedRemaining, 50);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Round_trip_preserves_speed_reduction()
    {
        // Arrange
        var originalState = new ParalyzeConditionState(
            ConditionType.Paralyze,
            SpeedReduction: 200,
            RemainingTimeMilliseconds: 30_000);

        var condition = ParalyzeCondition.Restore(originalState);

        // Act
        var json = ParalyzeConditionParser.Serialize(condition!);
        var restored = ParalyzeConditionParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        // Assert
        restoredState.SpeedReduction.Should().Be(200);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Round_trip_preserves_minimal_remaining_time()
    {
        // Arrange — minimal positive remaining time (1ms)
        var state = new ParalyzeConditionState(
            ConditionType.Paralyze,
            SpeedReduction: 50,
            RemainingTimeMilliseconds: 1);

        var condition = ParalyzeCondition.Restore(state);

        // Act
        var json = ParalyzeConditionParser.Serialize(condition!);
        var restored = ParalyzeConditionParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        // Assert
        restoredState.RemainingTimeMilliseconds.Should().Be(1);
        restoredState.SpeedReduction.Should().Be(50);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Round_trip_preserves_max_remaining_time()
    {
        // Arrange — use a large remaining time
        var state = new ParalyzeConditionState(
            ConditionType.Paralyze,
            SpeedReduction: 250,
            RemainingTimeMilliseconds: int.MaxValue);

        var condition = ParalyzeCondition.Restore(state);

        // Act
        var json = ParalyzeConditionParser.Serialize(condition!);
        var restored = ParalyzeConditionParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        // Assert
        restoredState.RemainingTimeMilliseconds.Should().Be(int.MaxValue);
        restoredState.SpeedReduction.Should().Be(250);
    }

    #endregion

    #region Validation

    [Fact]
    [Trait("Category", "Validation")]
    public void Serialize_throws_on_null_condition()
    {
        // Act
        Action act = () => ParalyzeConditionParser.Serialize(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}
