using System.Threading.Tasks;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using NeoServer.Game.Common.Results;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Networking.Packets.Outgoing.Custom;
using NeoServer.Server.Commands.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Enums;
using NeoServer.Server.Configurations;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.LogIn;

public class PlayerLogInHandler : PacketHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly IGameServer _game;
    private readonly PlayerLogInCommand _playerLogInCommand;
    private readonly PlayerLogOutCommand _playerLogOutCommand;
    private readonly ClientConfiguration _clientConfiguration;
    private readonly ServerConfiguration _serverConfiguration;

    public PlayerLogInHandler(IAccountRepository repositoryNeo,
        IGameServer game, ServerConfiguration serverConfiguration, PlayerLogInCommand playerLogInCommand,
        PlayerLogOutCommand playerLogOutCommand, ClientConfiguration clientConfiguration)
    {
        _accountRepository = repositoryNeo;
        _game = game;
        _serverConfiguration = serverConfiguration;
        _playerLogInCommand = playerLogInCommand;
        _playerLogOutCommand = playerLogOutCommand;
        _clientConfiguration = clientConfiguration;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        if (_game.State == GameState.Stopped) connection.Close();

        var packet = new PlayerLogInPacket(message);

        connection.SetXtea(packet.Xtea);

        //todo linux os

        if (!Verify(connection, packet)) return;

        //todo: ip ban validation

        async void TryConnect()
        {
            await Connect(connection, packet);
        }

        _game.Dispatcher.AddEvent(new Event(TryConnect));
    }

    private async Task Connect(IConnection connection, PlayerLogInPacket packet)
    {
        var playerOnline = await _accountRepository.GetOnlinePlayer(packet.Account);

        if (ValidateOnlineStatus(connection, playerOnline, packet).Failed) return;

        var playerRecord =
            await _accountRepository.GetPlayer(packet.Account, packet.Password, packet.CharacterName);

        if (playerRecord is null)
        {
            Disconnect(connection, "Account name or password is not correct.");
            return;
        }

        if (playerRecord.Account.BanishedAt is not null)
        {
            Disconnect(connection, "Your account is banned.");
            return;
        }
        
        connection.OtcV8Version = packet.OtcV8Version;
        
        _game.Dispatcher.AddEvent(new Event(() =>
        {
            var result = _playerLogInCommand.Execute(playerRecord, connection);
            if (result.Failed)
            {
                Disconnect(connection, TextMessageOutgoingParser.Parse(result.Error));
                return;
            }

            if (packet.OtcV8Version > 0 || packet.OperatingSystem >= OperatingSystem.OtcLinux)
            {
                if (packet.OtcV8Version > 0) connection.Send(new FeaturesPacket
                {
                    GameEnvironmentEffect = _clientConfiguration.OtcV8.GameEnvironmentEffect,
                    GameExtendedOpcode = _clientConfiguration.OtcV8.GameExtendedOpcode,
                    GameExtendedClientPing = _clientConfiguration.OtcV8.GameExtendedClientPing,
                    GameItemTooltip = _clientConfiguration.OtcV8.GameItemTooltip
                });
                connection.Send(new OpcodeMessagePacket());
            }
        }));
    }

    private Result ValidateOnlineStatus(IConnection connection, PlayerEntity playerOnline,
        PlayerLogInPacket packet)
    {
        if (playerOnline is null) return Result.Success;

        _game.CreatureManager.TryGetLoggedPlayer((uint)playerOnline.Id, out var player);

        if (player?.Name == packet.CharacterName)
        {
            _playerLogOutCommand.Execute(player, true);
            return Result.Success;
        }

        if (playerOnline.Account.AllowManyOnline) return Result.Success;

        Disconnect(connection, "You may only login with one character of your account at the same time.");
        return Result.NotPossible;
    }

    private bool Verify(IConnection connection, PlayerLogInPacket packet)
    {
        if (packet.ChallengeTimeStamp != connection.TimeStamp || packet.ChallengeNumber != connection.RandomNumber)
        {
            Disconnect(connection, "Login challenge is not valid.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(packet.Account))
        {
            Disconnect(connection, "You must enter your account name.");
            return false;
        }

        if (_serverConfiguration.Version != packet.Version)
        {
            Disconnect(connection, $"Only clients with protocol {_serverConfiguration.Version} allowed!");
            return false;
        }

        switch (_game.State)
        {
            case GameState.Opening:
                Disconnect(connection, "Gameworld is starting up. Please wait.");
                return false;
            case GameState.Maintaining:
                Disconnect(connection, "Gameworld is under maintenance. Please re-connect in a while.");
                return false;
            case GameState.Closed:
                Disconnect(connection, "Server is currently closed.\nPlease try again later.");
                return false;
        }

        return true;
    }

    private static void Disconnect(IConnection connection, string message)
    {
        connection.Send(new GameServerDisconnectPacket(message));
        connection.Close();
    }
}