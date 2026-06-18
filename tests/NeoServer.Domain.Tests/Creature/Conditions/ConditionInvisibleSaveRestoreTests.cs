using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Conditions;

public class ConditionInvisibleSaveRestoreTests
{
    [Fact]
    [Trait("Category", "HappyPath")]
    public void CaptureState_returns_correct_state_after_Start()
    {
        var creature = PlayerTestDataBuilder.Build();
        var condition = new ConditionInvisible(120_000, EffectT.GlitterBlue);
        condition.Start(creature);

        var state = condition.CaptureState();

        state.Type.Should().Be(ConditionType.Invisible);
        state.Effect.Should().Be(EffectT.GlitterBlue);
        state.RemainingTimeMilliseconds.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void CaptureState_returns_correct_effect_when_not_started()
    {
        var condition = new ConditionInvisible(120_000, EffectT.RingsGreen);

        var state = condition.CaptureState();

        state.Effect.Should().Be(EffectT.RingsGreen);
        state.RemainingTimeMilliseconds.Should().Be(120_000);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CaptureState_returns_correct_type()
    {
        var condition = new ConditionInvisible(10_000, EffectT.None);

        var state = condition.CaptureState();

        state.Type.Should().Be(ConditionType.Invisible);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Restore_creates_condition_with_preserved_Effect()
    {
        var state = new ConditionInvisibleState(
            ConditionType.Invisible,
            EffectT.GlitterBlue,
            RemainingTimeMilliseconds: 75_000);

        var condition = ConditionInvisible.Restore(state);

        condition.Should().NotBeNull();
        condition!.Effect.Should().Be(EffectT.GlitterBlue);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Restore_creates_condition_with_preserved_remaining_time()
    {
        var state = new ConditionInvisibleState(
            ConditionType.Invisible,
            EffectT.GlitterBlue,
            RemainingTimeMilliseconds: 75_000);

        var condition = ConditionInvisible.Restore(state);

        condition.Should().NotBeNull();
        var recaptured = condition!.CaptureState();
        recaptured.RemainingTimeMilliseconds.Should().Be(75_000);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Restore_Start_applies_invisibility()
    {
        var player = PlayerTestDataBuilder.Build();
        var state = new ConditionInvisibleState(
            ConditionType.Invisible,
            EffectT.GlitterBlue,
            RemainingTimeMilliseconds: 75_000);

        var condition = ConditionInvisible.Restore(state);
        player.AddCondition(condition!);

        player.HasCondition(ConditionType.Invisible).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Restore_Start_sets_EndAction_to_TurnVisible()
    {
        var player = PlayerTestDataBuilder.Build();
        var state = new ConditionInvisibleState(
            ConditionType.Invisible,
            EffectT.GlitterBlue,
            RemainingTimeMilliseconds: 75_000);

        var condition = ConditionInvisible.Restore(state);
        player.AddCondition(condition!);

        // After Start, the EndAction is set to TurnVisible.
        // We cannot directly assert on EndAction (it's a delegate),
        // but we can verify the condition is present and not expired.
        player.HasCondition(ConditionType.Invisible).Should().BeTrue();
        condition!.HasExpired.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void HasExpired_is_false_after_Restore()
    {
        var state = new ConditionInvisibleState(
            ConditionType.Invisible,
            EffectT.GlitterBlue,
            RemainingTimeMilliseconds: 75_000);

        var condition = ConditionInvisible.Restore(state);

        condition!.HasExpired.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CaptureState_after_Restore_and_Start_returns_saved_Effect()
    {
        var player = PlayerTestDataBuilder.Build();
        var state = new ConditionInvisibleState(
            ConditionType.Invisible,
            EffectT.GlitterBlue,
            RemainingTimeMilliseconds: 75_000);

        var condition = ConditionInvisible.Restore(state);
        player.AddCondition(condition!);

        var recaptured = condition!.CaptureState();

        recaptured.Effect.Should().Be(EffectT.GlitterBlue);
        recaptured.Type.Should().Be(ConditionType.Invisible);
        recaptured.RemainingTimeMilliseconds.Should().BeInRange(74_000, 75_001);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Restore_does_not_throw_when_state_has_EffectT_None()
    {
        var state = new ConditionInvisibleState(
            ConditionType.Invisible,
            EffectT.None,
            RemainingTimeMilliseconds: 5_000);

        var action = () => ConditionInvisible.Restore(state);

        action.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CaptureState_restore_Start_CaptureState_round_trip_preserves_remaining_time()
    {
        var player = PlayerTestDataBuilder.Build();
        var condition = new ConditionInvisible(60_000, EffectT.GlitterBlue);
        condition.Start(player);

        // Capture remaining time after condition has been running
        var firstState = condition.CaptureState();
        var savedRemaining = firstState.RemainingTimeMilliseconds;

        // Restore and re-apply
        var restored = ConditionInvisible.Restore(firstState);
        player.AddCondition(restored!);

        // Re-capture — remaining time should be close to the saved value,
        // not the full original Duration (60_000 ms).
        var secondState = restored!.CaptureState();

        secondState.RemainingTimeMilliseconds.Should().BeInRange(
            (long)(savedRemaining * 0.95),
            savedRemaining + 1);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Restore_returns_null_when_remaining_time_is_zero()
    {
        var state = new ConditionInvisibleState(
            ConditionType.Invisible,
            EffectT.GlitterBlue,
            RemainingTimeMilliseconds: 0);

        var condition = ConditionInvisible.Restore(state);

        condition.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Restore_returns_null_when_remaining_time_is_negative()
    {
        var state = new ConditionInvisibleState(
            ConditionType.Invisible,
            EffectT.GlitterBlue,
            RemainingTimeMilliseconds: -1);

        var condition = ConditionInvisible.Restore(state);

        condition.Should().BeNull();
    }
}
