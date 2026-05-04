using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players;

public class PlayerRecoverTests
{
    [Fact]
    public void Recovering_is_false_when_no_regeneration_condition()
    {
        var player = PlayerTestDataBuilder.Build();

        player.Recovering.Should().BeFalse();
    }

    [Fact]
    public void Recovering_is_true_when_has_regeneration_condition()
    {
        var player = PlayerTestDataBuilder.Build();
        player.Feed(10);

        player.Recovering.Should().BeTrue();
    }

    [Fact]
    public void Recover_heals_when_recovering()
    {
        var player = (NeoServer.Domain.Creatures.Player.Player)PlayerTestDataBuilder.Build();
        var enemy = PlayerTestDataBuilder.Build();
        player.Vocation.GainHpAmount = 100;
        player.OnDamage(enemy, new CombatDamageList(new CombatDamage(20, DamageType.Melee)));
        player.Feed(10);
        var initialHp = player.HealthPoints;

        player.Recover();

        player.HealthPoints.Should().BeGreaterThan(initialHp);
    }

    [Fact]
    public void Recover_does_nothing_when_not_recovering()
    {
        var player = PlayerTestDataBuilder.Build();
        var initialHp = player.HealthPoints;

        player.Recover();

        player.HealthPoints.Should().Be(initialHp);
    }
}
