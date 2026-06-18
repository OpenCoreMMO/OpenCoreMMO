using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Conditions;

public class OutfitConditionSaveRestoreTests
{
    [Fact]
    [Trait("Category", "HappyPath")]
    public void CaptureState_returns_correct_state_after_Start()
    {
        var creature = PlayerTestDataBuilder.Build();
        var condition = new OutfitCondition(120_000, 128, 1, 2, 3, 4, 0);
        condition.Start(creature);

        var state = condition.CaptureState();

        state.Type.Should().Be(ConditionType.Outfit);
        state.LookType.Should().Be(128);
        state.Head.Should().Be(1);
        state.Body.Should().Be(2);
        state.Legs.Should().Be(3);
        state.Feet.Should().Be(4);
        state.Addon.Should().Be(0);
        state.RemainingTimeMilliseconds.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void CaptureState_returns_correct_values_when_not_started()
    {
        var condition = new OutfitCondition(120_000, 129, 5, 6, 7, 8, 3);

        var state = condition.CaptureState();

        state.LookType.Should().Be(129);
        state.Head.Should().Be(5);
        state.Body.Should().Be(6);
        state.Legs.Should().Be(7);
        state.Feet.Should().Be(8);
        state.Addon.Should().Be(3);
        state.RemainingTimeMilliseconds.Should().Be(120_000);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CaptureState_returns_correct_type()
    {
        var condition = new OutfitCondition(10_000, 128, 0, 0, 0, 0, 0);

        var state = condition.CaptureState();

        state.Type.Should().Be(ConditionType.Outfit);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Public_properties_match_constructor_args()
    {
        var condition = new OutfitCondition(60_000, 130, 10, 20, 30, 40, 1);

        condition.LookType.Should().Be(130);
        condition.Head.Should().Be(10);
        condition.Body.Should().Be(20);
        condition.Legs.Should().Be(30);
        condition.Feet.Should().Be(40);
        condition.Addon.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Restore_creates_condition_with_preserved_outfit()
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

        condition.Should().NotBeNull();
        condition!.LookType.Should().Be(128);
        condition.Head.Should().Be(1);
        condition.Body.Should().Be(2);
        condition.Legs.Should().Be(3);
        condition.Feet.Should().Be(4);
        condition.Addon.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Restore_creates_condition_with_preserved_remaining_time()
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

        condition.Should().NotBeNull();
        var recaptured = condition!.CaptureState();
        recaptured.RemainingTimeMilliseconds.Should().Be(75_000);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Restore_Start_applies_outfit()
    {
        var player = PlayerTestDataBuilder.Build();
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
        player.AddCondition(condition!);

        player.Outfit.LookType.Should().Be(128);
        player.Outfit.Head.Should().Be(1);
        player.Outfit.Body.Should().Be(2);
        player.Outfit.Legs.Should().Be(3);
        player.Outfit.Feet.Should().Be(4);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void HasExpired_is_false_after_Restore()
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

        condition!.HasExpired.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CaptureState_after_Restore_and_Start_returns_saved_values()
    {
        var player = PlayerTestDataBuilder.Build();
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
        player.AddCondition(condition!);

        var recaptured = condition!.CaptureState();

        recaptured.LookType.Should().Be(128);
        recaptured.Head.Should().Be(1);
        recaptured.Body.Should().Be(2);
        recaptured.Legs.Should().Be(3);
        recaptured.Feet.Should().Be(4);
        recaptured.Addon.Should().Be(0);
        recaptured.Type.Should().Be(ConditionType.Outfit);
        recaptured.RemainingTimeMilliseconds.Should().BeInRange(74_000, 75_001);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Restore_does_not_throw_when_state_has_zero_addon()
    {
        var state = new OutfitConditionState(
            ConditionType.Outfit,
            LookType: 128,
            Head: 0,
            Body: 0,
            Legs: 0,
            Feet: 0,
            Addon: 0,
            RemainingTimeMilliseconds: 5_000);

        var action = () => OutfitCondition.Restore(state);

        action.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Restore_does_not_throw_when_state_has_max_addon()
    {
        var state = new OutfitConditionState(
            ConditionType.Outfit,
            LookType: 128,
            Head: 0,
            Body: 0,
            Legs: 0,
            Feet: 0,
            Addon: 7,
            RemainingTimeMilliseconds: 5_000);

        var action = () => OutfitCondition.Restore(state);

        action.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CaptureState_restore_Start_CaptureState_round_trip_preserves_remaining_time()
    {
        var player = PlayerTestDataBuilder.Build();
        var condition = new OutfitCondition(60_000, 128, 1, 2, 3, 4, 0);
        condition.Start(player);

        var firstState = condition.CaptureState();
        var savedRemaining = firstState.RemainingTimeMilliseconds;

        var restored = OutfitCondition.Restore(firstState);
        player.AddCondition(restored!);

        var secondState = restored!.CaptureState();

        secondState.RemainingTimeMilliseconds.Should().BeInRange(
            (long)(savedRemaining * 0.95),
            savedRemaining + 1);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Restore_returns_null_when_remaining_time_is_zero()
    {
        var state = new OutfitConditionState(
            ConditionType.Outfit,
            LookType: 128,
            Head: 1,
            Body: 2,
            Legs: 3,
            Feet: 4,
            Addon: 0,
            RemainingTimeMilliseconds: 0);

        var condition = OutfitCondition.Restore(state);

        condition.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Restore_returns_null_when_remaining_time_is_negative()
    {
        var state = new OutfitConditionState(
            ConditionType.Outfit,
            LookType: 128,
            Head: 1,
            Body: 2,
            Legs: 3,
            Feet: 4,
            Addon: 0,
            RemainingTimeMilliseconds: -1);

        var condition = OutfitCondition.Restore(state);

        condition.Should().BeNull();
    }
}
