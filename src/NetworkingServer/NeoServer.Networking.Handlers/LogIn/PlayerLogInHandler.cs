using System.Threading.Tasks;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Commands.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Enums;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.LogIn;

public class PlayerLogInHandler : PacketHandler
{
    private readonly IGameServer _game;
    private readonly PlayerLogInCommand _playerLogInCommand;

    public PlayerLogInHandler(IGameServer game, PlayerLogInCommand playerLogInCommand)
    {
        _game = game;
        _playerLogInCommand = playerLogInCommand;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        if (_game.State == GameState.Stopped) connection.Close();

        var packet = new PlayerLogInPacket(message);

        var request = new PlayerLogInRequest
        {
            Account = packet.Account,
            Password = packet.Password,
            CharacterName = packet.CharacterName,
            Xtea = packet.Xtea,
            OtcV8Version = (byte)packet.OtcV8Version,
            OperatingSystem = packet.OperatingSystem,
            Version = packet.Version,
            ChallengeTimeStamp = packet.ChallengeTimeStamp,
            ChallengeNumber = packet.ChallengeNumber
        };

        _game.Dispatcher.AddEvent(new Event(async () =>
        {
            var (success, message) = await _playerLogInCommand.Execute(request, connection);
            if (!success) Disconnect(connection, message);
        }));
    }

    private static void Disconnect(IConnection connection, string message)
    {
        connection.Send(new GameServerDisconnectPacket(message));
        connection.Close();
    }
}