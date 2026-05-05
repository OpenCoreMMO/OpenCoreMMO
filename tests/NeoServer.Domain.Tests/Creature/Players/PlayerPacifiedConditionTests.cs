using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players;

public class PlayerPacifiedConditionTests
{
    [Fact]
    public void Pacified_sets_IsPacified_when_applied()
    {
        var player = PlayerTestDataBuilder.Build();
        var pacified = new PacifiedCondition();

        player.AddCondition(pacified);

        player.IsPacified.Should().BeTrue();
    }

    [Fact]
    public void Pacified_removes_logout_block_when_applied()
    {
        var player = PlayerTestDataBuilder.Build();
        player.AddCondition(new Condition(ConditionType.LogoutBlock, 0));

        var pacified = new PacifiedCondition();
        player.AddCondition(pacified);

        player.HasCondition(ConditionType.LogoutBlock).Should().BeFalse();
        player.IsPacified.Should().BeTrue();
    }

    [Fact]
    public void Pacified_removes_protection_zone_block_when_applied()
    {
        var player = PlayerTestDataBuilder.Build();
        player.AddCondition(new Condition(ConditionType.ProtectionZoneBlock, 0));

        var pacified = new PacifiedCondition();
        player.AddCondition(pacified);

        player.HasCondition(ConditionType.ProtectionZoneBlock).Should().BeFalse();
        player.IsPacified.Should().BeTrue();
    }

    [Fact]
    public void Pacified_does_not_crash_when_no_conflicting_conditions()
    {
        var player = PlayerTestDataBuilder.Build();
        var pacified = new PacifiedCondition();

        player.Invoking(x => x.AddCondition(pacified)).Should().NotThrow();
        player.IsPacified.Should().BeTrue();
    }

    [Fact]
    public void Repeated_pacified_replaces_previous()
    {
        var player = PlayerTestDataBuilder.Build();
        var first = new PacifiedCondition();
        player.AddCondition(first);

        var second = new PacifiedCondition();
        player.AddCondition(second);

        player.HasCondition(ConditionType.Pacified).Should().BeTrue();
        player.IsPacified.Should().BeTrue();
    }

    [Fact]
    public void Pacified_end_does_not_restore_logout_block()
    {
        var player = PlayerTestDataBuilder.Build();
        player.AddCondition(new Condition(ConditionType.LogoutBlock, 0));

        var pacified = new PacifiedCondition();
        player.AddCondition(pacified);

        player.RemoveCondition(ConditionType.Pacified);

        player.IsPacified.Should().BeFalse();
        player.HasCondition(ConditionType.LogoutBlock).Should().BeFalse();
    }

    [Fact]
    public void Pacified_start_does_not_crash_when_other_condition_endaction_readds()
    {
        var player = PlayerTestDataBuilder.Build();

        var reentrantLogout = new Condition(ConditionType.LogoutBlock, 0, () =>
        {
            player.AddCondition(new Condition(ConditionType.Outfit, 5_000));
        });
        player.AddCondition(reentrantLogout);

        var pacified = new PacifiedCondition();

        player.Invoking(x => x.AddCondition(pacified)).Should().NotThrow();
        player.IsPacified.Should().BeTrue();
    }
}
