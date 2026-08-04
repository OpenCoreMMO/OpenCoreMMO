using NeoServer.Domain.Combat;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players;

public class PlayerPostAttackTests
{
    [Fact]
    public void PostAttack_sets_logout_block()
    {
        var player = PlayerTestDataBuilder.Build();

        player.PostAttack(new CombatParameter(), player, new CombatResult(0, Result.Success));

        player.IsLogoutBlocked.Should().BeTrue();
    }

    [Fact]
    public void PostAttack_sets_protection_zone_block_when_target_is_player()
    {
        var player = PlayerTestDataBuilder.Build();
        var target = PlayerTestDataBuilder.Build();

        player.PostAttack(new CombatParameter(), target, new CombatResult(0, Result.Success));

        player.IsProtectionZoneBlocked.Should().BeTrue();
    }
}
