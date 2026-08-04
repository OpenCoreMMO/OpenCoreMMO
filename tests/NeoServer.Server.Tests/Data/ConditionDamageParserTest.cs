using System;
using System.Text.Json;
using FluentAssertions;
using NeoServer.Data.Helpers.ConditionParsers;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using Xunit;

namespace NeoServer.Server.Tests.Data;

public class ConditionDamageParserTest
{
    #region CanHandle

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CanHandle_returns_true_for_all_damage_condition_types()
    {
        var damageTypes = new[]
        {
            ConditionType.Poisoned,
            ConditionType.Burning,
            ConditionType.Electrified,
            ConditionType.Bleeding,
            ConditionType.Freezing,
            ConditionType.Dazzled,
            ConditionType.Cursed,
            ConditionType.Drowning
        };

        foreach (var type in damageTypes)
            ConditionDamageParser.CanHandle(type).Should().BeTrue($"because {type} is a damage condition");
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void CanHandle_returns_false_for_non_damage_condition_types()
    {
        var nonDamageTypes = new[]
        {
            ConditionType.None,
            ConditionType.Haste,
            ConditionType.Paralyze,
            ConditionType.Outfit,
            ConditionType.Invisible,
            ConditionType.Light,
            ConditionType.ManaShield,
            ConditionType.LogoutBlock,
            ConditionType.Drunk,
            ConditionType.Regeneration,
            ConditionType.Soul,
            ConditionType.Muted,
            ConditionType.Pacified,
            ConditionType.Hungry
        };

        foreach (var type in nonDamageTypes)
            ConditionDamageParser.CanHandle(type).Should().BeFalse($"because {type} is not a damage condition");
    }

    #endregion

    #region Serialization

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Serialize_creates_valid_json_with_all_state_fields()
    {
        // Arrange - build a condition with known state via Restore
        var state = new ConditionDamageState(
            ConditionType.Poisoned,
            DamageType.Earth,
            EffectT.RingsGreen,
            Interval: 2000,
            RemainingCooldownMilliseconds: 1500,
            RemainingDamages: [10, 10, 10],
            Amount: 5,
            MinDamage: 10,
            MaxDamage: 10);

        var condition = ConditionDamage.Restore(state);

        // Act
        var json = ConditionDamageParser.Serialize(condition);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("Type").GetUInt32().Should().Be((uint)ConditionType.Poisoned);
        doc.RootElement.GetProperty("DamageType").GetUInt32().Should().Be((byte)DamageType.Earth);
        doc.RootElement.GetProperty("Effect").GetUInt32().Should().Be((byte)EffectT.RingsGreen);
        doc.RootElement.GetProperty("Interval").GetUInt32().Should().Be(2000u);
        doc.RootElement.GetProperty("RemainingCooldownMilliseconds").GetInt64().Should().BeGreaterThanOrEqualTo(0);
        doc.RootElement.GetProperty("Amount").GetUInt32().Should().Be(5);
        doc.RootElement.GetProperty("MinDamage").GetUInt32().Should().Be(10);
        doc.RootElement.GetProperty("MaxDamage").GetUInt32().Should().Be(10);

        var remainingDamages = doc.RootElement.GetProperty("RemainingDamages");
        remainingDamages.GetArrayLength().Should().Be(3);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Serialize_round_trips_fixed_amount_condition_correctly()
    {
        // Arrange
        var state = new ConditionDamageState(
            ConditionType.Burning,
            DamageType.Fire,
            EffectT.SparkYellow,
            Interval: 2000,
            RemainingCooldownMilliseconds: 2000,
            RemainingDamages: [25, 25, 25],
            Amount: 3,
            MinDamage: 25,
            MaxDamage: 25);

        var condition = ConditionDamage.Restore(state);

        // Act
        var json = ConditionDamageParser.Serialize(condition);
        var restored = ConditionDamageParser.Deserialize(json);

        // Assert
        restored.Type.Should().Be(ConditionType.Burning);
        restored.DamageType.Should().Be(DamageType.Fire);
        restored.Effect.Should().Be(EffectT.SparkYellow);
        restored.Interval.Should().Be(2000u);
        restored.Amount.Should().Be(3);

        var restoredState = restored.CaptureState();
        restoredState.MinDamage.Should().Be(25);
        restoredState.MaxDamage.Should().Be(25);
        restoredState.RemainingDamages.Should().HaveCount(3);
        restoredState.RemainingDamages.Should().AllSatisfy(d => d.Should().Be(25));
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Serialize_round_trips_variable_damage_condition_correctly()
    {
        // Arrange
        var state = new ConditionDamageState(
            ConditionType.Electrified,
            DamageType.Energy,
            EffectT.DamageEnergy,
            Interval: 3000,
            RemainingCooldownMilliseconds: 3000,
            RemainingDamages: [12, 30, 8, 45, 22],
            Amount: 0,
            MinDamage: 5,
            MaxDamage: 50);

        var condition = ConditionDamage.Restore(state);

        // Act
        var json = ConditionDamageParser.Serialize(condition);
        var restored = ConditionDamageParser.Deserialize(json);

        // Assert
        restored.Type.Should().Be(ConditionType.Electrified);
        restored.DamageType.Should().Be(DamageType.Energy);
        restored.Effect.Should().Be(EffectT.DamageEnergy);
        restored.Interval.Should().Be(3000u);
        var restoredState2 = restored.CaptureState();
        restoredState2.MinDamage.Should().Be(5);
        restoredState2.MaxDamage.Should().Be(50);
        restored.Amount.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Serialize_preserves_partially_consumed_damage_queue()
    {
        // Arrange - simulate a condition where 2 of 5 hits have been consumed
        var state = new ConditionDamageState(
            ConditionType.Bleeding,
            DamageType.Physical,
            EffectT.XBlood,
            Interval: 2000,
            RemainingCooldownMilliseconds: 500,
            RemainingDamages: [10, 10, 10],
            Amount: 5,
            MinDamage: 10,
            MaxDamage: 10);

        var condition = ConditionDamage.Restore(state);
        var damagesBefore = condition.CaptureState().RemainingDamages.Length;

        // Act
        var json = ConditionDamageParser.Serialize(condition);
        var restored = ConditionDamageParser.Deserialize(json);

        // Assert
        var restoredState = restored.CaptureState();
        restoredState.RemainingDamages.Should().HaveCount(damagesBefore);
        restoredState.RemainingDamages.Should().Equal(10, 10, 10);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Serialize_handles_empty_damage_queue()
    {
        // Arrange - condition with no remaining damages
        var state = new ConditionDamageState(
            ConditionType.Poisoned,
            DamageType.Earth,
            EffectT.None,
            Interval: 2000,
            RemainingCooldownMilliseconds: 0,
            RemainingDamages: [],
            Amount: 0,
            MinDamage: 0,
            MaxDamage: 0);

        var condition = ConditionDamage.Restore(state);

        // Act
        var json = ConditionDamageParser.Serialize(condition);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("RemainingDamages").GetArrayLength().Should().Be(0);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Serialize_all_damage_types_produce_valid_json()
    {
        var allDamageTypes = new[]
        {
            (ConditionType.Poisoned, DamageType.Earth, EffectT.RingsGreen),
            (ConditionType.Burning, DamageType.Fire, EffectT.Flame),
            (ConditionType.Electrified, DamageType.Energy, EffectT.DamageEnergy),
            (ConditionType.Bleeding, DamageType.Physical, EffectT.XBlood),
            (ConditionType.Freezing, DamageType.Ice, EffectT.IceAttack),
            (ConditionType.Dazzled, DamageType.Holy, EffectT.HolyDamage),
            (ConditionType.Cursed, DamageType.Death, EffectT.BubbleBlack),
            (ConditionType.Drowning, DamageType.Drown, EffectT.Watersplash)
        };

        foreach (var (type, expectedDamageType, effect) in allDamageTypes)
        {
            var state = new ConditionDamageState(
                type,
                expectedDamageType,
                effect,
                Interval: 2000,
                RemainingCooldownMilliseconds: 2000,
                RemainingDamages: [15, 15, 15],
                Amount: 3,
                MinDamage: 15,
                MaxDamage: 15);

            var condition = ConditionDamage.Restore(state);

            // Act
            var json = ConditionDamageParser.Serialize(condition);

            // Assert
            json.Should().NotBeNullOrWhiteSpace();
            json.Should().Contain($"\"Type\":{(uint)type}");
            json.Should().Contain($"\"DamageType\":{(byte)expectedDamageType}");
        }
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Serialize_preserves_cooldown_remaining_with_small_tolerance()
    {
        // Arrange
        var state = new ConditionDamageState(
            ConditionType.Poisoned,
            DamageType.Earth,
            EffectT.RingsGreen,
            Interval: 5000,
            RemainingCooldownMilliseconds: 4950, // slightly consumed
            RemainingDamages: [10, 10, 10],
            Amount: 3,
            MinDamage: 10,
            MaxDamage: 10);

        var condition = ConditionDamage.Restore(state);

        // Capture the expected value immediately after Restore, before any
        // serialization overhead adds wall-clock drift.
        var expectedCooldown = condition.CaptureState().RemainingCooldownMilliseconds;

        // Act
        var json = ConditionDamageParser.Serialize(condition);
        using var doc = JsonDocument.Parse(json);
        var serializedCooldown = doc.RootElement.GetProperty("RemainingCooldownMilliseconds").GetInt64();

        // Assert — the serialized value should match the captured value
        // within a generous tolerance. Both are read from the same live
        // object, so the gap is only the time between CaptureState() and
        // JsonSerializer.Serialize() — well under 50ms.
        serializedCooldown.Should().BeCloseTo(expectedCooldown, 50);
    }

    #endregion

    #region Deserialization

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Deserialize_reconstructs_fixed_amount_condition()
    {
        // Arrange
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Poisoned}},
                "DamageType": {{(byte)DamageType.Earth}},
                "Effect": {{(byte)EffectT.RingsGreen}},
                "Interval": 2000,
                "RemainingCooldownMilliseconds": 1500,
                "RemainingDamages": [10, 10, 10],
                "Amount": 3,
                "MinDamage": 10,
                "MaxDamage": 10
            }
            """;

        // Act
        var condition = ConditionDamageParser.Deserialize(json);

        // Assert
        condition.Type.Should().Be(ConditionType.Poisoned);
        condition.DamageType.Should().Be(DamageType.Earth);
        condition.Effect.Should().Be(EffectT.RingsGreen);
        condition.Interval.Should().Be(2000u);
        condition.Amount.Should().Be(3);
        var state3 = condition.CaptureState();
        state3.MinDamage.Should().Be(10);
        state3.MaxDamage.Should().Be(10);
        condition.HasExpired.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Deserialize_reconstructs_variable_damage_condition()
    {
        // Arrange
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Burning}},
                "DamageType": {{(byte)DamageType.Fire}},
                "Effect": {{(byte)EffectT.Flame}},
                "Interval": 3000,
                "RemainingCooldownMilliseconds": 3000,
                "RemainingDamages": [10, 10, 10, 10],
                "Amount": 0,
                "MinDamage": 5,
                "MaxDamage": 30
            }
            """;

        // Act
        var condition = ConditionDamageParser.Deserialize(json);

        // Assert
        condition.Type.Should().Be(ConditionType.Burning);
        var state4 = condition.CaptureState();
        state4.MinDamage.Should().Be(5);
        state4.MaxDamage.Should().Be(30);
        condition.Amount.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Deserialize_with_empty_damage_queue_creates_expired_condition()
    {
        // Arrange
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Poisoned}},
                "DamageType": {{(byte)DamageType.Earth}},
                "Effect": 0,
                "Interval": 2000,
                "RemainingCooldownMilliseconds": 0,
                "RemainingDamages": [],
                "Amount": 0,
                "MinDamage": 0,
                "MaxDamage": 0
            }
            """;

        // Act
        var condition = ConditionDamageParser.Deserialize(json);

        // Assert
        condition.HasExpired.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Deserialize_with_zero_cooldown_restores_correctly()
    {
        // Arrange
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Burning}},
                "DamageType": {{(byte)DamageType.Fire}},
                "Effect": 0,
                "Interval": 2000,
                "RemainingCooldownMilliseconds": 0,
                "RemainingDamages": [15, 15],
                "Amount": 2,
                "MinDamage": 15,
                "MaxDamage": 15
            }
            """;

        // Act
        var condition = ConditionDamageParser.Deserialize(json);

        // Assert
        condition.Interval.Should().Be(2000u);
        condition.HasExpired.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Deserialize_with_cooldown_exceeding_interval_does_not_throw()
    {
        // Arrange - RemainingCooldownMilliseconds exceeds Interval; gets clamped
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Poisoned}},
                "DamageType": {{(byte)DamageType.Earth}},
                "Effect": 0,
                "Interval": 2000,
                "RemainingCooldownMilliseconds": 5000,
                "RemainingDamages": [10],
                "Amount": 1,
                "MinDamage": 10,
                "MaxDamage": 10
            }
            """;

        // Act
        var condition = ConditionDamageParser.Deserialize(json);

        // Assert - should not throw; clamping to Interval happens internally
        condition.Interval.Should().Be(2000u);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_null_json()
    {
        // Act
        Action act = () => ConditionDamageParser.Deserialize(null);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_empty_json()
    {
        // Act
        Action act = () => ConditionDamageParser.Deserialize(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_whitespace_json()
    {
        // Act
        Action act = () => ConditionDamageParser.Deserialize("   ");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_malformed_json()
    {
        // Act
        Action act = () => ConditionDamageParser.Deserialize("{{{{{invalid}}}}");

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Deserialize_with_zero_interval_does_not_throw()
    {
        // Arrange
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Poisoned}},
                "DamageType": {{(byte)DamageType.Earth}},
                "Effect": 0,
                "Interval": 0,
                "RemainingCooldownMilliseconds": 0,
                "RemainingDamages": [5],
                "Amount": 1,
                "MinDamage": 5,
                "MaxDamage": 5
            }
            """;

        // Act
        var condition = ConditionDamageParser.Deserialize(json);

        // Assert
        condition.Interval.Should().Be(0u);
    }

    #endregion

    #region Round-Trip

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Round_trip_preserves_all_state_for_fixed_amount_condition()
    {
        // Arrange
        var originalState = new ConditionDamageState(
            ConditionType.Poisoned,
            DamageType.Earth,
            EffectT.RingsGreen,
            Interval: 2000,
            RemainingCooldownMilliseconds: 1500,
            RemainingDamages: [10, 10, 10, 10, 10],
            Amount: 5,
            MinDamage: 10,
            MaxDamage: 10);

        var condition = ConditionDamage.Restore(originalState);

        // Act
        var json = ConditionDamageParser.Serialize(condition);
        var restored = ConditionDamageParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        // Assert
        restoredState.Type.Should().Be(originalState.Type);
        restoredState.DamageType.Should().Be(originalState.DamageType);
        restoredState.Effect.Should().Be(originalState.Effect);
        restoredState.Interval.Should().Be(originalState.Interval);
        restoredState.Amount.Should().Be(originalState.Amount);
        restoredState.MinDamage.Should().Be(originalState.MinDamage);
        restoredState.MaxDamage.Should().Be(originalState.MaxDamage);
        restoredState.RemainingDamages.Should().Equal(originalState.RemainingDamages);
        restoredState.RemainingCooldownMilliseconds.Should().BeCloseTo(originalState.RemainingCooldownMilliseconds, 50);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Round_trip_preserves_all_state_for_variable_damage_condition()
    {
        // Arrange
        var originalState = new ConditionDamageState(
            ConditionType.Burning,
            DamageType.Fire,
            EffectT.Flame,
            Interval: 3000,
            RemainingCooldownMilliseconds: 2500,
            RemainingDamages: [12, 30, 8, 45],
            Amount: 0,
            MinDamage: 5,
            MaxDamage: 50);

        var condition = ConditionDamage.Restore(originalState);

        // Act
        var json = ConditionDamageParser.Serialize(condition);
        var restored = ConditionDamageParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        // Assert
        restoredState.Type.Should().Be(originalState.Type);
        restoredState.DamageType.Should().Be(originalState.DamageType);
        restoredState.Effect.Should().Be(originalState.Effect);
        restoredState.Interval.Should().Be(originalState.Interval);
        restoredState.Amount.Should().Be(originalState.Amount);
        restoredState.MinDamage.Should().Be(originalState.MinDamage);
        restoredState.MaxDamage.Should().Be(originalState.MaxDamage);
        restoredState.RemainingDamages.Should().Equal(originalState.RemainingDamages);
        restoredState.RemainingCooldownMilliseconds.Should().BeCloseTo(originalState.RemainingCooldownMilliseconds, 50);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Round_trip_preserves_bleeding_condition()
    {
        // Arrange - Bleeding maps to Physical damage
        var state = new ConditionDamageState(
            ConditionType.Bleeding,
            DamageType.Physical,
            EffectT.XBlood,
            Interval: 2000,
            RemainingCooldownMilliseconds: 1000,
            RemainingDamages: [8, 8, 8, 8],
            Amount: 4,
            MinDamage: 8,
            MaxDamage: 8);

        var condition = ConditionDamage.Restore(state);

        // Act
        var json = ConditionDamageParser.Serialize(condition);
        var restored = ConditionDamageParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        // Assert
        restored.DamageType.Should().Be(DamageType.Physical);
        restoredState.RemainingDamages.Should().HaveCount(4);
        restoredState.RemainingDamages.Should().AllSatisfy(d => d.Should().Be(8));
        restoredState.RemainingCooldownMilliseconds.Should().BeCloseTo(1000, 50);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Round_trip_preserves_drowning_condition()
    {
        // Arrange - Drowning maps to Drown damage
        var state = new ConditionDamageState(
            ConditionType.Drowning,
            DamageType.Drown,
            EffectT.Watersplash,
            Interval: 3000,
            RemainingCooldownMilliseconds: 2000,
            RemainingDamages: [12, 12, 12, 12, 12, 12],
            Amount: 6,
            MinDamage: 12,
            MaxDamage: 12);

        var condition = ConditionDamage.Restore(state);

        // Act
        var json = ConditionDamageParser.Serialize(condition);
        var restored = ConditionDamageParser.Deserialize(json);

        // Assert
        restored.DamageType.Should().Be(DamageType.Drown);
        restored.Effect.Should().Be(EffectT.Watersplash);
        var restoredState = restored.CaptureState();
        restoredState.RemainingDamages.Should().HaveCount(6);
        restoredState.RemainingDamages.Should().AllSatisfy(d => d.Should().Be(12));
        restoredState.RemainingCooldownMilliseconds.Should().BeCloseTo(2000, 50);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Round_trip_preserves_empty_damage_queue()
    {
        // Arrange - condition with empty queue (already expired)
        var state = new ConditionDamageState(
            ConditionType.Poisoned,
            DamageType.Earth,
            EffectT.None,
            Interval: 2000,
            RemainingCooldownMilliseconds: 0,
            RemainingDamages: [],
            Amount: 0,
            MinDamage: 0,
            MaxDamage: 0);

        var condition = ConditionDamage.Restore(state);

        // Act
        var json = ConditionDamageParser.Serialize(condition);
        var restored = ConditionDamageParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        // Assert
        restored.HasExpired.Should().BeTrue();
        restoredState.RemainingDamages.Should().BeEmpty();
        restoredState.RemainingCooldownMilliseconds.Should().Be(0);
    }

    #endregion

    #region Validation

    [Fact]
    [Trait("Category", "Validation")]
    public void Serialize_throws_on_null_condition()
    {
        // Act
        Action act = () => ConditionDamageParser.Serialize(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Serialize_with_default_effect_produces_valid_json()
    {
        // Arrange
        var state = new ConditionDamageState(
            ConditionType.Cursed,
            DamageType.Death,
            EffectT.None,
            Interval: 2000,
            RemainingCooldownMilliseconds: 2000,
            RemainingDamages: [15, 15, 15],
            Amount: 3,
            MinDamage: 15,
            MaxDamage: 15);

        var condition = ConditionDamage.Restore(state);

        // Act
        var json = ConditionDamageParser.Serialize(condition);
        using var doc = JsonDocument.Parse(json);
        var effect = doc.RootElement.GetProperty("Effect").GetUInt32();

        // Assert - EffectT.None is 255
        effect.Should().Be((byte)EffectT.None);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Serialize_with_minimal_interval_produces_valid_json()
    {
        // Arrange - interval of 1ms
        var state = new ConditionDamageState(
            ConditionType.Freezing,
            DamageType.Ice,
            EffectT.None,
            Interval: 1,
            RemainingCooldownMilliseconds: 1,
            RemainingDamages: [5],
            Amount: 1,
            MinDamage: 5,
            MaxDamage: 5);

        var condition = ConditionDamage.Restore(state);

        // Act
        var json = ConditionDamageParser.Serialize(condition);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("Interval").GetUInt32().Should().Be(1u);
    }

    #endregion
}


