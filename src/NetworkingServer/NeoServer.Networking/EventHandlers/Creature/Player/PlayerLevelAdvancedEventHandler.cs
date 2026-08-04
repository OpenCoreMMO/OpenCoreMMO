using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Parsers;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature.Player;

public class PlayerLevelAdvancedEventHandler(IGameServer game) : INetworkingEventHandler<PlayerLevelAdvancedEvent>
{
    public void Handle(PlayerLevelAdvancedEvent @event)
    {
        if (@event is null) return;

        if (!game.CreatureManager.GetPlayerConnection(@event.Player.CreatureId, out var connection)) return;

        connection.OutgoingPackets.Enqueue(new TextMessagePacket(
            MessageParser.GetSkillAdvancedMessage(@event.Type, @event.FromLevel, @event.ToLevel),
            TextMessageOutgoingType.MESSAGE_EVENT_LEVEL_CHANGE));

        connection.OutgoingPackets.Enqueue(new PlayerStatusPacket(@event.Player));
        connection.Send();
    }
}
