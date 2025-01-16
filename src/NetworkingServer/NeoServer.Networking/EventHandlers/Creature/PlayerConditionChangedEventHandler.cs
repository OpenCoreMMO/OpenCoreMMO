using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Parsers;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature;

public class PlayerConditionChangedEventHandler(IGameServer game) : INetworkEventHandler<ICreature>
{
    private void Execute(ICreature creature, ICondition c)
    {
        if (creature is not IPlayer player) return;
        if (!game.CreatureManager.GetPlayerConnection(creature.CreatureId, out var connection)) return;
        
        ushort icons = 0;
        foreach (var condition in player.Conditions) icons |= (ushort)ConditionIconParser.Parse(condition.Key);

        connection.OutgoingPackets.Enqueue(new ConditionIconPacket(icons));
        connection.Send();
    }
    public void Subscribe(ICreature entity)
    {
        if (entity is not IPlayer player) return;
        
        player.OnAddedCondition += Execute;
        player.OnRemovedCondition += Execute;
    }
    public void Unsubscribe(ICreature entity)
    {
        if (entity is not IPlayer player) return;
        
        player.OnAddedCondition -= Execute;
        player.OnRemovedCondition -= Execute;
    }
}