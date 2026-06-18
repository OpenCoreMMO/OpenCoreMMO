using System;
using System.Text.Json;
using FluentAssertions;
using NeoServer.Data.Helpers.ConditionParsers;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using Xunit;

namespace NeoServer.Server.Tests.Data;

public class ConditionInvisibleParserTest
{
    #region CanHandle

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CanHandle_returns_true_for_invisible()
    {
        ConditionInvisibleParser.CanHandle(ConditionType.Invisible).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void CanHandle_returns_false_for_non_invisible_condition_types()
    {
        var nonInvisibleTypes = new[]
        {
            ConditionType.None,
            ConditionType.Poisoned,
            ConditionType.Burning,
            ConditionType.Haste,
            ConditionType.Paralyze,
            ConditionType.Outfit,
            ConditionType.Light,
            ConditionType.ManaShield,
            ConditionType.LogoutBlock,
            ConditionType.Drunk,
            ConditionType.Regeneration,
            ConditionType.Pacified,
            ConditionType.Hungry
        };

        foreach (var type in nonInvisibleTypes)
            ConditionInvisibleParser.CanHandle(type).Should().BeFalse($"because {type} is not Invisible");
    }

    #endregion

    #region Serialization

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Serialize_creates_valid_json_with_all_state_fields()
    {
        // Arrange - build a condition with known state via Restore
        var state = new ConditionInvisibleState(
            ConditionType.Invisible,
            EffectT.GlitterBlue,
            RemainingTimeMilliseconds: 75_000);

        var condition = ConditionInvisible.Restore(state);

        // Act
        var json = ConditionInvisibleParser.Serialize(condition!);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("Type").GetUInt32().Should().Be((uint)ConditionType.Invisible);
        doc.RootElement.GetProperty("Effect").GetUInt32().Should().Be((byte)EffectT.GlitterBlue);
        doc.RootElement.GetProperty("RemainingTimeMilliseconds").GetInt64().Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Serialize_uses_remaining_time_not_full_duration()
    {
        // Arrange
        var state = new ConditionInvisibleState(
            ConditionType.Invisible,
            EffectT.GlitterBlue,
            RemainingTimeMilliseconds: 45_000);

        var condition = ConditionInvisible.Restore(state);

        // Act
        var json = ConditionInvisibleParser.Serialize(condition!);

        // Assert — serialized RemainingTimeMilliseconds should be close to 45_000, not 120_000
        using var doc = JsonDocument.Parse(json);
        var remaining = doc.RootElement.GetProperty("RemainingTimeMilliseconds").GetInt64();
        remaining.Should().BeInRange(44_000, 45_001);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Serialize_handles_effect_none()
    {
        // Arrange — EffectT.None = 255
        var state = new ConditionInvisibleState(
            ConditionType.Invisible,
            EffectT.None,
            RemainingTimeMilliseconds: 30_000);

        var condition = ConditionInvisible.Restore(state);

        // Act
        var json = ConditionInvisibleParser.Serialize(condition!);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("Effect").GetUInt32().Should().Be((byte)EffectT.None);
    }

    #endregion

    #region Deserialization

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Deserialize_reconstructs_invisible_condition()
    {
        // Arrange
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Invisible}},
                "Effect": {{(byte)EffectT.GlitterBlue}},
                "RemainingTimeMilliseconds": 75000
            }
            """;

        // Act
        var condition = ConditionInvisibleParser.Deserialize(json);

        // Assert
        condition!.Type.Should().Be(ConditionType.Invisible);

        var captured = condition!.CaptureState();
        captured.Effect.Should().Be(EffectT.GlitterBlue);
        captured.RemainingTimeMilliseconds.Should().Be(75000);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Deserialize_reconstructs_invisible_condition_with_different_values()
    {
        // Arrange
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Invisible}},
                "Effect": {{(byte)EffectT.RingsGreen}},
                "RemainingTimeMilliseconds": 15000
            }
            """;

        // Act
        var condition = ConditionInvisibleParser.Deserialize(json);

        // Assert
        condition!.Type.Should().Be(ConditionType.Invisible);
        var captured = condition!.CaptureState();
        captured.Effect.Should().Be(EffectT.RingsGreen);
        captured.RemainingTimeMilliseconds.Should().Be(15000);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Deserialize_returns_null_for_expired_invisible()
    {
        // Arrange — expired condition with zero remaining time.
        // The parser must not throw; it should silently skip expired
        // records so player materialisation does not fail.
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Invisible}},
                "Effect": {{(byte)EffectT.GlitterBlue}},
                "RemainingTimeMilliseconds": 0
            }
            """;

        // Act
        var condition = ConditionInvisibleParser.Deserialize(json);

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
                "Type": {{(uint)ConditionType.Invisible}},
                "Effect": {{(byte)EffectT.GlitterBlue}},
                "RemainingTimeMilliseconds": -1
            }
            """;

        // Act
        var condition = ConditionInvisibleParser.Deserialize(json);

        // Assert
        condition.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_null_json()
    {
        // Act
        Action act = () => ConditionInvisibleParser.Deserialize(null);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_empty_json()
    {
        // Act
        Action act = () => ConditionInvisibleParser.Deserialize(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_whitespace_json()
    {
        // Act
        Action act = () => ConditionInvisibleParser.Deserialize("   ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_malformed_json()
    {
        // Act
        Action act = () => ConditionInvisibleParser.Deserialize("{{{{{invalid}}}}");

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Deserialize_does_not_throw_when_type_field_is_missing()
    {
        // Arrange — JSON without a Type property; the state record defaults to
        // None, but ConditionInvisible.Type is always Invisible at runtime
        var json = $$"""
            {
                "Effect": {{(byte)EffectT.GlitterBlue}},
                "RemainingTimeMilliseconds": 10000
            }
            """;

        // Act
        var condition = ConditionInvisibleParser.Deserialize(json);

        // Assert
        condition!.Type.Should().Be(ConditionType.Invisible);
    }

    #endregion

    #region Round-Trip

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Round_trip_preserves_all_state_for_invisible_condition()
    {
        // Arrange
        var originalState = new ConditionInvisibleState(
            ConditionType.Invisible,
            EffectT.GlitterBlue,
            RemainingTimeMilliseconds: 45_000);

        var condition = ConditionInvisible.Restore(originalState);

        // Act
        var json = ConditionInvisibleParser.Serialize(condition!);
        var restored = ConditionInvisibleParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        // Assert
        restoredState.Type.Should().Be(originalState.Type);
        restoredState.Effect.Should().Be(originalState.Effect);
        restoredState.RemainingTimeMilliseconds.Should().Be(originalState.RemainingTimeMilliseconds);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Round_trip_preserves_effect()
    {
        // Arrange
        var originalState = new ConditionInvisibleState(
            ConditionType.Invisible,
            EffectT.RingsBlue,
            RemainingTimeMilliseconds: 30_000);

        var condition = ConditionInvisible.Restore(originalState);

        // Act
        var json = ConditionInvisibleParser.Serialize(condition!);
        var restored = ConditionInvisibleParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        // Assert
        restoredState.Effect.Should().Be(EffectT.RingsBlue);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Round_trip_preserves_remaining_time_with_small_tolerance()
    {
        // Arrange
        var state = new ConditionInvisibleState(
            ConditionType.Invisible,
            EffectT.GlitterBlue,
            RemainingTimeMilliseconds: 4950);

        var condition = ConditionInvisible.Restore(state);

        // Capture the expected value immediately after Restore, before any
        // serialization overhead adds wall-clock drift.
        var expectedRemaining = condition!.CaptureState().RemainingTimeMilliseconds;

        // Act
        var json = ConditionInvisibleParser.Serialize(condition);
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
    public void Round_trip_preserves_minimal_remaining_time()
    {
        // Arrange — minimal positive remaining time (1ms)
        var state = new ConditionInvisibleState(
            ConditionType.Invisible,
            EffectT.GlitterBlue,
            RemainingTimeMilliseconds: 1);

        var condition = ConditionInvisible.Restore(state);

        // Act
        var json = ConditionInvisibleParser.Serialize(condition!);
        var restored = ConditionInvisibleParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        // Assert
        restoredState.RemainingTimeMilliseconds.Should().Be(1);
        restoredState.Effect.Should().Be(EffectT.GlitterBlue);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Round_trip_preserves_max_remaining_time()
    {
        // Arrange — use a large remaining time
        var state = new ConditionInvisibleState(
            ConditionType.Invisible,
            EffectT.GlitterBlue,
            RemainingTimeMilliseconds: int.MaxValue);

        var condition = ConditionInvisible.Restore(state);

        // Act
        var json = ConditionInvisibleParser.Serialize(condition!);
        var restored = ConditionInvisibleParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        // Assert
        restoredState.RemainingTimeMilliseconds.Should().Be(int.MaxValue);
        restoredState.Effect.Should().Be(EffectT.GlitterBlue);
    }

    #endregion

    #region Validation

    [Fact]
    [Trait("Category", "Validation")]
    public void Serialize_throws_on_null_condition()
    {
        // Act
        Action act = () => ConditionInvisibleParser.Serialize(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}
