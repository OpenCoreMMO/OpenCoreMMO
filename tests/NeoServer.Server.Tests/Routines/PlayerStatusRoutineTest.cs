using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Routines.Creatures.Player;
using Xunit;

namespace NeoServer.Server.Tests.Routines;

public class PlayerStatusRoutineTest
{
    [Fact]
    public void Execute_does_not_remove_logout_block_when_time_not_expired()
    {
        // Arrange

        var player = PlayerTestDataBuilder.Build();
        player.AddCondition(new CombatBlockCondition(ConditionType.LogoutBlock));

        var config = new GameConfiguration(LogoutBlockDuration: int.MaxValue);

        var mapMock = new Mock<IMap>();
        mapMock.Setup(m => m.GetSpectators(It.IsAny<Location>(), It.IsAny<bool>()))
               .Returns(new HashSet<ICreature>());

        var gameMock = new Mock<IGameServer>();
        gameMock.SetupGet(g => g.Map).Returns(mapMock.Object);

        var sut = new PlayerStatusRoutine(config, gameMock.Object);

        // Act

        sut.Execute(player);

        // Assert

        player.HasCondition(ConditionType.LogoutBlock).Should().BeTrue();
    }

    [Fact]
    public void Execute_removes_logout_block_when_expired_and_no_hostile_monsters()
    {
        // Arrange

        var player = PlayerTestDataBuilder.Build();
        player.AddCondition(new CombatBlockCondition(ConditionType.LogoutBlock));

        var config = new GameConfiguration(LogoutBlockDuration: 0);

        var mapMock = new Mock<IMap>();
        mapMock.Setup(m => m.GetSpectators(It.IsAny<Location>(), It.IsAny<bool>()))
               .Returns(new HashSet<ICreature>());

        var gameMock = new Mock<IGameServer>();
        gameMock.SetupGet(g => g.Map).Returns(mapMock.Object);

        var sut = new PlayerStatusRoutine(config, gameMock.Object);

        // Act

        sut.Execute(player);

        // Assert

        player.HasCondition(ConditionType.LogoutBlock).Should().BeFalse();
    }

    [Fact]
    public void Execute_resets_logout_block_when_expired_and_hostile_monsters_nearby()
    {
        // Arrange

        var player = PlayerTestDataBuilder.Build();
        player.AddCondition(new CombatBlockCondition(ConditionType.LogoutBlock));

        var monsterMock = new Mock<IMonster>();
        monsterMock.Setup(m => m.IsHostileTo((ICombatActor)player)).Returns(true);

        var config = new GameConfiguration(LogoutBlockDuration: 0);

        var mapMock = new Mock<IMap>();
        mapMock.Setup(m => m.GetSpectators(It.IsAny<Location>(), It.IsAny<bool>()))
               .Returns(new HashSet<ICreature> { monsterMock.Object });

        var gameMock = new Mock<IGameServer>();
        gameMock.SetupGet(g => g.Map).Returns(mapMock.Object);

        var sut = new PlayerStatusRoutine(config, gameMock.Object);

        // Act

        sut.Execute(player);

        // Assert

        player.HasCondition(ConditionType.LogoutBlock).Should().BeTrue();
    }

    [Fact]
    public void Execute_does_nothing_when_no_logout_block()
    {
        // Arrange

        var player = PlayerTestDataBuilder.Build();
        var config = new GameConfiguration(LogoutBlockDuration: 0);

        var mapMock = new Mock<IMap>();
        mapMock.Setup(m => m.GetSpectators(It.IsAny<Location>(), It.IsAny<bool>()))
               .Returns(new HashSet<ICreature>());

        var gameMock = new Mock<IGameServer>();
        gameMock.SetupGet(g => g.Map).Returns(mapMock.Object);

        var sut = new PlayerStatusRoutine(config, gameMock.Object);

        // Act & Assert

        sut.Invoking(x => x.Execute(player)).Should().NotThrow();
    }

    [Fact]
    public void Execute_does_not_remove_protection_zone_block_when_time_not_expired()
    {
        // Arrange

        var player = PlayerTestDataBuilder.Build();
        player.AddCondition(new CombatBlockCondition(ConditionType.ProtectionZoneBlock));

        var config = new GameConfiguration(ProtectionZoneBlockDuration: int.MaxValue);

        var mapMock = new Mock<IMap>();
        mapMock.Setup(m => m.GetSpectators(It.IsAny<Location>(), It.IsAny<bool>()))
               .Returns(new HashSet<ICreature>());

        var gameMock = new Mock<IGameServer>();
        gameMock.SetupGet(g => g.Map).Returns(mapMock.Object);

        var sut = new PlayerStatusRoutine(config, gameMock.Object);

        // Act

        sut.Execute(player);

        // Assert

        player.HasCondition(ConditionType.ProtectionZoneBlock).Should().BeTrue();
    }

    [Fact]
    public void Execute_removes_protection_zone_block_when_expired()
    {
        // Arrange

        var player = PlayerTestDataBuilder.Build();
        player.AddCondition(new CombatBlockCondition(ConditionType.ProtectionZoneBlock));

        var config = new GameConfiguration(ProtectionZoneBlockDuration: 0);

        var mapMock = new Mock<IMap>();
        mapMock.Setup(m => m.GetSpectators(It.IsAny<Location>(), It.IsAny<bool>()))
               .Returns(new HashSet<ICreature>());

        var gameMock = new Mock<IGameServer>();
        gameMock.SetupGet(g => g.Map).Returns(mapMock.Object);

        var sut = new PlayerStatusRoutine(config, gameMock.Object);

        // Act

        sut.Execute(player);

        // Assert

        player.HasCondition(ConditionType.ProtectionZoneBlock).Should().BeFalse();
    }
}
