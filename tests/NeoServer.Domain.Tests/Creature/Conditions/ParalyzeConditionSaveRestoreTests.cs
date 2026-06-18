using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Conditions;

public class ParalyzeConditionSaveRestoreTests
{
    [Fact]
    [Trait("Category", "HappyPath")]
    public void CaptureState_returns_correct_state_after_Start()
    {
        var creature = PlayerTestDataBuilder.Build(speed: 400);
        var condition = new ParalyzeCondition(10_000, 120);
        condition.Start(creature);

        var state = condition.CaptureState();

        state.Type.Should().Be(ConditionType.Paralyze);
        state.SpeedReduction.Should().Be(120);
        state.RemainingTimeMilliseconds.Should().BeGreaterThan(0);
        state.Duration.Should().Be(10_000 * TimeSpan.TicksPerMillisecond);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void CaptureState_returns_correct_SpeedReduction_when_not_started()
    {
        var condition = new ParalyzeCondition(10_000, 120);

        var state = condition.CaptureState();

        state.SpeedReduction.Should().Be(120);
        state.RemainingTimeMilliseconds.Should().Be(10_000);
        state.Duration.Should().Be(10_000 * TimeSpan.TicksPerMillisecond);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CaptureState_returns_correct_type()
    {
        var condition = new ParalyzeCondition(10_000, 120);

        var state = condition.CaptureState();

        state.Type.Should().Be(ConditionType.Paralyze);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Restore_creates_condition_with_preserved_SpeedReduction()
    {
        var state = new ParalyzeConditionState(
            ConditionType.Paralyze,
            SpeedReduction: 120,
            RemainingTimeMilliseconds: 30_000,
            Duration: 60_000 * TimeSpan.TicksPerMillisecond);

        var condition = ParalyzeCondition.Restore(state);

        condition.Should().NotBeNull();
        condition!.SpeedReduction.Should().Be(120);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Restore_Start_applies_saved_speed_reduction()
    {
        var player = PlayerTestDataBuilder.Build(speed: 400);
        var state = new ParalyzeConditionState(
            ConditionType.Paralyze,
            SpeedReduction: 120,
            RemainingTimeMilliseconds: 30_000,
            Duration: 60_000 * TimeSpan.TicksPerMillisecond);

        var condition = ParalyzeCondition.Restore(state);
        player.AddCondition(condition!);

        player.Speed.Should().Be(280);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Restore_Start_removes_haste()
    {
        var player = PlayerTestDataBuilder.Build(speed: 400);
        player.AddCondition(new HasteCondition(10_000, new FormulaValues { MinA = 1, MinB = 100, MaxA = 1, MaxB = 100 }));
        player.Speed.Should().Be(500);

        var state = new ParalyzeConditionState(
            ConditionType.Paralyze,
            SpeedReduction: 120,
            RemainingTimeMilliseconds: 30_000,
            Duration: 60_000 * TimeSpan.TicksPerMillisecond);

        var condition = ParalyzeCondition.Restore(state);
        player.AddCondition(condition!);

        player.HasCondition(ConditionType.Haste).Should().BeFalse();
        player.Speed.Should().Be(280);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void HasExpired_is_false_after_Restore()
    {
        var state = new ParalyzeConditionState(
            ConditionType.Paralyze,
            SpeedReduction: 120,
            RemainingTimeMilliseconds: 30_000,
            Duration: 60_000 * TimeSpan.TicksPerMillisecond);

        var condition = ParalyzeCondition.Restore(state);

        condition!.HasExpired.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CaptureState_after_Restore_and_Start_returns_saved_SpeedReduction()
    {
        var player = PlayerTestDataBuilder.Build(speed: 400);
        var state = new ParalyzeConditionState(
            ConditionType.Paralyze,
            SpeedReduction: 120,
            RemainingTimeMilliseconds: 30_000,
            Duration: 60_000 * TimeSpan.TicksPerMillisecond);

        var condition = ParalyzeCondition.Restore(state);
        player.AddCondition(condition!);

        var recaptured = condition!.CaptureState();

        recaptured.SpeedReduction.Should().Be(120);
        recaptured.Type.Should().Be(ConditionType.Paralyze);
        // Remaining time must be close to the saved value (30_000 ms),
        // not the full Duration (60_000 ms).
        recaptured.RemainingTimeMilliseconds.Should().BeInRange(29_000, 30_001);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Restore_does_not_throw()
    {
        var state = new ParalyzeConditionState(
            ConditionType.Paralyze,
            SpeedReduction: 120,
            RemainingTimeMilliseconds: 5_000,
            Duration: 30_000 * TimeSpan.TicksPerMillisecond);

        var action = () => ParalyzeCondition.Restore(state);

        action.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CaptureState_restore_Start_CaptureState_round_trip_preserves_remaining_time()
    {
        var player = PlayerTestDataBuilder.Build(speed: 400);
        var condition = new ParalyzeCondition(60_000, 120);
        condition.Start(player);

        // Capture remaining time after condition has been running
        var firstState = condition.CaptureState();
        var savedRemaining = firstState.RemainingTimeMilliseconds;

        // Restore and re-apply
        var restored = ParalyzeCondition.Restore(firstState);
        player.AddCondition(restored!);

        // Re-capture — remaining time should be close to the saved value,
        // not the full original Duration (60_000 ms).
        var secondState = restored!.CaptureState();

        secondState.RemainingTimeMilliseconds.Should().BeInRange(
            (long)(savedRemaining * 0.95),
            savedRemaining + 1);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Restore_Start_ends_previous_paralyze()
    {
        var player = PlayerTestDataBuilder.Build(speed: 400);
        player.AddCondition(new ParalyzeCondition(60_000, 200));
        player.Speed.Should().Be(200);

        var state = new ParalyzeConditionState(
            ConditionType.Paralyze,
            SpeedReduction: 120,
            RemainingTimeMilliseconds: 30_000,
            Duration: 60_000 * TimeSpan.TicksPerMillisecond);

        var restored = ParalyzeCondition.Restore(state);
        player.AddCondition(restored!);

        // The previous paralyze (200 reduction) should be gone,
        // only the restored one (120 reduction) should apply.
        player.HasCondition(ConditionType.Paralyze).Should().BeTrue();
        player.Speed.Should().Be(280);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Restore_returns_null_when_remaining_time_is_zero()
    {
        var state = new ParalyzeConditionState(
            ConditionType.Paralyze,
            SpeedReduction: 120,
            RemainingTimeMilliseconds: 0,
            Duration: 0);

        var condition = ParalyzeCondition.Restore(state);

        // Restore returns null for expired conditions to avoid
        // creating a persistent (never-expiring) paralyze.
        condition.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Restore_returns_null_when_remaining_time_is_negative()
    {
        var state = new ParalyzeConditionState(
            ConditionType.Paralyze,
            SpeedReduction: 120,
            RemainingTimeMilliseconds: -1,
            Duration: 0);

        var condition = ParalyzeCondition.Restore(state);

        condition.Should().BeNull();
    }
}
