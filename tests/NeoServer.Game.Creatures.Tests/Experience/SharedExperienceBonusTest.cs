using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Moq;
using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Creatures.Experience;
using NeoServer.Game.Creatures.Player;
using NeoServer.Game.Tests.Helpers.Player;
using Xunit;

namespace NeoServer.Game.Creatures.Tests.Experience;

public class SharedExperienceBonusTest
{
    [Fact]
    public void IsEnabled_ReturnsFalse_WhenPlayerIsNull()
    {
        var config = new Mock<ISharedExperienceConfiguration>().Object;
        var bonus = new SharedExperienceBonus(config);
        var monster = new Mock<IMonster>().Object;

        Assert.False(bonus.IsEnabled(null, monster));
    }

    [Fact]
    public void IsEnabled_ReturnsFalse_WhenMonsterIsNull()
    {
        var config = new Mock<ISharedExperienceConfiguration>().Object;
        var bonus = new SharedExperienceBonus(config);
        var player = new Mock<IPlayer>().Object;

        Assert.False(bonus.IsEnabled(player, null));
    }

    [Fact]
    public void IsEnabled_ReturnsFalse_WhenPlayerIsNotInParty()
    {
        var config = new Mock<ISharedExperienceConfiguration>().Object;
        var bonus = new SharedExperienceBonus(config);
        var playerMock = new Mock<IPlayer>();
        playerMock.SetupGet(x => x.PlayerParty).Returns(new PlayerParty(playerMock.Object));
        var monster = new Mock<IMonster>().Object;

        Assert.False(bonus.IsEnabled(playerMock.Object, monster));
    }

    [InlineData(true, true, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    [Theory]
    public void IsExperienceSharingTurnedOn(bool isAlwaysOn, bool isEnabled, bool expectedResult)
    {
        var configMock = new Mock<ISharedExperienceConfiguration>();
        configMock.Setup(x => x.IsSharedExperienceAlwaysOn).Returns(isAlwaysOn);

        var partyMock = new Mock<IParty>();
        partyMock.Setup(x => x.IsSharedExperienceEnabled).Returns(isEnabled);

        var bonus = new SharedExperienceBonus(configMock.Object);

        Assert.Equal(expectedResult, bonus.IsExperienceSharingTurnedOn(partyMock.Object));
    }

    [InlineData(1, 0, false)]
    [InlineData(1, 1, true)]
    [InlineData(1, 2, true)]
    [Theory]
    public void DoesMonsterQualify(int minimumExperience, int monsterExperience, bool expectedResult)
    {
        var configMock = new Mock<ISharedExperienceConfiguration>();
        configMock.Setup(x => x.MinimumMonsterExperienceToBeShared).Returns((uint)minimumExperience);

        var monsterMock = new Mock<IMonster>();
        monsterMock.Setup(x => x.Experience).Returns((uint)monsterExperience);

        var bonus = new SharedExperienceBonus(configMock.Object);

        Assert.Equal(expectedResult, bonus.DoesMonsterQualify(monsterMock.Object));
    }

    [InlineData(false, 3d / 2d, true, 1, 2, 3, 4)]
    [InlineData(false, 3d / 2d, true, 1, 1, 1, 100)]
    [InlineData(true, 3d / 2d, false, 1, 1, 1, 100)]
    [InlineData(true, 3d / 2d, true, 8, 8, 8, 8)]
    [InlineData(true, 3d / 2d, true, 8, 8, 8, 9)]
    [InlineData(true, 3d / 2d, true, 1, 1, 1, 1)]
    [Theory]
    public void ArePartyLevelsInProperRange(bool requirePartyMemberLevelProximity,
        double lowestLevelSupportedMultiplier, bool expectedResult, params int[] partyLevels)
    {
        var players = partyLevels.Select(x =>
        {
            var playerMock = new Mock<IPlayer>();
            playerMock.Setup(x => x.Level).Returns((ushort)x);
            return playerMock.Object;
        }).ToList();

        var partyMock = new Mock<IParty>();
        partyMock.Setup(x => x.Members).Returns(players);

        var configMock = new Mock<ISharedExperienceConfiguration>();
        configMock.Setup(x => x.RequirePartyMemberLevelProximity).Returns(requirePartyMemberLevelProximity);
        configMock.Setup(x => x.LowestLevelSupportedMultipler).Returns(lowestLevelSupportedMultiplier);

        var bonus = new SharedExperienceBonus(configMock.Object);

        Assert.Equal(expectedResult, bonus.ArePartyLevelsInProperRange(partyMock.Object));
    }

    [InlineData(true, 30, 1, true, 101, 100, 7)] // x + 1
    [InlineData(true, 30, 1, true, 100, 101, 7)] // y + 1
    [InlineData(true, 30, 1, true, 130, 100, 7)] // x + 30
    [InlineData(true, 30, 1, true, 100, 130, 7)] // y + 30
    [InlineData(true, 30, 1, false, 100, 131, 7)] // x + 31
    [InlineData(true, 30, 1, false, 131, 100, 7)] // y + 31
    [InlineData(true, 30, 1, true, 100, 100, 8)] // z + 1
    [InlineData(true, 30, 1, true, 100, 100, 6)] // z - 1
    [InlineData(true, 30, 1, false, 100, 100, 9)] // z + 2
    [InlineData(true, 30, 1, false, 100, 100, 5)] // z - 2
    [Theory]
    public void ArePartyMembersCloseEnoughToEachOther(bool requirePartyProximity, int maxDistance,
        int maxVerticalDistance, bool expectedResult, int x, int y, int z)
    {
        var originPlayer = PlayerTestDataBuilder.Build();
        var distantPlayer = PlayerTestDataBuilder.Build(2);
        distantPlayer.SetNewLocation(new Location((ushort)x, (ushort)y, (byte)z));

        var partyMock = new Mock<IParty>();
        partyMock.Setup(x => x.Members).Returns(new List<IPlayer>
        {
            originPlayer,
            distantPlayer
        });

        var configMock = new Mock<ISharedExperienceConfiguration>();
        configMock.Setup(x => x.RequirePartyProximity).Returns(requirePartyProximity);
        configMock.Setup(x => x.MaximumPartyDistanceToReceiveExperienceSharing).Returns(maxDistance);
        configMock.Setup(x => x.MaximumPartyVerticalDistanceToReceiveExperienceSharing).Returns(maxVerticalDistance);

        var bonus = new SharedExperienceBonus(configMock.Object);

        Assert.Equal(expectedResult, bonus.ArePartyCloseEnoughToEachOther(partyMock.Object));
    }

    [Fact]
    public void IsEveryMemberActive_ReturnsTrue_WhenNotRequired()
    {
        var playerOne = PlayerTestDataBuilder.Build(hp: 99);
        var playerTwo = PlayerTestDataBuilder.Build(2, hp: 99);

        var configMock = new Mock<ISharedExperienceConfiguration>();
        configMock.Setup(x => x.RequirePartyMemberParticipation).Returns(false);

        var partyMock = new Mock<IParty>();
        partyMock.Setup(x => x.Members).Returns(new List<IPlayer>
        {
            playerOne,
            playerTwo
        });

        var monsterMock = new Mock<IMonster>();

        var bonus = new SharedExperienceBonus(configMock.Object);

        Assert.True(bonus.IsEveryMemberActive(partyMock.Object, monsterMock.Object));
    }

    [Fact]
    public void IsEveryMemberActive_ReturnsTrue_WhenAllMembersHaveHealedAnotherRecently()
    {
        var playerOne = PlayerTestDataBuilder.Build(hp: 99);
        var playerTwo = PlayerTestDataBuilder.Build(2, hp: 99);

        var members = new List<IPlayer>
        {
            playerOne,
            playerTwo
        };
        var heals = new Dictionary<IPlayer, DateTime>
        {
            { playerOne, DateTime.UtcNow },
            { playerTwo, DateTime.UtcNow }
        };

        var partyMock = new Mock<IParty>();
        partyMock.Setup(x => x.Members).Returns(members);
        partyMock.Setup(x => x.Heals).Returns(heals);

        var monsterMock = new Mock<IMonster>();
        monsterMock.Setup(x => x.ReceivedDamages).Returns(new DamageRecordList());

        var configMock = new Mock<ISharedExperienceConfiguration>();
        configMock.Setup(x => x.RequirePartyMemberParticipation).Returns(true);
        configMock.Setup(x => x.SecondsBetweenHealsToBeConsideredActive).Returns(10);

        var bonus = new SharedExperienceBonus(configMock.Object);

        Assert.True(bonus.IsEveryMemberActive(partyMock.Object, monsterMock.Object));
    }

    [Fact]
    public void IsEveryMemberActive_ReturnsTrue_WhenAllMembersHaveAttackedTheMonsterOrHealedAPartyMember()
    {
        var playerOne = PlayerTestDataBuilder.Build(hp: 99);
        var playerTwo = PlayerTestDataBuilder.Build(2, hp: 99);

        var members = new List<IPlayer>
        {
            playerOne,
            playerTwo
        };
        var heals = new Dictionary<IPlayer, DateTime>
        {
            { playerOne, DateTime.UtcNow }
        };
        
        var damages = new DamageRecordList();
        damages.AddOrUpdateDamage(playerTwo, 10);

        var partyMock = new Mock<IParty>();
        partyMock.Setup(x => x.Members).Returns(members);
        partyMock.Setup(x => x.Heals).Returns(heals);

        var monsterMock = new Mock<IMonster>();
        monsterMock.Setup(x => x.ReceivedDamages).Returns(damages);

        var configMock = new Mock<ISharedExperienceConfiguration>();
        configMock.Setup(x => x.RequirePartyMemberParticipation).Returns(true);
        configMock.Setup(x => x.SecondsBetweenHealsToBeConsideredActive).Returns(10);

        var bonus = new SharedExperienceBonus(configMock.Object);

        Assert.True(bonus.IsEveryMemberActive(partyMock.Object, monsterMock.Object));
    }

    [Fact]
    public void IsEveryMemberActive_ReturnsTrue_WhenAllMembersHaveAttackedTheMonster()
    {
        var playerOne = PlayerTestDataBuilder.Build(hp: 99);
        var playerTwo = PlayerTestDataBuilder.Build(2, hp: 99);

        var members = new List<IPlayer>
        {
            playerOne,
            playerTwo
        };
        var heals = new Dictionary<IPlayer, DateTime>();
        
        var damages = new DamageRecordList();
        damages.AddOrUpdateDamage(playerOne, 10);
        damages.AddOrUpdateDamage(playerTwo, 10);

        var partyMock = new Mock<IParty>();
        partyMock.Setup(x => x.Members).Returns(members);
        partyMock.Setup(x => x.Heals).Returns(heals);

        var monsterMock = new Mock<IMonster>();
        monsterMock.Setup(x => x.ReceivedDamages).Returns(damages);

        var configMock = new Mock<ISharedExperienceConfiguration>();
        configMock.Setup(x => x.RequirePartyMemberParticipation).Returns(true);
        configMock.Setup(x => x.SecondsBetweenHealsToBeConsideredActive).Returns(10);

        var bonus = new SharedExperienceBonus(configMock.Object);

        Assert.True(bonus.IsEveryMemberActive(partyMock.Object, monsterMock.Object));
    }

    [InlineData(1, 0.0, 0.0, 0.2, 0.6, 1.0)]
    [InlineData(2, 0.2, 0.0, 0.2, 0.6, 1.0)]
    [InlineData(3, 0.6, 0.0, 0.2, 0.6, 1.0)]
    [InlineData(4, 1.0, 0.0, 0.2, 0.6, 1.0)]
    [InlineData(5, 1.0, 0.0, 0.2, 0.6, 1.0)]
    [Theory]
    public void GetPartyBonusFactor(int vocationCount, double expectedResult, params double[] bonusFactors)
    {
        var configMock = new Mock<ISharedExperienceConfiguration>();
        configMock.Setup(x => x.UniqueVocationBonusExperienceFactor).Returns(bonusFactors);

        var bonus = new SharedExperienceBonus(configMock.Object);

        var partyMembers = MockPartyMembers(vocationCount);
        var partyMock = new Mock<IParty>();
        partyMock.Setup(x => x.Members).Returns(partyMembers);

        var playerMock = new Mock<IPlayer>();
        playerMock.Setup(x => x.PlayerParty.IsInParty).Returns(true);
        playerMock.Setup(x => x.PlayerParty.Party).Returns(partyMock.Object);

        var monsterMock = new Mock<IMonster>();

        Assert.Equal(expectedResult, bonus.GetBonusFactorAmount(playerMock.Object, monsterMock.Object));
    }

    private List<IPlayer> MockPartyMembers(int count)
    {
        var players = new List<IPlayer>();
        for (var i = 0; i < count; i++) players.Add(MockPartyMember((byte)i));
        return players;
    }

    private IPlayer MockPartyMember(byte vocation)
    {
        var player = new Mock<IPlayer>();
        player.Setup(x => x.Vocation.Id).Returns(vocation);
        player.Setup(x => x.PlayerParty.IsInParty).Returns(true);
        return player.Object;
    }
}