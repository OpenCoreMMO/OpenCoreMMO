using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Moq;
using NeoServer.Data.InMemory.DataStores;
using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Creatures.Monster.Summon;
using NeoServer.Game.Creatures.Services;
using NeoServer.Game.Tests.Helpers;
using NeoServer.Game.Tests.Helpers.Player;
using Xunit;

namespace NeoServer.Game.Creatures.Tests.Events.Monsters;

public class MonsterKilledEventHandlerTest
{
    [Fact]
    public void Execute_GrantsMonsterExperience_WhenMonsterKilledByOnePlayer()
    {
        var player = PlayerTestDataBuilder.Build();
        
        var damages = new DamageRecordList();
        damages.AddOrUpdateDamage(player, 100);

        var monsterMock = new Mock<IMonster>();
        monsterMock.Setup(x => x.Experience).Returns(100);
        monsterMock.Setup(x => x.ReceivedDamages).Returns(damages);

        var experienceSharingService = new ExperienceSharingService();

        var before = player.Experience;
        experienceSharingService.Share(monsterMock.Object);
        var after = player.Experience;

        Assert.Equal(before + 100, after);
    }

    [Fact]
    public void Execute_GrantsProportionalMonsterExperience_WhenMonsterKilledByTwoPlayers()
    {
        var playerOne = PlayerTestDataBuilder.Build();
        var playerTwo = PlayerTestDataBuilder.Build(2);
        
        var damages = new DamageRecordList();
        damages.AddOrUpdateDamage(playerOne, 100);
        damages.AddOrUpdateDamage(playerTwo, 100);

        var monsterMock = new Mock<IMonster>();
        monsterMock.Setup(x => x.Experience).Returns(100);
        monsterMock.Setup(x => x.ReceivedDamages).Returns(damages);

        var experienceSharingService = new ExperienceSharingService();

        var playerOneBefore = playerOne.Experience;
        var playerTwoBefore = playerTwo.Experience;
        experienceSharingService.Share(monsterMock.Object);
        var playerOneAfter = playerOne.Experience;
        var playerTwoAfter = playerTwo.Experience;

        Assert.Equal(playerOneBefore + 50, playerOneAfter);
        Assert.Equal(playerTwoBefore + 50, playerTwoAfter);
    }

    [Fact]
    public void Execute_ConsidersSummonDamageAsMasterDamage_WhenCalculatingExperienceProportionally()
    {
        var playerOne = PlayerTestDataBuilder.Build();
        var playerTwo = PlayerTestDataBuilder.Build(2);
        var playerOneSummon = MockSummon(playerOne);
 
        var damages = new DamageRecordList();
        damages.AddOrUpdateDamage(playerOne, 100);
        damages.AddOrUpdateDamage(playerTwo, 100);
        damages.AddOrUpdateDamage(playerOneSummon, 200);

        var monsterMock = new Mock<IMonster>();
        monsterMock.Setup(x => x.Experience).Returns(100);
        monsterMock.Setup(x => x.ReceivedDamages).Returns(damages);

        var experienceSharingService = new ExperienceSharingService();

        var playerOneBefore = playerOne.Experience;
        var playerTwoBefore = playerTwo.Experience;
        experienceSharingService.Share(monsterMock.Object);
        var playerOneAfter = playerOne.Experience;
        var playerTwoAfter = playerTwo.Experience;

        Assert.Equal(playerOneBefore + 75, playerOneAfter);
        Assert.Equal(playerTwoBefore + 25, playerTwoAfter);
    }

    [Fact]
    public void Execute_GrantsProportionalMonsterExperience_WhenSharedExperienceIsDisabled()
    {
        var playerOne = PlayerTestDataBuilder.Build();
        var playerTwo = PlayerTestDataBuilder.Build(2);
        var party = PartyTestDataBuilder.Build(null, playerOne, playerTwo);
        
        var damages = new DamageRecordList();
        damages.AddOrUpdateDamage(playerOne, 300);
        damages.AddOrUpdateDamage(playerTwo, 100);

        var monsterMock = new Mock<IMonster>();
        monsterMock.Setup(x => x.Experience).Returns(100);
        monsterMock.Setup(x => x.ReceivedDamages).Returns(damages);

        var experienceSharingService = new ExperienceSharingService();

        var playerOneBefore = playerOne.Experience;
        var playerTwoBefore = playerTwo.Experience;
        experienceSharingService.Share(monsterMock.Object);
        var playerOneAfter = playerOne.Experience;
        var playerTwoAfter = playerTwo.Experience;

        Assert.Equal(playerOneBefore + 75, playerOneAfter);
        Assert.Equal(playerTwoBefore + 25, playerTwoAfter);
    }

    [Fact]
    public void Execute_GrantsProportionalMonsterExperienceAndBonus_WhenSharedExperienceIsEnabled()
    {
        var vocationStore = MockVocations(1, 2);

        // p1 and p2 will be in a party. p3 will be solo.
        var playerOne = PlayerTestDataBuilder.Build(vocationType: 1, vocationStore: vocationStore);
        var playerTwo = PlayerTestDataBuilder.Build(2, vocationType: 2, vocationStore: vocationStore);
        var playerThree = PlayerTestDataBuilder.Build(3, vocationType: 3, vocationStore: vocationStore);

        var party = PartyTestDataBuilder.Build(null, playerOne, playerTwo);
        party.IsSharedExperienceEnabled = true;

        var heals = new Dictionary<IPlayer, DateTime>();
      
        var damages = new DamageRecordList();
        damages.AddOrUpdateDamage(playerOne, 200);
        damages.AddOrUpdateDamage(playerTwo, 100);
        damages.AddOrUpdateDamage(playerThree, 100);

        var monsterMock = new Mock<IMonster>();
        monsterMock.Setup(x => x.Experience).Returns(100);
        monsterMock.Setup(x => x.ReceivedDamages).Returns(damages);

        var experienceSharingService = new ExperienceSharingService();

        var playerOneBefore = playerOne.Experience;
        var playerTwoBefore = playerTwo.Experience;
        var playerThreeBefore = playerThree.Experience;
        experienceSharingService.Share(monsterMock.Object);
        var playerOneAfter = playerOne.Experience;
        var playerTwoAfter = playerTwo.Experience;
        var playerThreeAfter = playerThree.Experience;

        Assert.Equal(playerOneBefore + 60, playerOneAfter);
        Assert.Equal(playerTwoBefore + 30, playerTwoAfter);
        Assert.Equal(playerThreeBefore + 25, playerThreeAfter);
    }

    /// <summary>
    ///     Mocks a summon for the specified player.
    /// </summary>
    /// <param name="player">The master of the summon.</param>
    private Summon MockSummon(IPlayer player)
    {
        var monsterTypeMock = new Mock<IMonsterType>();
        var mapTool = new Mock<IMapTool>();
        monsterTypeMock.Setup(x => x.Name).Returns("Test Summon");
        monsterTypeMock.Setup(x => x.Look).Returns(new Dictionary<LookType, ushort>
        {
            { LookType.Type, 0 },
            { LookType.Body, 0 },
            { LookType.Feet, 0 },
            { LookType.Head, 0 },
            { LookType.Legs, 0 }
        });

        return new Summon(monsterTypeMock.Object, mapTool.Object, player);
    }

    /// <summary>
    ///     Mocks and loads a vocation matching each number (VocationType byte).
    /// </summary>
    private IVocationStore MockVocations(params int[] vocations)
    {
        var vocationStore = new VocationStore();
        var mockedVocations = vocations.Select(x =>
        {
            var mock = new Mock<IVocation>();
            mock.Setup(x => x.Id).Returns(x.ToString());
            mock.Setup(x => x.Name).Returns(x.ToString());
            mock.Setup(x => x.VocationType).Returns((byte)x);
            return mock.Object;
        });

        foreach (var vocation in mockedVocations) vocationStore.Add(vocation.VocationType, vocation);

        return vocationStore;
    }
}