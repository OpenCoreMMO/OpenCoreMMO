using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Conditions;

public class HasteConditionSaveRestoreTests
{
    [Fact]
    [Trait("Category", "HappyPath")]
    public void CaptureState_returns_correct_state_after_Start()
    {
        var creature = PlayerTestDataBuilder.Build(speed: 400);
        var formula = new FormulaValues { MinA = 0.5, MinB = 50, MaxA = 1.0, MaxB = 100 };
        var condition = new HasteCondition(10_000, formula, EffectT.GlitterBlue);
        condition.Start(creature);

        var state = condition.CaptureState();

        state.Type.Should().Be(ConditionType.Haste);
        state.Effect.Should().Be(EffectT.GlitterBlue);
        state.SpeedBoost.Should().BeGreaterThan((ushort)0);
        state.RemainingTimeMilliseconds.Should().BeGreaterThan(0);
        state.Duration.Should().Be(10_000 * TimeSpan.TicksPerMillisecond);
        state.FormulaValues.Should().Be(formula);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void CaptureState_returns_zero_SpeedBoost_when_not_started()
    {
        var formula = new FormulaValues { MinA = 0.5, MinB = 50, MaxA = 1.0, MaxB = 100 };
        var condition = new HasteCondition(10_000, formula, EffectT.GlitterBlue);

        var state = condition.CaptureState();

        state.SpeedBoost.Should().Be(0);
        state.RemainingTimeMilliseconds.Should().Be(10_000);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CaptureState_returns_correct_type_and_effect()
    {
        var formula = new FormulaValues { MinA = 0.5, MinB = 50, MaxA = 1.0, MaxB = 100 };
        var condition = new HasteCondition(10_000, formula, EffectT.GroundShaker);

        var state = condition.CaptureState();

        state.Type.Should().Be(ConditionType.Haste);
        state.Effect.Should().Be(EffectT.GroundShaker);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Restore_creates_condition_with_preserved_SpeedBoost()
    {
        var formula = new FormulaValues { MinA = 0.5, MinB = 50, MaxA = 1.0, MaxB = 100 };
        var state = new HasteConditionState(
            ConditionType.Haste,
            EffectT.GlitterBlue,
            SpeedBoost: 240,
            RemainingTimeMilliseconds: 30_000,
            Duration: 60_000 * TimeSpan.TicksPerMillisecond,
            FormulaValues: formula);

        var condition = HasteCondition.Restore(state);

        condition.Should().NotBeNull();
        condition.SpeedBoost.Should().Be(240);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Restore_preserves_FormulaValues_and_Effect()
    {
        var formula = new FormulaValues { MinA = 1.5, MinB = 200, MaxA = 2.5, MaxB = 400 };
        var state = new HasteConditionState(
            ConditionType.Haste,
            EffectT.GroundShaker,
            SpeedBoost: 350,
            RemainingTimeMilliseconds: 15_000,
            Duration: 30_000 * TimeSpan.TicksPerMillisecond,
            FormulaValues: formula);

        var condition = HasteCondition.Restore(state);

        condition.FormulaValues.MinA.Should().Be(1.5);
        condition.FormulaValues.MinB.Should().Be(200);
        condition.FormulaValues.MaxA.Should().Be(2.5);
        condition.FormulaValues.MaxB.Should().Be(400);
        condition.Effect.Should().Be(EffectT.GroundShaker);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Restore_Start_applies_saved_speed_boost()
    {
        var player = PlayerTestDataBuilder.Build(speed: 400);
        var formula = new FormulaValues { MinA = 1, MinB = 100, MaxA = 1, MaxB = 100 };
        var state = new HasteConditionState(
            ConditionType.Haste,
            EffectT.None,
            SpeedBoost: 200,
            RemainingTimeMilliseconds: 10_000,
            Duration: 60_000 * TimeSpan.TicksPerMillisecond,
            FormulaValues: formula);

        var condition = HasteCondition.Restore(state);
        player.AddCondition(condition);

        player.Speed.Should().Be(600);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Restore_Start_does_not_regenerate_SpeedBoost()
    {
        var player = PlayerTestDataBuilder.Build(speed: 400);
        var formula = new FormulaValues { MinA = 1, MinB = 100, MaxA = 1, MaxB = 100 };
        var state = new HasteConditionState(
            ConditionType.Haste,
            EffectT.None,
            SpeedBoost: 200,
            RemainingTimeMilliseconds: 10_000,
            Duration: 60_000 * TimeSpan.TicksPerMillisecond,
            FormulaValues: formula);

        var condition = HasteCondition.Restore(state);
        player.AddCondition(condition);

        condition.SpeedBoost.Should().Be(200);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Restore_Start_removes_paralyze()
    {
        var player = PlayerTestDataBuilder.Build(speed: 400);
        player.AddCondition(new ParalyzeCondition(30_000, 100));
        player.Speed.Should().Be(300);

        var formula = new FormulaValues { MinA = 1, MinB = 50, MaxA = 1, MaxB = 50 };
        var state = new HasteConditionState(
            ConditionType.Haste,
            EffectT.None,
            SpeedBoost: 100,
            RemainingTimeMilliseconds: 10_000,
            Duration: 60_000 * TimeSpan.TicksPerMillisecond,
            FormulaValues: formula);

        var condition = HasteCondition.Restore(state);
        player.AddCondition(condition);

        player.HasCondition(ConditionType.Paralyze).Should().BeFalse();
        player.Speed.Should().Be(500);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void HasExpired_is_false_after_Restore()
    {
        var formula = new FormulaValues { MinA = 0.5, MinB = 50, MaxA = 1.0, MaxB = 100 };
        var state = new HasteConditionState(
            ConditionType.Haste,
            EffectT.GlitterBlue,
            SpeedBoost: 240,
            RemainingTimeMilliseconds: 30_000,
            Duration: 60_000 * TimeSpan.TicksPerMillisecond,
            FormulaValues: formula);

        var condition = HasteCondition.Restore(state);

        condition.HasExpired.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CaptureState_after_Restore_and_Start_returns_saved_SpeedBoost()
    {
        var player = PlayerTestDataBuilder.Build(speed: 400);
        var formula = new FormulaValues { MinA = 1, MinB = 100, MaxA = 1, MaxB = 100 };
        var state = new HasteConditionState(
            ConditionType.Haste,
            EffectT.None,
            SpeedBoost: 200,
            RemainingTimeMilliseconds: 10_000,
            Duration: 60_000 * TimeSpan.TicksPerMillisecond,
            FormulaValues: formula);

        var condition = HasteCondition.Restore(state);
        player.AddCondition(condition);

        var recaptured = condition.CaptureState();

        recaptured.SpeedBoost.Should().Be(200);
        recaptured.Type.Should().Be(ConditionType.Haste);
        recaptured.Effect.Should().Be(EffectT.None);
        // Remaining time must be close to the saved value (10_000 ms),
        // not the full Duration (60_000 ms).
        recaptured.RemainingTimeMilliseconds.Should().BeInRange(9_000, 10_001);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Restore_does_not_throw_when_state_has_EffectT_None()
    {
        var formula = new FormulaValues { MinA = 0.5, MinB = 50, MaxA = 1.0, MaxB = 100 };
        var state = new HasteConditionState(
            ConditionType.Haste,
            EffectT.None,
            SpeedBoost: 150,
            RemainingTimeMilliseconds: 5_000,
            Duration: 30_000 * TimeSpan.TicksPerMillisecond,
            FormulaValues: formula);

        var action = () => HasteCondition.Restore(state);

        action.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CaptureState_restore_Start_CaptureState_round_trip_preserves_remaining_time()
    {
        var player = PlayerTestDataBuilder.Build(speed: 400);
        var formula = new FormulaValues { MinA = 1, MinB = 100, MaxA = 1, MaxB = 100 };
        var condition = new HasteCondition(60_000, formula, EffectT.GlitterBlue);
        condition.Start(player);

        // Capture remaining time after condition has been running
        var firstState = condition.CaptureState();
        var savedRemaining = firstState.RemainingTimeMilliseconds;

        // Restore and re-apply
        var restored = HasteCondition.Restore(firstState);
        player.AddCondition(restored);

        // Re-capture — remaining time should be close to the saved value,
        // not the full original Duration (60_000 ms).
        var secondState = restored.CaptureState();

        secondState.RemainingTimeMilliseconds.Should().BeInRange(
            (long)(savedRemaining * 0.95),
            savedRemaining + 1);
    }
}
