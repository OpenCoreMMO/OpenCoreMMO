using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Combat;

public class CreatureStoppedAttackEventHandler(IGameCreatureManager gameCreatureManager)
    : INetworkingEventHandler<CreatureStoppedAttackEvent>
{
    public void Handle(CreatureStoppedAttackEvent @event)
    {
        var actor = @event.Actor;

        if (!gameCreatureManager.GetPlayerConnection(actor.CreatureId, out var connection)) return;
        connection.OutgoingPackets.Enqueue(new CancelTargetPacket());
        connection.Send();
    }
}
