using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Networking.Packets.Outgoing.Window;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Player;

public class PlayerReadTextEventHandler(IGameServer game)
    : IApplicationEventHandler<PlayerReadTextEvent>
{
    public void Handle(PlayerReadTextEvent @event)
    {
        if (Guard.AnyNull(@event.Player, @event.Readable)) return;

        if (!game.CreatureManager.GetPlayerConnection(@event.Player.CreatureId, out var connection)) return;

        var id = ItemTextWindowStore.Add(@event.Player, @event.Readable);

        connection.OutgoingPackets.Enqueue(new TextWindowPacket(id, @event.Readable));

        connection.Send();
    }
}