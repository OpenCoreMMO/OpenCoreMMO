using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.World.Models.Tiles;
using NeoServer.Networking.Handlers.Player;
using NeoServer.Networking.Handlers.Player.Movement;
using NeoServer.Server.Tests.Login;
using NeoServer.Server.Commands.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Routines.Creatures.Player;
using Xunit;

namespace NeoServer.Server.Tests.Functional.Conditions;

public class LogoutBlockConditionTests
{
    private readonly PlayerLogInCommand _loginCommand;
    private readonly IServiceProvider _container;
    private readonly IGameServer _game;
    private readonly IMonsterFactory _monsterFactory;
    private readonly IPlayer _player;
    private readonly PlayerAttackCommand _attackCommand;
    private readonly ICreatureGameInstance _creatureInstance;
    private readonly NeoServer.Domain.World.World _world;
    private readonly Mock<IConnection> _playerConnection;

    public LogoutBlockConditionTests()
    {
        _container = TestSetup.Setup().Result;
        _loginCommand = _container.GetService<PlayerLogInCommand>();
        _container.GetService<PlayerLogOutCommand>();
        _game = _container.GetService<IGameServer>();
        _container.GetService<GameConfiguration>();
        _monsterFactory = _container.GetService<IMonsterFactory>();
        _attackCommand = _container.GetService<PlayerAttackCommand>();
        _creatureInstance = _container.GetService<ICreatureGameInstance>();
        _world = _container.GetService<NeoServer.Domain.World.World>();
        _game.Open();

        var timestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomNumber = (byte)123;
        _playerConnection = CreateMockConnection(timestamp, randomNumber);
        var request = CreatePlayerLogInRequest(timestamp, randomNumber);
        var (success, _) = _loginCommand.Execute(request, _playerConnection.Object).Result;
        success.Should().BeTrue();

        _game.CreatureManager.TryGetLoggedPlayer(3, out var player).Should().BeTrue();
        _player = player;
    }

    [SkipOnGitHubActionsFact]
    [Trait("Category", "Condition")]
    public void Player_gets_logout_block_when_attacking_and_cannot_logout()
    {
        _player.IsLogoutBlocked.Should().BeFalse();
        _player.CannotLogout.Should().BeFalse();

        AttackThroughHandler();

        _player.IsLogoutBlocked.Should().BeTrue();
        _player.CannotLogout.Should().BeTrue();

        var logoutResult = _player.Logout(false);

        logoutResult.Should().BeFalse();
    }

    [SkipOnGitHubActionsFact]
    [Trait("Category", "Condition")]
    public void Player_can_logout_after_logout_block_expires()
    {
        AttackThroughHandler();
        _player.IsLogoutBlocked.Should().BeTrue();

        var routine = new PlayerStatusRoutine(
            new GameConfiguration(LogoutBlockDuration: 0),
            _game);

        routine.Execute(_player);

        _player.IsLogoutBlocked.Should().BeFalse();
        _player.CannotLogout.Should().BeFalse();

        var logoutResult = _player.Logout(false);
        logoutResult.Should().BeTrue();
    }

    [SkipOnGitHubActionsFact]
    [Trait("Category", "Condition")]
    public void Player_logout_block_refreshes_when_hostile_monster_nearby()
    {
        AttackThroughHandler();
        _player.IsLogoutBlocked.Should().BeTrue();

        TryPlaceMonsterNearPlayer();

        var routine = new PlayerStatusRoutine(
            new GameConfiguration(LogoutBlockDuration: 0),
            _game);

        routine.Execute(_player);

        _player.IsLogoutBlocked.Should().BeTrue();
    }

    [SkipOnGitHubActionsFact]
    [Trait("Category", "Condition")]
    public void Player_can_logout_after_monster_removed()
    {
        AttackThroughHandler();
        _player.IsLogoutBlocked.Should().BeTrue();

        var monster = TryPlaceMonsterNearPlayer();

        _game.Map.RemoveCreature(monster);

        var routine = new PlayerStatusRoutine(
            new GameConfiguration(LogoutBlockDuration: 0),
            _game);

        routine.Execute(_player);

        _player.IsLogoutBlocked.Should().BeFalse();
        _player.CannotLogout.Should().BeFalse();

        var logoutResult = _player.Logout(false);
        logoutResult.Should().BeTrue();
    }

    [SkipOnGitHubActionsFact]
    [Trait("Category", "Condition")]
    public void Player_logout_block_removed_when_entering_protection_zone()
    {
        AttackThroughHandler();
        _player.IsLogoutBlocked.Should().BeTrue();

        var playerLocation = _player.Location;
        var pzTileLocation = new Location(playerLocation.X, (ushort)(playerLocation.Y + 1), playerLocation.Z);
        var pzTile = CreateProtectionZoneTile(pzTileLocation);
        _world.ReplaceTile(pzTile);

        var addedTile = _game.Map[pzTileLocation];
        addedTile.Should().NotBeNull("PZ tile should exist on the map after ReplaceTile");
        addedTile.ProtectionZone.Should().BeTrue("PZ tile should have ProtectionZone flag");

        _playerConnection.SetupGet(c => c.CreatureId).Returns(_player.CreatureId);

        var messageMock = new Mock<IReadOnlyNetworkMessage>();
        var getByteCallCount = 0;
        messageMock.Setup(m => m.GetByte()).Returns(() =>
        {
            getByteCallCount++;
            return getByteCallCount switch
            {
                1 => 1,
                2 => 7,
                _ => 0
            };
        });

        var handler = new PlayerAutoWalkHandler(_game);
        handler.HandleMessage(messageMock.Object, _playerConnection.Object);

        WaitFor(() => _player.IsPacified, timeoutMs: 5000);

        _player.IsLogoutBlocked.Should().BeFalse();
        _player.IsPacified.Should().BeTrue();
    }

    private void AttackThroughHandler()
    {
        var monster = _monsterFactory.Create("Rat");
        monster.Should().NotBeNull();

        foreach (var location in GetNeighbourLocations())
        {
            monster.Born(location);
            _game.Map.PlaceCreature(monster);

            if (monster.Tile is null) continue;

            _creatureInstance.Add(monster);
            break;
        }

        var messageMock = new Mock<IReadOnlyNetworkMessage>();
        messageMock.Setup(m => m.GetUInt32()).Returns(monster.CreatureId);

        _playerConnection.SetupGet(c => c.CreatureId).Returns(_player.CreatureId);

        var handler = new PlayerAttackHandler(_game, _attackCommand);
        handler.HandleMessage(messageMock.Object, _playerConnection.Object);

        WaitFor(() => _player.IsLogoutBlocked, timeoutMs: 5000);
    }

    private static void WaitFor(Func<bool> condition, int timeoutMs = 5000, int pollIntervalMs = 50)
    {
        var start = DateTime.UtcNow;
        while (DateTime.UtcNow.Subtract(start).TotalMilliseconds < timeoutMs)
        {
            if (condition()) return;
            Thread.Sleep(pollIntervalMs);
        }
    }

    private IMonster TryPlaceMonsterNearPlayer()
    {
        var monster = _monsterFactory.Create("Rat");
        monster.Should().NotBeNull();

        foreach (var location in GetNeighbourLocations())
        {
            monster.Born(location);
            _game.Map.PlaceCreature(monster);

            if (monster.Tile is not null) return monster;
        }

        return monster;
    }

    private IEnumerable<Location> GetNeighbourLocations()
    {
        var playerLocation = _player.Location;
        return
        [
            new Location((ushort)(playerLocation.X + 1), playerLocation.Y, playerLocation.Z),
            new Location((ushort)(playerLocation.X - 1), playerLocation.Y, playerLocation.Z),
            new Location((ushort)(playerLocation.X + 1), (ushort)(playerLocation.Y + 1), playerLocation.Z),
            new Location((ushort)(playerLocation.X - 1), (ushort)(playerLocation.Y + 1), playerLocation.Z),
            new Location((ushort)(playerLocation.X + 1), (ushort)(playerLocation.Y - 1), playerLocation.Z),
            new Location((ushort)(playerLocation.X - 1), (ushort)(playerLocation.Y - 1), playerLocation.Z),
            new Location(playerLocation.X, (ushort)(playerLocation.Y + 1), playerLocation.Z),
            new Location(playerLocation.X, (ushort)(playerLocation.Y - 1), playerLocation.Z),
            new Location(playerLocation.X, playerLocation.Y, playerLocation.Z)
        ];
    }

    private static IDynamicTile CreateRegularTile(Location location)
    {
        var ground = MapTestDataBuilder.CreateGround(location, 1);
        return new DynamicTile(new Coordinate(location), TileFlag.None, ground, [], []);
    }

    private static IDynamicTile CreateProtectionZoneTile(Location location)
    {
        var ground = MapTestDataBuilder.CreateGround(location, 1);
        return new DynamicTile(new Coordinate(location), (TileFlag)TileFlags.ProtectionZone, ground, [], []);
    }

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

    private static PlayerLogInRequest CreatePlayerLogInRequest(uint timestamp, byte randomNumber)
    {
        return new PlayerLogInRequest
        {
            Account = "1",
            Password = "1",
            CharacterName = "Knight Sample",
            Xtea = [123456, 789012, 345678, 901234],
            OtcV8Version = 0,
            OperatingSystem = NeoServer.Server.Common.Enums.OperatingSystem.Windows,
            Version = 860,
            ChallengeTimeStamp = timestamp,
            ChallengeNumber = randomNumber
        };
    }
}
