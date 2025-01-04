using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Player;

public class ExtendedOpcodeHandler : PacketHandler
{
    private readonly IGameServer _game;

    public ExtendedOpcodeHandler(IGameServer game)
    {
        _game = game;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        if (_game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player))
            _game.Dispatcher.AddEvent(new Event(() => player.ExtendedOpcode(message.GetByte(), message.GetString())));
    }
}