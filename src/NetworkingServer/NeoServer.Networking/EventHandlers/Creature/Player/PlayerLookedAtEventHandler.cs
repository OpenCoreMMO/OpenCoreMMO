using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature.Player;

public class PlayerLookedAtEventHandler(IGameServer game) : INetworkingEventHandler<PlayerLookedAtEvent>
{
    public void Handle(PlayerLookedAtEvent @event)
    {
        if (@event is null) return;

        if (!game.CreatureManager.GetPlayerConnection(@event.Player.CreatureId, out var connection)) return;

        var text = @event.Thing.GetLookText(@event.IsClose, @event.Player.CanSeeInspectionDetails);

        connection.OutgoingPackets.Enqueue(new TextMessagePacket(text, TextMessageOutgoingType.Description));
        connection.Send();
    }
}
