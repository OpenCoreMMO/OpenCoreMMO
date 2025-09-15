using System;
using System.Collections.Generic;
using NeoServer.Data.Contexts;
using NeoServer.Server.Commands.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using Xunit;
using Moq;
using NeoServer.Networking.Packets.Outgoing;
using FluentAssertions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OperatingSystem = NeoServer.Server.Common.Enums.OperatingSystem;

namespace NeoServer.Server.Tests.Login;

public class PlayerLoginTests
{
    private readonly IServiceProvider _container;
    private readonly PlayerLogInCommand _command;
    private readonly IGameServer _game;
    private readonly NeoContext _context;

    public PlayerLoginTests()
    {
         _container = TestSetup.Setup().Result;
         _command = _container.GetService<PlayerLogInCommand>();
         _game = _container.GetService<IGameServer>();
         _context = _container.GetService<NeoContext>();
    }
    
    [Fact]
    [Trait("Category", "HappyPath")]
    public async Task Player_gets_loaded_and_placed_on_map_when_login_succeeds()
    {
        // Arrange
        _game.Open();
        
        // Create challenge values
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomNumber = (byte)123;

        // Create mock connection
        var connection = CreateMockConnection(timestamp, randomNumber);

        // Create PlayerLogInRequest with valid data
        var request = CreatePlayerLogInRequest(timestamp, randomNumber);

        // Act
        var (success, message) = await _command.Execute(request, connection.Object);

        // Assert command execution
        success.Should().BeTrue();
        message.Should().BeNull();

        // Assert
        // Verify player is loaded and placed on map
        _game.CreatureManager.TryGetLoggedPlayer(3, out var player).Should().BeTrue();
        player.Should().NotBeNull();
        player.Name.Should().Be("Knight Sample");

        // Verify player appears in game world at correct location
        var tile = _game.Map[player.Location];
        tile.Should().NotBeNull();
        tile.TopCreatureOnStack.Should().Be(player);

        // Check DB: online status = true, last login updated
        var playerEntity = await _context.Players.FindAsync(3);
        playerEntity.Online.Should().BeTrue();
        playerEntity.LastLogIn.Should().NotBeNull();

        // Assert no disconnect packet received
        connection.Verify(c => c.Send(It.IsAny<GameServerDisconnectPacket>()), Times.Never);
        connection.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);

        // Verify VIP list loaded
        player.Vip.Should().NotBeNull();
    }

    #region Helper Methods

    private static Mock<IConnection> CreateMockConnection(uint timestamp, byte randomNumber)
    {
        var connection = new Mock<IConnection>();
        connection.SetupGet(c => c.Ip).Returns("127.0.0.1:12345");
        connection.SetupGet(c => c.TimeStamp).Returns(timestamp);
        connection.SetupGet(c => c.RandomNumber).Returns(randomNumber);
        connection.Setup(c => c.SetXtea(It.IsAny<uint[]>()));
        connection.SetupGet(x=>x.OutgoingPackets).Returns(new Queue<IOutgoingPacket>());
        return connection;
    }
    private static PlayerLogInRequest CreatePlayerLogInRequest(uint timestamp, byte randomNumber)
    {
        return new PlayerLogInRequest
        {
            Account = "1",
            Password = "1",
            CharacterName = "Knight Sample",
            Xtea = [123456, 789012, 345678, 901234],
            OtcV8Version = 0,
            OperatingSystem = OperatingSystem.Windows,
            Version = 860,
            ChallengeTimeStamp = timestamp,
            ChallengeNumber = randomNumber
        };
    }


    #endregion
}