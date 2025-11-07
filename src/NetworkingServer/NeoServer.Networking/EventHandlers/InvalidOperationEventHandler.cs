using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.World.Events;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers;

public class InvalidOperationEventHandler(IGameServer game) : INetworkingEventHandler<ThingMovementFailedInTheMap>
{
    private void Execute(IThing thing, InvalidOperation error)
    {
        if (thing is not IPlayer player) return;
        if (!game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection)) return;

        connection.OutgoingPackets.Enqueue(new TextMessagePacket(TextMessageOutgoingParser.Parse(error),
            TextMessageOutgoingType.Small));
        connection.Send();
    }

    public void Handle(ThingMovementFailedInTheMap @event) => Execute(@event.Thing, @event.Error);
}