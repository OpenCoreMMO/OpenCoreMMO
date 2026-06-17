using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Conditions;

public class ConditionDamageRestoreTests
{
    [Fact]
    [Trait("Category", "HappyPath")]
    public void CaptureState_returns_correct_state_for_fixed_amount_condition()
    {
        // Arrange
        var creature = PlayerTestDataBuilder.Build();
        var interval = 2000u;
        byte amount = 5;
        ushort damage = 10;
        var condition = new ConditionDamage(creature, ConditionType.Poisoned, interval, amount, damage, EffectT.RingsGreen);
        condition.Start(creature);

        // Act
        var state = condition.CaptureState();

        // Assert
        state.Type.Should().Be(ConditionType.Poisoned);
        state.DamageType.Should().Be(DamageType.Earth);
        state.Effect.Should().Be(EffectT.RingsGreen);
        state.Interval.Should().Be(interval);
        state.RemainingCooldownMilliseconds.Should().BeGreaterThanOrEqualTo(0);
        state.RemainingDamages.Should().HaveCount(5);
        state.RemainingDamages.Should().AllSatisfy(d => d.Should().Be(damage));
        state.Amount.Should().Be(amount);
        state.MinDamage.Should().Be(damage);
        state.MaxDamage.Should().Be(damage);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CaptureState_returns_correct_state_for_variable_damage_condition()
    {
        // Arrange
        var creature = PlayerTestDataBuilder.Build();
        var interval = 2000u;
        var condition = new ConditionDamage(creature, ConditionType.Burning, interval, minDamage: 5, maxDamage: 50, effect: EffectT.SparkYellow);
        condition.Start(creature);

        // Act
        var state = condition.CaptureState();

        // Assert
        state.Type.Should().Be(ConditionType.Burning);
        state.DamageType.Should().Be(DamageType.Fire);
        state.Effect.Should().Be(EffectT.SparkYellow);
        state.Interval.Should().Be(interval);
        state.RemainingCooldownMilliseconds.Should().BeGreaterThanOrEqualTo(0);
        state.RemainingDamages.Should().NotBeEmpty();
        state.Amount.Should().Be(0);
        state.MinDamage.Should().Be(5);
        state.MaxDamage.Should().Be(50);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Restore_preserves_remaining_damage_queue()
    {
        // Arrange
        var creature = PlayerTestDataBuilder.Build();
        var expectedDamages = new ushort[] { 10, 5 };
        var state = new ConditionDamageState(
            ConditionType.Poisoned,
            DamageType.Earth,
            EffectT.RingsGreen,
            Interval: 2000,
            RemainingCooldownMilliseconds: 500,
            RemainingDamages: expectedDamages,
            Amount: 3,
            MinDamage: 5,
            MaxDamage: 30);

        // Act
        var condition = ConditionDamage.Restore(state);
        condition.Start(creature);

        // Assert - queue should NOT have been regenerated
        var capturedState = condition.CaptureState();
        capturedState.RemainingDamages.Should().Equal(expectedDamages);
        capturedState.RemainingDamages.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Restore_with_some_hits_consumed_has_correct_remaining_damages()
    {
        // Arrange
        var creature = PlayerTestDataBuilder.Build();
        var remainingDamages = new ushort[] { 10, 10 };
        var state = new ConditionDamageState(
            ConditionType.Poisoned,
            DamageType.Earth,
            EffectT.RingsGreen,
            Interval: 2000,
            RemainingCooldownMilliseconds: 1000,
            RemainingDamages: remainingDamages,
            Amount: 5,
            MinDamage: 10,
            MaxDamage: 10);

        // Act
        var condition = ConditionDamage.Restore(state);
        condition.Start(creature);

        // Assert
        var captured = condition.CaptureState();
        captured.RemainingDamages.Should().HaveCount(2);
        captured.RemainingDamages.Should().Equal(remainingDamages);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void HasExpired_is_false_when_damage_queue_has_items_after_restore()
    {
        // Arrange
        var state = new ConditionDamageState(
            ConditionType.Poisoned,
            DamageType.Earth,
            EffectT.None,
            Interval: 2000,
            RemainingCooldownMilliseconds: 1000,
            RemainingDamages: [10, 20],
            Amount: 2,
            MinDamage: 10,
            MaxDamage: 20);

        // Act
        var condition = ConditionDamage.Restore(state);

        // Assert
        condition.HasExpired.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Restore_with_empty_queue_has_expired()
    {
        // Arrange
        var state = new ConditionDamageState(
            ConditionType.Poisoned,
            DamageType.Earth,
            EffectT.RingsGreen,
            Interval: 2000,
            RemainingCooldownMilliseconds: 0,
            RemainingDamages: Array.Empty<ushort>(),
            Amount: 0,
            MinDamage: 0,
            MaxDamage: 0);

        // Act
        var condition = ConditionDamage.Restore(state);

        // Assert
        condition.HasExpired.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void CaptureState_returns_empty_array_when_not_started()
    {
        // Arrange
        var creature = PlayerTestDataBuilder.Build();
        var condition = new ConditionDamage(creature, ConditionType.Poisoned, 2000, amount: 5, damage: 10);

        // Act - no Start() call, so _damageQueue is null
        var state = condition.CaptureState();

        // Assert
        state.RemainingDamages.Should().BeEmpty();
        state.RemainingCooldownMilliseconds.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void HasExpired_does_not_throw_when_damage_queue_is_null()
    {
        // Arrange
        var creature = PlayerTestDataBuilder.Build();
        var condition = new ConditionDamage(creature, ConditionType.Poisoned, 2000, amount: 3, damage: 10);

        // Act & Assert - no Start() call, _damageQueue is null, should not throw
        condition.Invoking(c => c.HasExpired).Should().NotThrow();
        condition.HasExpired.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Restore_with_null_cause_does_not_throw()
    {
        // Arrange
        var state = new ConditionDamageState(
            ConditionType.Poisoned,
            DamageType.Earth,
            EffectT.None,
            Interval: 2000,
            RemainingCooldownMilliseconds: 100,
            RemainingDamages: [10],
            Amount: 1,
            MinDamage: 10,
            MaxDamage: 10);

        // Act
        var action = () => ConditionDamage.Restore(state);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Execute_does_not_throws_when_cause_is_null()
    {
        // Arrange
        var creature = PlayerTestDataBuilder.Build();

        var state = new ConditionDamageState(
            ConditionType.Poisoned,
            DamageType.Earth,
            EffectT.None,
            Interval: 2000,
            RemainingCooldownMilliseconds: 0,
            RemainingDamages: [10],
            Amount: 1,
            MinDamage: 10,
            MaxDamage: 10);

        var condition = ConditionDamage.Restore(state);

        // Act
        var executeAction = () => condition.Execute(creature);

        // Assert — Execute() gracefully handles a null Cause
        // by falling back to a generic DamageElement source,
        // confirming the Restore-with-null-cause path is safe
        // at the point where damage is actually applied.
        executeAction.Should().NotThrow();
    }
}
