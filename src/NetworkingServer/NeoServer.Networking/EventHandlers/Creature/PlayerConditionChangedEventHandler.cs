using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Parsers;
using NeoServer.Domain.Creatures.Models.Bases.Events;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature;

public class PlayerConditionChangedEventHandler(IGameServer game)
    : INetworkingEventHandler<CreatureConditionAddedEvent>, INetworkingEventHandler<CreatureConditionRemovedEvent>
{
    public void Handle(CreatureConditionAddedEvent @event)
    {
        SendPackets(@event.Creature);
    }

    public void Handle(CreatureConditionRemovedEvent @event)
    {
        SendPackets(@event.Creature);
    }

    private void SendPackets(ICreature creature)
    {
        if (creature is not IPlayer player) return;
        if (!game.CreatureManager.GetPlayerConnection(creature.CreatureId, out var connection)) return;

        ushort icons = 0;

        foreach (var condition in player.GetConditions())
        {
            icons |= (ushort)ConditionIconParser.Parse(condition.Type);
        }
     
        connection.OutgoingPackets.Enqueue(new ConditionIconPacket(icons));
        connection.Send();
    }
}