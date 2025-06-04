using NeoServer.Domain.Common.Location;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Contracts.Network.Enums;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Player.Movement;

public class PlayerTurnHandler : PacketHandler
{
    private readonly IGameServer _game;

    public PlayerTurnHandler(IGameServer game)
    {
        _game = game;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var direction = ParseTurnPacket(message.IncomingPacket);

        if (!_game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        _game.Dispatcher.AddEvent(new Event(() => player.TurnTo(direction)));
    }

    private Direction ParseTurnPacket(GameIncomingPacketType turnPacket)
    {
        var direction = Direction.North;

        switch (turnPacket)
        {
            case GameIncomingPacketType.TurnNorth:
                direction = Direction.North;
                break;
            case GameIncomingPacketType.TurnEast:
                direction = Direction.East;
                break;
            case GameIncomingPacketType.TurnSouth:
                direction = Direction.South;
                break;
            case GameIncomingPacketType.TurnWest:
                direction = Direction.West;
                break;
        }

        return direction;
    }
}