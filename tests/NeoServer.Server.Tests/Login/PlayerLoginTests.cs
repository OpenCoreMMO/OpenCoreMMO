using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Domain.Common.Location;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Networking.Packets.Outgoing.Custom;
using NeoServer.Networking.Packets.Outgoing.Login;
using NeoServer.Server.Commands.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Enums;
using Xunit;
using OperatingSystem = NeoServer.Server.Common.Enums.OperatingSystem;

namespace NeoServer.Server.Tests.Login;

// Custom attribute to skip tests when running on GitHub Actions
public class SkipOnGitHubActionsFactAttribute : FactAttribute
{
    public SkipOnGitHubActionsFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true") Skip = "Test skipped on GitHub Actions";
    }
}

public class PlayerLoginTests
{
    private readonly PlayerLogInCommand _command;
    private readonly IServiceProvider _container;
    private readonly NeoContext _context;
    private readonly IGameServer _game;

    public PlayerLoginTests()
    {
        _container = TestSetup.Setup().Result;
        _command = _container.GetService<PlayerLogInCommand>();
        _game = _container.GetService<IGameServer>();
        _context = _container.GetService<NeoContext>();
    }

    [SkipOnGitHubActionsFact]
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
        var (success, message) = _command.Execute(request, connection.Object);

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

    [SkipOnGitHubActionsFact]
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
        var request = CreatePlayerLogInRequest(timestamp, randomNumber, 1);

        // Act
        var (success, message) = _command.Execute(request, connection.Object);

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

    [SkipOnGitHubActionsFact]
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
        var (success, message) = _command.Execute(request, connection.Object);

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

    [SkipOnGitHubActionsFact]
    [Trait("Category", "Validation")]
    public void Player_login_fails_when_account_name_is_empty()
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
        var (success, message) = _command.Execute(request, connection.Object);

        // Assert command execution
        success.Should().BeFalse();
        message.Should().Be("You must enter your account name.");

        // Assert no packets sent (command doesn't send packets, handler does)
        connection.Verify(c => c.Send(It.IsAny<IOutgoingPacket>()), Times.Never);
        connection.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);
    }

    [SkipOnGitHubActionsFact]
    [Trait("Category", "Validation")]
    public void Player_login_fails_when_character_name_is_empty()
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
        var (success, message) = _command.Execute(request, connection.Object);

        // Assert command execution
        success.Should().BeFalse();
        message.Should().Be("Account name or password is not correct.");

        // Assert no packets sent (command doesn't send packets, handler does)
        connection.Verify(c => c.Send(It.IsAny<IOutgoingPacket>()), Times.Never);
        connection.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);
    }

    [SkipOnGitHubActionsFact]
    [Trait("Category", "Validation")]
    public void Player_login_fails_when_challenge_timestamp_mismatch()
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
        var (success, message) = _command.Execute(request, connection.Object);

        // Assert command execution
        success.Should().BeFalse();
        message.Should().Be("Login challenge is not valid.");

        // Assert no packets sent (command doesn't send packets, handler does)
        connection.Verify(c => c.Send(It.IsAny<IOutgoingPacket>()), Times.Never);
        connection.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);
    }

    [SkipOnGitHubActionsFact]
    [Trait("Category", "Validation")]
    public void Player_login_fails_when_client_version_too_low()
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
        var (success, message) = _command.Execute(request, connection.Object);

        // Assert command execution
        success.Should().BeFalse();
        message.Should().Be("Only clients with protocol 860 allowed!");

        // Assert no packets sent (command doesn't send packets, handler does)
        connection.Verify(c => c.Send(It.IsAny<IOutgoingPacket>()), Times.Never);
        connection.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);
    }

    [SkipOnGitHubActionsFact]
    [Trait("Category", "Validation")]
    public void Player_login_fails_when_client_version_too_high()
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
        var (success, message) = _command.Execute(request, connection.Object);

        // Assert command execution
        success.Should().BeFalse();
        message.Should().Be("Only clients with protocol 860 allowed!");

        // Assert no packets sent (command doesn't send packets, handler does)
        connection.Verify(c => c.Send(It.IsAny<IOutgoingPacket>()), Times.Never);
        connection.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);
    }

    [SkipOnGitHubActionsFact]
    [Trait("Category", "ErrorCondition")]
    public void Player_login_fails_when_server_is_closed()
    {
        // Arrange
        if (_game is GameServer gameServer) gameServer.Close();

        // Create challenge values
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomNumber = (byte)123;

        // Create mock connection
        var connection = CreateMockConnection(timestamp, randomNumber);

        // Create PlayerLogInRequest
        var request = CreatePlayerLogInRequest(timestamp, randomNumber);

        // Act
        var (success, message) = _command.Execute(request, connection.Object);

        // Assert command execution
        success.Should().BeFalse();
        message.Should().Be("Server is currently closed. Please try again later.");

        // Assert no packets sent (command doesn't send packets, handler does)
        connection.Verify(c => c.Send(It.IsAny<IOutgoingPacket>()), Times.Never);
        connection.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);
    }

    [SkipOnGitHubActionsFact]
    [Trait("Category", "ErrorCondition")]
    public void Player_login_fails_when_server_is_opening()
    {
        // Arrange
        if (_game is GameServer gameServer)
        {
            var stateProperty = typeof(GameServer).GetProperty("State");
            stateProperty.SetValue(gameServer, GameState.Opening);
        }

        // Create challenge values
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomNumber = (byte)123;

        // Create mock connection
        var connection = CreateMockConnection(timestamp, randomNumber);

        // Create PlayerLogInRequest
        var request = new PlayerLogInRequest
        {
            Account = "2",
            Password = "2",
            CharacterName = "Another Knight",
            Xtea = [123456, 789012, 345678, 901234],
            OtcV8Version = 0,
            OperatingSystem = OperatingSystem.Windows,
            Version = 860,
            ChallengeTimeStamp = timestamp,
            ChallengeNumber = randomNumber
        };

        // Act
        var (success, message) = _command.Execute(request, connection.Object);

        // Assert command execution
        success.Should().BeFalse();
        message.Should().Be("Gameworld is starting up. Please wait.");

        // Assert no packets sent (command doesn't send packets, handler does)
        connection.Verify(c => c.Send(It.IsAny<IOutgoingPacket>()), Times.Never);
        connection.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);
    }

    [SkipOnGitHubActionsFact]
    [Trait("Category", "ErrorCondition")]
    public void Player_login_fails_when_server_is_under_maintenance()
    {
        // Arrange
        if (_game is GameServer gameServer)
        {
            var stateProperty = typeof(GameServer).GetProperty("State");
            stateProperty.SetValue(gameServer, GameState.Maintaining);
        }

        // Create challenge values
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomNumber = (byte)123;

        // Create mock connection
        var connection = CreateMockConnection(timestamp, randomNumber);

        // Create PlayerLogInRequest
        var request = CreatePlayerLogInRequest(timestamp, randomNumber);

        // Act
        var (success, message) = _command.Execute(request, connection.Object);

        // Assert command execution
        success.Should().BeFalse();
        message.Should().Be("Gameworld is under maintenance. Please re-connect in a while.");

        // Assert no packets sent (command doesn't send packets, handler does)
        connection.Verify(c => c.Send(It.IsAny<IOutgoingPacket>()), Times.Never);
        connection.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);
    }

    [SkipOnGitHubActionsFact]
    [Trait("Category", "ErrorCondition")]
    public async Task Player_login_fails_when_ip_is_banned()
    {
        // Arrange
        _game.Open();

        // Add IP ban to database
        var banExpiresAt = new DateTime(2100, 9, 17);
        _context.IpBans.Add(new IpBanEntity
        {
            Ip = "127.0.0.1",
            Reason = "Test ban",
            ExpiresAt = banExpiresAt
        });
        await _context.SaveChangesAsync();

        // Create challenge values
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomNumber = (byte)123;

        // Create mock connection
        var connection = CreateMockConnection(timestamp, randomNumber);

        // Create PlayerLogInRequest
        var request = CreatePlayerLogInRequest(timestamp, randomNumber);

        // Act
        var (success, message) = _command.Execute(request, connection.Object);

        // Assert command execution
        success.Should().BeFalse();
        message.Should().Be("Your IP address 127.0.0.1 has been banished until 09/17/2100.\nReason: Test ban");

        // Assert no packets sent (command doesn't send packets, handler does)
        connection.Verify(c => c.Send(It.IsAny<IOutgoingPacket>()), Times.Never);
        connection.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);

        // Undo the ban
        _context.IpBans.RemoveRange(_context.IpBans.Where(b => b.Ip == "127.0.0.1"));
        await _context.SaveChangesAsync();
    }

    [SkipOnGitHubActionsFact]
    [Trait("Category", "ErrorCondition")]
    public void Player_login_fails_when_credentials_are_invalid()
    {
        // Arrange
        _game.Open();

        // Create challenge values
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomNumber = (byte)123;

        // Create mock connection
        var connection = CreateMockConnection(timestamp, randomNumber);

        // Create PlayerLogInRequest with invalid password
        var request = CreatePlayerLogInRequest(timestamp, randomNumber);
        request.Password = "invalid";

        // Act
        var (success, message) = _command.Execute(request, connection.Object);

        // Assert command execution
        success.Should().BeFalse();
        message.Should().Be("Account name or password is not correct.");

        // Assert no packets sent (command doesn't send packets, handler does)
        connection.Verify(c => c.Send(It.IsAny<IOutgoingPacket>()), Times.Never);
        connection.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);
    }

    [SkipOnGitHubActionsFact]
    [Trait("Category", "ErrorCondition")]
    public async Task Player_login_fails_when_account_is_banned()
    {
        // Arrange
        _game.Open();

        // Ban the account
        var playerEntity = await _context.Players.Include(p => p.Account).FirstOrDefaultAsync(p => p.Id == 3);
        playerEntity.Account.BanishedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Create challenge values
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomNumber = (byte)123;

        // Create mock connection
        var connection = CreateMockConnection(timestamp, randomNumber);

        // Create PlayerLogInRequest
        var request = CreatePlayerLogInRequest(timestamp, randomNumber);

        // Act
        var (success, message) = _command.Execute(request, connection.Object);

        // Assert command execution
        success.Should().BeFalse();
        message.Should().Be("Your account is banned.");

        // Assert no packets sent (command doesn't send packets, handler does)
        connection.Verify(c => c.Send(It.IsAny<IOutgoingPacket>()), Times.Never);
        connection.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);

        // Undo the ban
        playerEntity.Account.BanishedAt = null;
        await _context.SaveChangesAsync();
    }

    [SkipOnGitHubActionsFact]
    [Trait("Category", "ErrorCondition")]
    public async Task Player_login_fails_when_player_already_online_with_single_character_account()
    {
        //arrange
        _game.Open();

        // Ensure account allows many online for waiting queue test
        var account = _context.Accounts.Find(1);
        account.AllowManyOnline = false;
        await _context.SaveChangesAsync();

        // Login with first character
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomNumber = (byte)123;
        var connection1 = CreateMockConnection(timestamp, randomNumber);
        var request1 = CreatePlayerLogInRequest(timestamp, randomNumber);
        _command.Execute(request1, connection1.Object);

        // Try to login with second character from same account
        var connection2 = CreateMockConnection(timestamp, randomNumber);
        var request2 = CreatePlayerLogInRequest(timestamp, randomNumber);
        request2.CharacterName = "Druid Sample";

        // Act
        var (success, message) = _command.Execute(request2, connection2.Object);

        // Assert command execution
        success.Should().BeFalse();
        message.Should().Be("You may only login with one character of your account at the same time.");

        // Assert no packets sent (command doesn't send packets, handler does)
        connection2.Verify(c => c.Send(It.IsAny<IOutgoingPacket>()), Times.Never);
        connection2.Verify(c => c.Close(It.IsAny<bool>()), Times.Never);
    }

    [SkipOnGitHubActionsFact]
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
        _command.Execute(request, connection1.Object);

        // Get the player after first login
        _game.CreatureManager.TryGetLoggedPlayer(3, out var originalPlayer).Should().BeTrue();

        // Create second connection for reconnect
        var connection2 = CreateMockConnection(timestamp, randomNumber);

        // Act - reconnect with the same character
        var (success, message) = _command.Execute(request, connection2.Object);

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
        var directions = new[]
        {
            Direction.North, Direction.South, Direction.East, Direction.West, Direction.NorthEast, Direction.NorthWest,
            Direction.SouthEast, Direction.SouthWest
        };
        foreach (var direction in directions)
        {
            var adjacentTile = _game.Map.GetNextTile(player.Location, direction);
            if (adjacentTile != null) adjacentTile.TopCreatureOnStack.Should().NotBe(player);
        }
    }

    [SkipOnGitHubActionsFact]
    [Trait("Category", "ErrorCondition")]
    public async Task Player_login_fails_when_waiting_queue_is_full()
    {
        // Arrange
        _game.Open();

        // Set world capacity to 1
        var world = _context.Worlds.First();
        world.MaxCapacity = 1;
        await _context.SaveChangesAsync();

        // Add another account and player
        var newAccount = new AccountEntity { Id = 2, AccountName = "2", Password = "2", EmailAddress = "2" };
        _context.Accounts.Add(newAccount);
        var newPlayer = new PlayerEntity
        {
            Id = 100,
            Name = "Another Knight",
            AccountId = 2,
            WorldId = world.Id,
            PosX = 100,
            PosY = 100,
            PosZ = 7
        };
        _context.Players.Add(newPlayer);
        await _context.SaveChangesAsync();

        // Login first player
        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomNumber = (byte)123;
        var connection1 = CreateMockConnection(timestamp, randomNumber);
        var request1 = CreatePlayerLogInRequest(timestamp, randomNumber);
        _command.Execute(request1, connection1.Object);

        // Now try to login second player
        var connection2 = CreateMockConnection(timestamp, randomNumber);
        var request2 = CreatePlayerLogInRequest(timestamp, randomNumber);
        request2.Account = "2";
        request2.Password = "2";
        request2.CharacterName = "Another Knight";

        // Act
        var (success, message) = _command.Execute(request2, connection2.Object);

        // Assert command execution
        success.Should().BeFalse();
        message.Should().Contain("There are too many players online.");
        message.Should().Contain("You are at place");

        // Assert packet sent and connection closed
        connection2.Verify(c => c.Send(It.IsAny<WaitingInLinePacket>()), Times.Once);
        connection2.Verify(c => c.Close(It.IsAny<bool>()), Times.Once);

        // Cleanup
        world.MaxCapacity = 1000;
        await _context.SaveChangesAsync();
    }

    #region Helper Methods

    private static Mock<IConnection> CreateMockConnection(uint timestamp, byte randomNumber)
    {
        var connection = new Mock<IConnection>();
        connection.SetupGet(c => c.Ip).Returns("127.0.0.1:12345");
        connection.SetupGet(c => c.TimeStamp).Returns(timestamp);
        connection.SetupGet(c => c.RandomNumber).Returns(randomNumber);
        connection.Setup(c => c.SetXtea(It.IsAny<uint[]>()));
        connection.SetupGet(x => x.OutgoingPackets).Returns(new Queue<IOutgoingPacket>());
        return connection;
    }

    private static PlayerLogInRequest CreatePlayerLogInRequest(uint timestamp, byte randomNumber, byte otcV8Version = 0,
        OperatingSystem operatingSystem = OperatingSystem.Windows)
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