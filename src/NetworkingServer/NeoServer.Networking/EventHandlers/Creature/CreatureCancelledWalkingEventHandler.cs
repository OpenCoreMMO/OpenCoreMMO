using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature;

public class CreatureCancelledWalkingEventHandler(IGameServer game)
    : INetworkingEventHandler<CreatureCancelledWalkingEvent>
{
    public void Handle(CreatureCancelledWalkingEvent @event)
    {
        if (@event is null) return;
        if (@event.Creature is not IPlayer player) return;
        if (!game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection)) return;

        connection.OutgoingPackets.Enqueue(new PlayerWalkCancelPacket(player));
        connection.Send();
    }
}
