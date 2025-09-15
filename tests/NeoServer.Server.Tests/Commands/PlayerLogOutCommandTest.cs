using Moq;
using NeoServer.Data.Interfaces;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.Locker;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Commands.Player;
using Xunit;

namespace NeoServer.Server.Tests.Commands;

public class PlayerLogOutCommandTest
{
    [Fact]
    public void Execute_When_Player_Is_Dead_Should_Not_Logout_Player()
    {
        // Arrange
        var gameServer = new Mock<IGameServer>();
        var playerRepository = new Mock<IPlayerRepository>();
        var playerDepotItemRepository = new Mock<IPlayerDepotItemRepository>();
        var playerMailItemRepository = new Mock<IPlayerMailItemRepository>();
        var lockerManager = new Mock<LockerManager>();
        var tradeService = new Mock<ITradeService>();
        var chatChannelStore = new Mock<IChatChannelStore>();
        var playerChannelService = new PlayerChannelService(chatChannelStore.Object);
        var map = new Mock<IMap>();

        var player = new Mock<IPlayer>();
        player.Setup(p => p.IsDead).Returns(true);

        var command = new PlayerLogOutCommand(
            gameServer.Object,
            playerRepository.Object,
            playerDepotItemRepository.Object,
            playerMailItemRepository.Object,
            lockerManager.Object,
            tradeService.Object,
            playerChannelService,
            map.Object);

        // Act
        command.Execute(player.Object);

        // Assert
        player.Verify(p => p.Logout(It.IsAny<bool>()), Times.Never);
        gameServer.Verify(g => g.CreatureManager.RemovePlayer(It.IsAny<IPlayer>()), Times.Never);
    }

    [Fact]
    public void Execute_When_Player_Logout_Returns_False_And_Not_Forced_Should_Not_Continue()
    {
        // Arrange
        var gameServer = new Mock<IGameServer>();
        var playerRepository = new Mock<IPlayerRepository>();
        var playerDepotItemRepository = new Mock<IPlayerDepotItemRepository>();
        var playerMailItemRepository = new Mock<IPlayerMailItemRepository>();
        var lockerManager = new Mock<LockerManager>();
        var tradeService = new Mock<ITradeService>();
        var chatChannelStore = new Mock<IChatChannelStore>();
        var playerChannelService = new PlayerChannelService(chatChannelStore.Object);
        var map = new Mock<IMap>();

        var player = new Mock<IPlayer>();
        player.Setup(p => p.IsDead).Returns(false);
        player.Setup(p => p.Logout(false)).Returns(false);

        var command = new PlayerLogOutCommand(
            gameServer.Object,
            playerRepository.Object,
            playerDepotItemRepository.Object,
            playerMailItemRepository.Object,
            lockerManager.Object,
            tradeService.Object,
            playerChannelService,
            map.Object);

        // Act
        command.Execute(player.Object);

        // Assert
        player.Verify(p => p.Logout(false), Times.Once);
        gameServer.Verify(g => g.CreatureManager.RemovePlayer(It.IsAny<IPlayer>()), Times.Never);
    }
}