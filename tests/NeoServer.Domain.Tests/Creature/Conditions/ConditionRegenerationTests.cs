using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;

namespace NeoServer.Domain.Tests.Creature.Conditions;

public class ConditionRegenerationTests
{
    [Fact]
    public void Constructor_sets_type_to_regeneration()
    {
        var sut = new ConditionRegeneration(10000, () => { });

        sut.Type.Should().Be(ConditionType.Regeneration);
    }

    [Fact]
    public void End_invokes_onExpired_callback()
    {
        var expired = false;
        var sut = new ConditionRegeneration(10000, () => expired = true);

        sut.End();

        expired.Should().BeTrue();
    }

    [Fact]
    public void End_guards_against_double_invocation()
    {
        var callCount = 0;
        var sut = new ConditionRegeneration(10000, () => callCount++);

        sut.End();
        sut.End();

        callCount.Should().Be(1);
    }

    [Fact]
    public void End_does_not_invoke_onExpired_when_condition_is_persistent()
    {
        var expired = false;
        var sut = new ConditionRegeneration(0, () => expired = true);

        sut.End();

        expired.Should().BeFalse();
    }

    [Fact]
    public void TryExtend_extends_duration_when_under_max()
    {
        var sut = new ConditionRegeneration(10000, () => { });
        sut.Start(null);

        var result = sut.TryExtend(5000);

        result.Should().BeTrue();
        sut.RemainingTime.Should().BeGreaterThan(14000);
    }

    [Fact]
    public void TryExtend_returns_false_when_exceeding_max()
    {
        var sut = new ConditionRegeneration(10000, () => { });
        sut.Start(null);

        var result = sut.TryExtend(1_200_000);

        result.Should().BeFalse();
    }

    [Fact]
    public void TryExtend_extends_up_to_but_not_over_max_duration()
    {
        var sut = new ConditionRegeneration(10000, () => { });
        sut.Start(null);

        var result = sut.TryExtend(1_189_000);

        result.Should().BeTrue();
        sut.RemainingTime.Should().BeLessThan(1_200_000);
        sut.RemainingTime.Should().BeGreaterThan(1_198_000);
    }
}
