using Moq;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;

namespace NeoServer.Domain.Tests.Creature.Conditions;

public class ConditionRegenerationTests
{
    [Fact]
    public void Constructor_sets_type_to_regeneration()
    {
        var sut = new ConditionRegeneration(10000);

        sut.Type.Should().Be(ConditionType.Regeneration);
    }

    [Fact]
    public void End_invokes_onExpired_callback()
    {
        var expired = false;
        var sut = new ConditionRegeneration(10000);
        var mockPlayer = new Mock<IPlayer>();
        mockPlayer.Setup(p => p.SetAsHungry()).Callback(() => expired = true);
        sut.Start(mockPlayer.Object);

        sut.End();

        expired.Should().BeTrue();
    }

    [Fact]
    public void End_guards_against_double_invocation()
    {
        var callCount = 0;
        var sut = new ConditionRegeneration(10000);
        var mockPlayer = new Mock<IPlayer>();
        mockPlayer.Setup(p => p.SetAsHungry()).Callback(() => callCount++);
        sut.Start(mockPlayer.Object);

        sut.End();
        sut.End();

        callCount.Should().Be(1);
    }

    [Fact]
    public void End_does_not_invoke_onExpired_when_condition_is_persistent()
    {
        var expired = false;
        var sut = new ConditionRegeneration(0);
        var mockPlayer = new Mock<IPlayer>();
        mockPlayer.Setup(p => p.SetAsHungry()).Callback(() => expired = true);
        sut.Start(mockPlayer.Object);

        sut.End();

        expired.Should().BeFalse();
    }

    [Fact]
    public void TryExtend_extends_duration_when_under_max()
    {
        var sut = new ConditionRegeneration(10000);
        sut.Start(null);

        var result = sut.TryExtend(5000);

        result.Should().BeTrue();
        sut.RemainingTime.Should().BeGreaterThan(14000);
    }

    [Fact]
    public void TryExtend_returns_false_when_exceeding_max()
    {
        var sut = new ConditionRegeneration(10000);
        sut.Start(null);

        var result = sut.TryExtend(1_200_000);

        result.Should().BeFalse();
    }

    [Fact]
    public void TryExtend_extends_up_to_but_not_over_max_duration()
    {
        var sut = new ConditionRegeneration(10000);
        sut.Start(null);

        var result = sut.TryExtend(1_189_000);

        result.Should().BeTrue();
        sut.RemainingTime.Should().BeLessThan(1_200_000);
        sut.RemainingTime.Should().BeGreaterThan(1_198_000);
    }

    [Fact]
    public void TryExtend_treats_negative_remaining_time_as_zero()
    {
        var sut = new ConditionRegeneration(10000);

        var result = sut.TryExtend(1_189_000);

        result.Should().BeTrue();
        sut.RemainingTime.Should().BeGreaterThan(1_188_000);
    }

    [Fact]
    public void TryExtend_returns_false_when_clamped_remaining_plus_additional_exceeds_max()
    {
        var sut = new ConditionRegeneration(10000);

        var result = sut.TryExtend(1_200_001);

        result.Should().BeFalse();
    }

    [Fact]
    public void CaptureState_returns_remaining_time()
    {
        var sut = new ConditionRegeneration(90000);

        var state = sut.CaptureState();

        state.Type.Should().Be(ConditionType.Regeneration);
        state.RemainingTimeMilliseconds.Should().Be(90000);
    }

    [Fact]
    public void Restore_creates_condition_from_state()
    {
        var state = new ConditionRegenerationState(ConditionType.Regeneration, 45000);

        var result = ConditionRegeneration.Restore(state);

        result.Should().NotBeNull();
        result.Type.Should().Be(ConditionType.Regeneration);
        result.RemainingTime.Should().Be(45000);
    }

    [Fact]
    public void Restore_returns_null_when_remaining_time_is_zero()
    {
        var state = new ConditionRegenerationState(ConditionType.Regeneration, 0);

        var result = ConditionRegeneration.Restore(state);

        result.Should().BeNull();
    }

    [Fact]
    public void Restore_returns_null_when_remaining_time_is_negative()
    {
        var state = new ConditionRegenerationState(ConditionType.Regeneration, -1000);

        var result = ConditionRegeneration.Restore(state);

        result.Should().BeNull();
    }
}
