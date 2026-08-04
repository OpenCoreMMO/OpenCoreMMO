using NeoServer.Domain.Common.Services;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players;

public class PlayerLogoutCooldownTests
{
    [Fact]
    public void Player_cannot_logout_when_logout_cooldown_is_active()
    {
        var player = PlayerTestDataBuilder.Build();

        player.Login(10_000);

        var result = player.Logout(false);

        result.Should().BeFalse();
    }

    [Fact]
    public void Player_can_logout_when_logout_cooldown_has_expired()
    {
        var player = PlayerTestDataBuilder.Build();

        player.Login(1);

        Thread.Sleep(10);

        var result = player.Logout(false);

        result.Should().BeTrue();
    }

    [Fact]
    public void Player_can_logout_during_logout_cooldown_when_forced()
    {
        var player = PlayerTestDataBuilder.Build();

        player.Login(10_000);

        var result = player.Logout(true);

        result.Should().BeTrue();
    }

    [Fact]
    public void Player_can_logout_when_logout_cooldown_is_zero()
    {
        var player = PlayerTestDataBuilder.Build();

        player.Login(0);

        var result = player.Logout(false);

        result.Should().BeTrue();
    }

    [Fact]
    public void Player_receives_cooldown_message_when_logout_blocked()
    {
        var player = PlayerTestDataBuilder.Build();

        player.Login(10_000);

        var error = string.Empty;
        OperationFailService.OnOperationFailed += (_, message, _) => error = message;

        player.Logout(false);

        error.Should().NotBeEmpty();
        error.Should().Contain("seconds");
    }

    [Fact]
    public void Player_logout_cooldown_is_restarted_on_reconnect()
    {
        var player = PlayerTestDataBuilder.Build();

        player.Login(1);
        player.Login(10_000);

        Thread.Sleep(10);

        var result = player.Logout(false);

        result.Should().BeFalse();
    }
}
