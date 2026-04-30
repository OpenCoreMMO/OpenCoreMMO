using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature;

public class StoppedFollowEventHandler(IGameServer game) : INetworkingEventHandler<StoppedFollowEvent>
{
    public void Handle(StoppedFollowEvent @event)
    {
        if (@event.Creature is not IPlayer player) return;
        if (!game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection)) return;
        
        // If the player is attacking, do not cancel the target when stopped follow.
        if (player.IsAttacking)
        {
            return;
        }

        connection.OutgoingPackets.Enqueue(new CancelTargetPacket());
        connection.Send();
    }
}
