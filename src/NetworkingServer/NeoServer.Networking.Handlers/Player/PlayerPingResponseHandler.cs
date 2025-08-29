using System;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Player;

public sealed class PlayerPingResponseHandler : PacketHandler
{
    private readonly IGameServer _game;

    public PlayerPingResponseHandler(IGameServer game)
    {
        _game = game;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        _game.Dispatcher.AddEvent(new Event(() => connection.LastPingResponse = DateTime.UtcNow.Ticks));
    }
}