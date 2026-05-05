using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players;

public class PlayerProtectionZoneBlockTests
{
    [Fact]
    public void SetProtectionZoneBlock_adds_condition_when_not_blocked()
    {
        var player = PlayerTestDataBuilder.Build();

        player.SetProtectionZoneBlock();

        player.IsProtectionZoneBlocked.Should().BeTrue();
    }

    [Fact]
    public void SetProtectionZoneBlock_does_nothing_when_pacified()
    {
        var player = PlayerTestDataBuilder.Build();
        player.AddCondition(new PacifiedCondition());

        player.SetProtectionZoneBlock();

        player.IsProtectionZoneBlocked.Should().BeFalse();
    }

    [Fact]
    public void SetProtectionZoneBlock_does_nothing_when_already_blocked()
    {
        var player = PlayerTestDataBuilder.Build();
        player.SetProtectionZoneBlock();
        player.IsProtectionZoneBlocked.Should().BeTrue();

        player.SetProtectionZoneBlock();

        var conditions = player.GetConditions();
        conditions.Should().ContainSingle(c => c.Type == ConditionType.ProtectionZoneBlock);
    }

    [Fact]
    public void RemoveProtectionZoneBlock_removes_condition()
    {
        var player = PlayerTestDataBuilder.Build();
        player.SetProtectionZoneBlock();
        player.IsProtectionZoneBlocked.Should().BeTrue();

        player.RemoveProtectionZoneBlock();

        player.IsProtectionZoneBlocked.Should().BeFalse();
    }

    [Fact]
    public void RemoveProtectionZoneBlock_does_nothing_when_not_blocked()
    {
        var player = PlayerTestDataBuilder.Build();

        player.Invoking(x => x.RemoveProtectionZoneBlock()).Should().NotThrow();
        player.IsProtectionZoneBlocked.Should().BeFalse();
    }

    [Fact]
    public void Repeated_combat_block_keeps_only_one_condition()
    {
        var player = PlayerTestDataBuilder.Build();
        player.AddCondition(new CombatBlockCondition(ConditionType.ProtectionZoneBlock));
        player.AddCondition(new CombatBlockCondition(ConditionType.ProtectionZoneBlock));

        player.GetConditions().Should().ContainSingle(c => c.Type == ConditionType.ProtectionZoneBlock);
    }
}
