using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players;

public class PlayerParalyzeConditionTests
{
    [Fact]
    public void Paralyze_reduces_speed_when_applied()
    {
        var player = PlayerTestDataBuilder.Build(speed: 400);
        var paralyze = new ParalyzeCondition(10_000, 200);

        player.AddCondition(paralyze);

        player.Speed.Should().Be(200);
    }

    [Fact]
    public void Paralyze_removes_haste_when_applied()
    {
        var player = PlayerTestDataBuilder.Build(speed: 400);
        player.AddCondition(new HasteCondition(10_000, new FormulaValues { MinA = 1, MinB = 100, MaxA = 1, MaxB = 100 }));
        player.Speed.Should().Be(500);

        var paralyze = new ParalyzeCondition(10_000, 200);
        player.AddCondition(paralyze);

        player.HasCondition(ConditionType.Haste).Should().BeFalse();
        player.Speed.Should().Be(200);
    }

    [Fact]
    public void Paralyze_restores_speed_when_it_ends()
    {
        var player = PlayerTestDataBuilder.Build(speed: 400);
        var paralyze = new ParalyzeCondition(10_000, 200);
        player.AddCondition(paralyze);
        player.Speed.Should().Be(200);

        player.RemoveCondition(paralyze);

        player.Speed.Should().Be(400);
    }

    [Fact]
    public void Paralyze_restores_speed_and_haste_does_not_return()
    {
        var player = PlayerTestDataBuilder.Build(speed: 400);
        player.AddCondition(new HasteCondition(10_000, new FormulaValues { MinA = 1, MinB = 100, MaxA = 1, MaxB = 100 }));
        player.Speed.Should().Be(500);

        var paralyze = new ParalyzeCondition(10_000, 200);
        player.AddCondition(paralyze);
        player.Speed.Should().Be(200);

        player.RemoveCondition(paralyze);

        player.Speed.Should().Be(400);
        player.HasCondition(ConditionType.Haste).Should().BeFalse();
    }

    [Fact]
    public void Paralyze_does_not_crash_when_no_haste_is_active()
    {
        var player = PlayerTestDataBuilder.Build(speed: 400);
        var paralyze = new ParalyzeCondition(10_000, 200);

        player.Invoking(x => x.AddCondition(paralyze)).Should().NotThrow();
        player.Speed.Should().Be(200);
    }

    [Fact]
    public void Reapplying_paralyze_ends_previous_paralyze()
    {
        var player = PlayerTestDataBuilder.Build(speed: 400);
        var first = new ParalyzeCondition(10_000, 200);
        player.AddCondition(first);
        player.Speed.Should().Be(200);

        var second = new ParalyzeCondition(10_000, 100);
        player.AddCondition(second);

        player.HasCondition(ConditionType.Paralyze).Should().BeTrue();
        player.GetCondition(ConditionType.Paralyze).Should().BeSameAs(second);
        player.Speed.Should().Be(300);
    }

    [Fact]
    public void Paralyze_does_not_stack_overflow_when_haste_endaction_readds_paralyze()
    {
        var player = PlayerTestDataBuilder.Build(speed: 400);

        var reentrantHaste = new Condition(ConditionType.Haste, 10_000, () =>
        {
            player.AddCondition(new ParalyzeCondition(10_000, 50));
        });
        player.AddCondition(reentrantHaste);
        player.Speed.Should().Be(400);

        var paralyze = new ParalyzeCondition(10_000, 200);

        player.Invoking(x => x.AddCondition(paralyze)).Should().NotThrow();
    }

    [Fact]
    public void Paralyze_does_not_stack_overflow_when_paralyze_endaction_readds_paralyze()
    {
        var player = PlayerTestDataBuilder.Build(speed: 400);

        var innerParalyze = new ParalyzeCondition(10_000, 50);
        var outerParalyze = new ParalyzeCondition(10_000, 200);

        innerParalyze.EndAction = () => player.AddCondition(outerParalyze);
        player.AddCondition(innerParalyze);

        player.Invoking(x => x.RemoveCondition(innerParalyze)).Should().NotThrow();
    }

    [Fact]
    public void Paralyze_start_reentrancy_does_not_corrupt_condition_list()
    {
        var player = PlayerTestDataBuilder.Build(speed: 400);

        var reentrantHaste = new Condition(ConditionType.Haste, 10_000, () =>
        {
            player.AddCondition(new Condition(ConditionType.Outfit, 5_000));
        });
        player.AddCondition(reentrantHaste);

        var paralyze = new ParalyzeCondition(10_000, 200);
        player.AddCondition(paralyze);

        player.HasCondition(ConditionType.Haste).Should().BeFalse();
        player.HasCondition(ConditionType.Paralyze).Should().BeTrue();
        player.HasCondition(ConditionType.Outfit).Should().BeTrue();
        player.Speed.Should().Be(200);
    }

    [Fact]
    public void Paralyze_does_not_crash_when_haste_endaction_readds_haste()
    {
        var player = PlayerTestDataBuilder.Build(speed: 400);

        var reentrantHaste = new Condition(ConditionType.Haste, 10_000, () =>
        {
            player.AddCondition(new HasteCondition(10_000, new FormulaValues { MinA = 1, MinB = 50, MaxA = 1, MaxB = 50 }));
        });
        player.AddCondition(reentrantHaste);

        var paralyze = new ParalyzeCondition(10_000, 200);

        player.Invoking(x => x.AddCondition(paralyze)).Should().NotThrow();
    }
}
