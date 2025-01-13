using System.Collections.Generic;
using System.Collections.Immutable;
using Moq;
using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Creatures.Experience;
using Xunit;

namespace NeoServer.Game.Creatures.Tests.Experience;

public class ProportionalExperienceModifierTest
{
    [InlineData(1, 10, 50, 5)]
    [InlineData(2, 10, 50, 10)]
    [InlineData(3, 10, 50, 15)]
    [InlineData(4, 10, 50, 20)]
    [InlineData(5, 10, 50, 25)]
    [InlineData(6, 10, 50, 30)]
    [InlineData(7, 10, 50, 35)]
    [InlineData(8, 10, 50, 40)]
    [InlineData(9, 10, 50, 45)]
    [InlineData(10, 10, 50, 50)]
    [Theory]
    public void GetModifiedBaseExperience(int playerDamage, int totalDamage, uint monsterExperience,
        uint expectedResult)
    {
        var player = new Mock<IPlayer>().Object;
        var otherCreatures = new Mock<ICreature>().Object;
   
        var damages = new DamageRecordList();
        damages.AddOrUpdateDamage(player, (ushort)playerDamage);
        damages.AddOrUpdateDamage(otherCreatures, (ushort)(totalDamage - playerDamage));

        var monsterMock = new Mock<IMonster>();
        monsterMock.Setup(x => x.ReceivedDamages).Returns(damages);
        monsterMock.Setup(x => x.Experience).Returns(monsterExperience);

        var modifier = new ProportionalExperienceModifier();

        Assert.Equal(expectedResult, modifier.GetModifiedBaseExperience(player, monsterMock.Object, monsterExperience));
    }
}