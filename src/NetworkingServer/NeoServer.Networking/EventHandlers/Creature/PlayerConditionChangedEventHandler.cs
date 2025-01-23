using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Parsers;
using NeoServer.Game.Creatures.Models.Bases.Events;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature;

public class PlayerConditionChangedEventHandler(IGameServer game) : INetworkingEventHandler<CreatureConditionAddedEvent>
{
    public void Handle(CreatureConditionAddedEvent @event)
    {
        if (@event.Creature is not IPlayer player) return;
        if (!game.CreatureManager.GetPlayerConnection(@event.Creature.CreatureId, out var connection)) return;

        ushort icons = 0;
        
        foreach (var condition in player.Conditions)
        {
            icons |= (ushort)ConditionIconParser.Parse(condition.Key);
        }

        connection.OutgoingPackets.Enqueue(new ConditionIconPacket(icons));
        connection.Send();
    }
}