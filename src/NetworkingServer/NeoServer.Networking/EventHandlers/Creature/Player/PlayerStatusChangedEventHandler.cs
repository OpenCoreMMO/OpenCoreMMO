using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature.Player;

public class PlayerStatusChangedEventHandler(IGameServer game) : INetworkingEventHandler<PlayerStatusChangedEvent>
{
    public void Handle(PlayerStatusChangedEvent @event)
    {
        if (@event is null) return;

        if (!game.CreatureManager.GetPlayerConnection(@event.Player.CreatureId, out var connection)) return;

        connection.OutgoingPackets.Enqueue(new PlayerStatusPacket(@event.Player));
        connection.Send();
    }
}
