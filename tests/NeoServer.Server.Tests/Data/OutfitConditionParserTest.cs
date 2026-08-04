using System;
using System.Text.Json;
using FluentAssertions;
using NeoServer.Data.Helpers.ConditionParsers;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using Xunit;

namespace NeoServer.Server.Tests.Data;

public class OutfitConditionParserTest
{
    #region CanHandle

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CanHandle_returns_true_for_outfit()
    {
        OutfitConditionParser.CanHandle(ConditionType.Outfit).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void CanHandle_returns_false_for_non_outfit_condition_types()
    {
        var nonOutfitTypes = new[]
        {
            ConditionType.None,
            ConditionType.Poisoned,
            ConditionType.Burning,
            ConditionType.Haste,
            ConditionType.Paralyze,
            ConditionType.Invisible,
            ConditionType.Light,
            ConditionType.ManaShield,
            ConditionType.LogoutBlock,
            ConditionType.Drunk,
            ConditionType.Regeneration,
            ConditionType.Pacified,
            ConditionType.Hungry
        };

        foreach (var type in nonOutfitTypes)
            OutfitConditionParser.CanHandle(type).Should().BeFalse($"because {type} is not Outfit");
    }

    #endregion

    #region Serialization

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Serialize_creates_valid_json_with_all_state_fields()
    {
        var state = new OutfitConditionState(
            ConditionType.Outfit,
            LookType: 128,
            Head: 1,
            Body: 2,
            Legs: 3,
            Feet: 4,
            Addon: 0,
            RemainingTimeMilliseconds: 75_000);

        var condition = OutfitCondition.Restore(state);

        var json = OutfitConditionParser.Serialize(condition!);

        json.Should().NotBeNullOrWhiteSpace();

        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("Type").GetUInt32().Should().Be((uint)ConditionType.Outfit);
        doc.RootElement.GetProperty("LookType").GetUInt32().Should().Be(128);
        doc.RootElement.GetProperty("Head").GetUInt32().Should().Be(1);
        doc.RootElement.GetProperty("Body").GetUInt32().Should().Be(2);
        doc.RootElement.GetProperty("Legs").GetUInt32().Should().Be(3);
        doc.RootElement.GetProperty("Feet").GetUInt32().Should().Be(4);
        doc.RootElement.GetProperty("Addon").GetUInt32().Should().Be(0);
        doc.RootElement.GetProperty("RemainingTimeMilliseconds").GetInt64().Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Serialize_uses_remaining_time_not_full_duration()
    {
        var state = new OutfitConditionState(
            ConditionType.Outfit,
            LookType: 128,
            Head: 1,
            Body: 2,
            Legs: 3,
            Feet: 4,
            Addon: 0,
            RemainingTimeMilliseconds: 45_000);

        var condition = OutfitCondition.Restore(state);

        var json = OutfitConditionParser.Serialize(condition!);

        using var doc = JsonDocument.Parse(json);
        var remaining = doc.RootElement.GetProperty("RemainingTimeMilliseconds").GetInt64();
        remaining.Should().BeInRange(44_000, 45_001);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Serialize_handles_addon_none()
    {
        var state = new OutfitConditionState(
            ConditionType.Outfit,
            LookType: 128,
            Head: 0,
            Body: 0,
            Legs: 0,
            Feet: 0,
            Addon: 0,
            RemainingTimeMilliseconds: 30_000);

        var condition = OutfitCondition.Restore(state);

        var json = OutfitConditionParser.Serialize(condition!);

        json.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("Addon").GetUInt32().Should().Be(0);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Serialize_handles_max_addon()
    {
        var state = new OutfitConditionState(
            ConditionType.Outfit,
            LookType: 128,
            Head: 0,
            Body: 0,
            Legs: 0,
            Feet: 0,
            Addon: 7,
            RemainingTimeMilliseconds: 30_000);

        var condition = OutfitCondition.Restore(state);

        var json = OutfitConditionParser.Serialize(condition!);

        json.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("Addon").GetUInt32().Should().Be(7);
    }

    #endregion

    #region Deserialization

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Deserialize_reconstructs_outfit_condition()
    {
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Outfit}},
                "LookType": 128,
                "Head": 1,
                "Body": 2,
                "Legs": 3,
                "Feet": 4,
                "Addon": 0,
                "RemainingTimeMilliseconds": 75000
            }
            """;

        var condition = OutfitConditionParser.Deserialize(json);

        condition!.Type.Should().Be(ConditionType.Outfit);

        var captured = condition!.CaptureState();
        captured.LookType.Should().Be(128);
        captured.Head.Should().Be(1);
        captured.Body.Should().Be(2);
        captured.Legs.Should().Be(3);
        captured.Feet.Should().Be(4);
        captured.Addon.Should().Be(0);
        captured.RemainingTimeMilliseconds.Should().Be(75000);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Deserialize_reconstructs_outfit_condition_with_different_values()
    {
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Outfit}},
                "LookType": 130,
                "Head": 10,
                "Body": 20,
                "Legs": 30,
                "Feet": 40,
                "Addon": 3,
                "RemainingTimeMilliseconds": 15000
            }
            """;

        var condition = OutfitConditionParser.Deserialize(json);

        condition!.Type.Should().Be(ConditionType.Outfit);
        var captured = condition!.CaptureState();
        captured.LookType.Should().Be(130);
        captured.Head.Should().Be(10);
        captured.Body.Should().Be(20);
        captured.Legs.Should().Be(30);
        captured.Feet.Should().Be(40);
        captured.Addon.Should().Be(3);
        captured.RemainingTimeMilliseconds.Should().Be(15000);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Deserialize_returns_null_for_expired_outfit()
    {
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Outfit}},
                "LookType": 128,
                "Head": 1,
                "Body": 2,
                "Legs": 3,
                "Feet": 4,
                "Addon": 0,
                "RemainingTimeMilliseconds": 0
            }
            """;

        var condition = OutfitConditionParser.Deserialize(json);

        condition.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Deserialize_returns_null_for_negative_remaining_time()
    {
        var json = $$"""
            {
                "Type": {{(uint)ConditionType.Outfit}},
                "LookType": 128,
                "Head": 1,
                "Body": 2,
                "Legs": 3,
                "Feet": 4,
                "Addon": 0,
                "RemainingTimeMilliseconds": -1
            }
            """;

        var condition = OutfitConditionParser.Deserialize(json);

        condition.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_null_json()
    {
        Action act = () => OutfitConditionParser.Deserialize(null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_empty_json()
    {
        Action act = () => OutfitConditionParser.Deserialize(string.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_whitespace_json()
    {
        Action act = () => OutfitConditionParser.Deserialize("   ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Deserialize_throws_on_malformed_json()
    {
        Action act = () => OutfitConditionParser.Deserialize("{{{{{invalid}}}}");

        act.Should().Throw<JsonException>();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Deserialize_does_not_throw_when_type_field_is_missing()
    {
        var json = $$"""
            {
                "LookType": 128,
                "Head": 1,
                "Body": 2,
                "Legs": 3,
                "Feet": 4,
                "Addon": 0,
                "RemainingTimeMilliseconds": 10000
            }
            """;

        var condition = OutfitConditionParser.Deserialize(json);

        condition!.Type.Should().Be(ConditionType.Outfit);
    }

    #endregion

    #region Round-Trip

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Round_trip_preserves_all_state_for_outfit_condition()
    {
        var originalState = new OutfitConditionState(
            ConditionType.Outfit,
            LookType: 128,
            Head: 1,
            Body: 2,
            Legs: 3,
            Feet: 4,
            Addon: 0,
            RemainingTimeMilliseconds: 45_000);

        var condition = OutfitCondition.Restore(originalState);

        var json = OutfitConditionParser.Serialize(condition!);
        var restored = OutfitConditionParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        restoredState.Type.Should().Be(originalState.Type);
        restoredState.LookType.Should().Be(originalState.LookType);
        restoredState.Head.Should().Be(originalState.Head);
        restoredState.Body.Should().Be(originalState.Body);
        restoredState.Legs.Should().Be(originalState.Legs);
        restoredState.Feet.Should().Be(originalState.Feet);
        restoredState.Addon.Should().Be(originalState.Addon);
        restoredState.RemainingTimeMilliseconds.Should().Be(originalState.RemainingTimeMilliseconds);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Round_trip_preserves_look_type_and_colors()
    {
        var originalState = new OutfitConditionState(
            ConditionType.Outfit,
            LookType: 130,
            Head: 10,
            Body: 20,
            Legs: 30,
            Feet: 40,
            Addon: 3,
            RemainingTimeMilliseconds: 30_000);

        var condition = OutfitCondition.Restore(originalState);

        var json = OutfitConditionParser.Serialize(condition!);
        var restored = OutfitConditionParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        restoredState.LookType.Should().Be(130);
        restoredState.Head.Should().Be(10);
        restoredState.Body.Should().Be(20);
        restoredState.Legs.Should().Be(30);
        restoredState.Feet.Should().Be(40);
        restoredState.Addon.Should().Be(3);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Round_trip_preserves_remaining_time_with_small_tolerance()
    {
        var state = new OutfitConditionState(
            ConditionType.Outfit,
            LookType: 128,
            Head: 1,
            Body: 2,
            Legs: 3,
            Feet: 4,
            Addon: 0,
            RemainingTimeMilliseconds: 4950);

        var condition = OutfitCondition.Restore(state);

        var expectedRemaining = condition!.CaptureState().RemainingTimeMilliseconds;

        var json = OutfitConditionParser.Serialize(condition);
        using var doc = JsonDocument.Parse(json);
        var serializedRemaining = doc.RootElement.GetProperty("RemainingTimeMilliseconds").GetInt64();

        serializedRemaining.Should().BeCloseTo(expectedRemaining, 50);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Round_trip_preserves_minimal_remaining_time()
    {
        var state = new OutfitConditionState(
            ConditionType.Outfit,
            LookType: 128,
            Head: 1,
            Body: 2,
            Legs: 3,
            Feet: 4,
            Addon: 0,
            RemainingTimeMilliseconds: 1);

        var condition = OutfitCondition.Restore(state);

        var json = OutfitConditionParser.Serialize(condition!);
        var restored = OutfitConditionParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        restoredState.RemainingTimeMilliseconds.Should().Be(1);
        restoredState.LookType.Should().Be(128);
        restoredState.Head.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Round_trip_preserves_max_remaining_time()
    {
        var state = new OutfitConditionState(
            ConditionType.Outfit,
            LookType: 128,
            Head: 1,
            Body: 2,
            Legs: 3,
            Feet: 4,
            Addon: 0,
            RemainingTimeMilliseconds: int.MaxValue);

        var condition = OutfitCondition.Restore(state);

        var json = OutfitConditionParser.Serialize(condition!);
        var restored = OutfitConditionParser.Deserialize(json);
        var restoredState = restored.CaptureState();

        restoredState.RemainingTimeMilliseconds.Should().Be(int.MaxValue);
        restoredState.LookType.Should().Be(128);
    }

    #endregion

    #region Validation

    [Fact]
    [Trait("Category", "Validation")]
    public void Serialize_throws_on_null_condition()
    {
        Action act = () => OutfitConditionParser.Serialize(null);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}
