using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature.Player;

public class PlayerUsedItemEventHandler(IGameServer game) : INetworkingEventHandler<PlayerUsedItemEvent>
{
    public void Handle(PlayerUsedItemEvent @event)
    {
        if (@event is null) return;

        if (@event.Item.Effect == EffectT.None) return;

        foreach (var spectator in game.Map.GetPlayersAtPositionZone(@event.Thing.Location))
        {
            if (!game.CreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;

            connection.OutgoingPackets.Enqueue(new MagicEffectPacket(@event.Thing.Location, @event.Item.Effect));
            connection.Send();
        }
    }
}
