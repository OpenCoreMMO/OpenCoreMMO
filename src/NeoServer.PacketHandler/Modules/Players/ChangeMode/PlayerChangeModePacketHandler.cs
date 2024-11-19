using Mediator;
using NeoServer.BuildingBlocks.Application.Contracts;
using NeoServer.BuildingBlocks.Infrastructure.Threading.Event;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Modules.Movement.Creature.Follow;
using NeoServer.Modules.Players.ChangeMode;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Networking.Packets.Network;

namespace NeoServer.PacketHandler.Modules.Players.ChangeMode;

public class PlayerChangesModePacketHandler : PacketHandler
{
    private readonly IGameServer _game;
    private readonly IMediator _mediator;

    public PlayerChangesModePacketHandler(IGameServer game, IMediator mediator)
    {
        _game = game;
        _mediator = mediator;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var changeMode = new ChangeModePacket(message);

        if (!_game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        _game.Dispatcher.AddEvent(new Event(() =>
        {
            _mediator.Send(new ChangeModeCommand(player, changeMode.FightMode, changeMode.ChaseMode,
                changeMode.SecureMode));

   
        }));
    }
}