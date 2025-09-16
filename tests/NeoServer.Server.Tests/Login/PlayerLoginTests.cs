using System;
using System.Reflection;
using System.Collections.Generic;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using NeoServer.Domain.Common.Location;
using NeoServer.Server.Commands.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Enums;
using Xunit;
using Moq;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Networking.Packets.Outgoing.Login;
using NeoServer.Networking.Packets.Outgoing.Custom;
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

    [Fact]
    [Trait("Category", "HappyPath")]
    public async Task Player_logs_in_successfully_with_OTCv8_client()
    {
        // Arrange
        _game.Open();

        // Create challenge values
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomNumber = (byte)123;

        // Create mock connection
        var connection = CreateMockConnection(timestamp, randomNumber);

        // Create PlayerLogInRequest with valid data and OTCv8 enabled
        var request = CreatePlayerLogInRequest(timestamp, randomNumber, otcV8Version: 1);

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

        // Verify OTCv8 packets sent
        connection.Verify(c => c.Send(It.IsAny<FeaturesPacket>()), Times.Once);
        connection.Verify(c => c.Send(It.IsAny<OpcodeMessagePacket>()), Times.Once);

        // Verify VIP list loaded
        player.Vip.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public async Task Player_logs_in_successfully_with_OTC_Linux_client()
    {
        // Arrange
        _game.Open();

        // Create challenge values
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomNumber = (byte)123;

        // Create mock connection
        var connection = CreateMockConnection(timestamp, randomNumber);

        // Create PlayerLogInRequest with valid data and OTC Linux OS
        var request = CreatePlayerLogInRequest(timestamp, randomNumber, operatingSystem: OperatingSystem.OtcLinux);

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

        // Verify OTC Linux packets sent: only OpcodeMessagePacket, no FeaturesPacket
        connection.Verify(c => c.Send(It.IsAny<OpcodeMessagePacket>()), Times.Once);
        connection.Verify(c => c.Send(It.IsAny<FeaturesPacket>()), Times.Never);

        // Verify VIP list loaded
        player.Vip.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public async Task Player_login_fails_when_account_name_is_empty()
    {
        // Arrange
        _game.Open();

        // Create challenge values
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomNumber = (byte)123;

        // Create mock connection
        var connection = CreateMockConnection(timestamp, randomNumber);

        // Create PlayerLogInRequest with empty account name
        var request = CreatePlayerLogInRequest(timestamp, randomNumber);
        request.Account = "";

        // Act
        var (success, message) = await _command.Execute(request, connection.Object);

        // Assert command execution
        success.Should().BeFalse();
        message.Should().Be("You must enter your account name.");

        // Assert no packets sent (command doesn't send packets, handler does)
        connection.Verify(c => c.Send(It.IsAny<IOutgoingPacket>()), Times.Never);
        connection.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public async Task Player_login_fails_when_character_name_is_empty()
    {
        // Arrange
        _game.Open();

        // Create challenge values
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomNumber = (byte)123;

        // Create mock connection
        var connection = CreateMockConnection(timestamp, randomNumber);

        // Create PlayerLogInRequest with empty character name
        var request = CreatePlayerLogInRequest(timestamp, randomNumber);
        request.CharacterName = "";

        // Act
        var (success, message) = await _command.Execute(request, connection.Object);

        // Assert command execution
        success.Should().BeFalse();
        message.Should().Be("Account name or password is not correct.");

        // Assert no packets sent (command doesn't send packets, handler does)
        connection.Verify(c => c.Send(It.IsAny<IOutgoingPacket>()), Times.Never);
        connection.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public async Task Player_login_fails_when_challenge_timestamp_mismatch()
    {
        // Arrange
        _game.Open();

        // Create challenge values
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomNumber = (byte)123;

        // Create mock connection
        var connection = CreateMockConnection(timestamp, randomNumber);

        // Create PlayerLogInRequest with mismatched challenge timestamp
        var request = CreatePlayerLogInRequest(timestamp, randomNumber);
        request.ChallengeTimeStamp = timestamp + 1; // Mismatch

        // Act
        var (success, message) = await _command.Execute(request, connection.Object);

        // Assert command execution
        success.Should().BeFalse();
        message.Should().Be("Login challenge is not valid.");

        // Assert no packets sent (command doesn't send packets, handler does)
        connection.Verify(c => c.Send(It.IsAny<IOutgoingPacket>()), Times.Never);
        connection.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public async Task Player_login_fails_when_client_version_too_low()
    {
        // Arrange
        _game.Open();

        // Create challenge values
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomNumber = (byte)123;

        // Create mock connection
        var connection = CreateMockConnection(timestamp, randomNumber);

        // Create PlayerLogInRequest with version too low
        var request = CreatePlayerLogInRequest(timestamp, randomNumber);
        request.Version = 850; // Too low

        // Act
        var (success, message) = await _command.Execute(request, connection.Object);

        // Assert command execution
        success.Should().BeFalse();
        message.Should().Be("Only clients with protocol 860 allowed!");

        // Assert no packets sent (command doesn't send packets, handler does)
        connection.Verify(c => c.Send(It.IsAny<IOutgoingPacket>()), Times.Never);
        connection.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public async Task Player_login_fails_when_client_version_too_high()
    {
        // Arrange
        _game.Open();

        // Create challenge values
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomNumber = (byte)123;

        // Create mock connection
        var connection = CreateMockConnection(timestamp, randomNumber);

        // Create PlayerLogInRequest with version too high
        var request = CreatePlayerLogInRequest(timestamp, randomNumber);
        request.Version = 870; // Too high

        // Act
        var (success, message) = await _command.Execute(request, connection.Object);

        // Assert command execution
        success.Should().BeFalse();
        message.Should().Be("Only clients with protocol 860 allowed!");

        // Assert no packets sent (command doesn't send packets, handler does)
        connection.Verify(c => c.Send(It.IsAny<IOutgoingPacket>()), Times.Never);
        connection.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "ErrorCondition")]
    public async Task Player_login_fails_when_server_is_stopped()
    {
        // Arrange
        if (_game is NeoServer.Server.GameServer gameServer)
        {
            gameServer.Close();
        }

        // Create challenge values
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomNumber = (byte)123;

        // Create mock connection
        var connection = CreateMockConnection(timestamp, randomNumber);

        // Create PlayerLogInRequest
        var request = CreatePlayerLogInRequest(timestamp, randomNumber);

        // Act
        var (success, message) = await _command.Execute(request, connection.Object);

        // Assert command execution
        success.Should().BeFalse();
        message.Should().Be("Server is currently closed. Please try again later.");

        // Assert no packets sent (command doesn't send packets, handler does)
        connection.Verify(c => c.Send(It.IsAny<IOutgoingPacket>()), Times.Never);
        connection.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "ErrorCondition")]
    public async Task Player_login_fails_when_server_is_opening()
    {
        // Arrange
        if (_game is NeoServer.Server.GameServer gameServer)
        {
            var stateProperty = typeof(NeoServer.Server.GameServer).GetProperty("State");
            stateProperty.SetValue(gameServer, GameState.Opening);
        }

        // Create challenge values
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomNumber = (byte)123;

        // Create mock connection
        var connection = CreateMockConnection(timestamp, randomNumber);

        // Create PlayerLogInRequest
        var request = CreatePlayerLogInRequest(timestamp, randomNumber);

        // Act
        var (success, message) = await _command.Execute(request, connection.Object);

        // Assert command execution
        success.Should().BeFalse();
        message.Should().Be("Gameworld is starting up. Please wait.");

        // Assert no packets sent (command doesn't send packets, handler does)
        connection.Verify(c => c.Send(It.IsAny<IOutgoingPacket>()), Times.Never);
        connection.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public async Task Player_reconnects_successfully_when_logging_in_with_existing_online_character()
    {
        // Arrange
        _game.Open();

        // Create challenge values
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomNumber = (byte)123;

        // Create first connection and login
        var connection1 = CreateMockConnection(timestamp, randomNumber);
        var request = CreatePlayerLogInRequest(timestamp, randomNumber);
        await _command.Execute(request, connection1.Object);

        // Get the player after first login
        _game.CreatureManager.TryGetLoggedPlayer(3, out var originalPlayer).Should().BeTrue();

        // Create second connection for reconnect
        var connection2 = CreateMockConnection(timestamp, randomNumber);

        // Act - reconnect with the same character
        var (success, message) = await _command.Execute(request, connection2.Object);

        // Assert command execution
        success.Should().BeTrue();
        message.Should().BeNull();

        // Assert
        // Verify player is still loaded and placed on map (same instance, no duplication)
        _game.CreatureManager.TryGetLoggedPlayer(3, out var player).Should().BeTrue();
        player.Should().NotBeNull();
        player.Should().BeSameAs(originalPlayer); // Ensure no player duplication
        player.Name.Should().Be("Knight Sample");

        // Verify player appears in game world at correct location
        var tile = _game.Map[player.Location];
        tile.Should().NotBeNull();
        tile.TopCreatureOnStack.Should().Be(player);

        // Check DB: online status = true, last login updated
        var playerEntity = await _context.Players.FindAsync(3);
        playerEntity.Online.Should().BeTrue();
        playerEntity.LastLogIn.Should().NotBeNull();

        // Assert first connection was disconnected
        connection1.Verify(c => c.Disconnect(It.IsAny<string>()), Times.Once);

        // Assert second connection received no disconnect packet
        connection2.Verify(c => c.Send(It.IsAny<GameServerDisconnectPacket>()), Times.Never);
        connection2.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);

        // Verify VIP list loaded
        player.Vip.Should().NotBeNull();

        // Verify player is not duplicated on adjacent tiles
        var directions = new[] { Direction.North, Direction.South, Direction.East, Direction.West, Direction.NorthEast, Direction.NorthWest, Direction.SouthEast, Direction.SouthWest };
        foreach (var direction in directions)
        {
            var adjacentTile = _game.Map.GetNextTile(player.Location, direction);
            if (adjacentTile != null)
            {
                adjacentTile.TopCreatureOnStack.Should().NotBe(player);
            }
        }
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
    
    private static PlayerLogInRequest CreatePlayerLogInRequest(uint timestamp, byte randomNumber, byte otcV8Version = 0, OperatingSystem operatingSystem = OperatingSystem.Windows)
    {
        return new PlayerLogInRequest
        {
            Account = "1",
            Password = "1",
            CharacterName = "Knight Sample",
            Xtea = [123456, 789012, 345678, 901234],
            OtcV8Version = otcV8Version,
            OperatingSystem = operatingSystem,
            Version = 860,
            ChallengeTimeStamp = timestamp,
            ChallengeNumber = randomNumber
        };
    }

    #endregion
}