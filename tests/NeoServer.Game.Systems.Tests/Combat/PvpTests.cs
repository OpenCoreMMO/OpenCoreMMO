using FluentAssertions;
using NeoServer.Game.Combat.Services;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Enums;
using NeoServer.Game.Tests.Helpers.Player;
using NeoServer.Server.Events.Combat;

namespace NeoServer.Game.Systems.Tests.Combat;

public class PvpTests
{
    [Fact]
    public void Player_gets_white_skull_when_attacking_another_player()
    {
        //arrange
        var aggressor = PlayerTestDataBuilder.Build();
        var victim = PlayerTestDataBuilder.Build();
        var spectator = PlayerTestDataBuilder.Build();

        var handler = new CreatureAttackEventHandler(new PlayerSkullService(new GameConfiguration()
        {
            PvP = new PvPConfiguration("Open", true, 3, 5, 5, 5, 5, 5, 5, 5, 5, 5)
        }));
        
        aggressor.OnAttackEnemy += handler.Execute;

        //act
        aggressor.Attack(victim);

        //assert
        aggressor.HasSkull.Should().BeTrue();
        aggressor.Skull.Should().Be(Skull.White);
        aggressor.GetSkull(spectator).Should().Be(Skull.White);
        aggressor.GetSkull(victim).Should().Be(Skull.White);
    }
}