using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Networking.Packets.Outgoing.Creature;
using NeoServer.Networking.Packets.Outgoing.Item;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Creature;

public class CreatureChangedVisibilityEventHandler
{
    private readonly IGameServer game;
    private readonly IMap map;

    public CreatureChangedVisibilityEventHandler(IMap map, IGameServer game)
    {
        this.map = map;
        this.game = game;
    }

    public void Execute(IWalkableCreature creature)
    {
        foreach (var spectator in map.GetPlayersAtPositionZone(creature.Location))
        {
            if (ReferenceEquals(spectator, creature)) continue;

            if (!game.CreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;

            if (!creature.Tile.TryGetStackPositionOfThing((IPlayer)spectator, creature, out var stackPosition))
                continue;

            if (!spectator.CanSee(creature.Location)) continue;

            if (creature.IsInvisible)
            {
                connection.OutgoingPackets.Enqueue(new RemoveTileThingPacket(creature.Tile.Location, stackPosition));
            }
            else if (spectator is IPlayer playerToSend)
            {
                connection.OutgoingPackets.Enqueue(new AddAtStackPositionPacket(creature, stackPosition));
                connection.OutgoingPackets.Enqueue(
                    playerToSend.KnowsCreatureWithId(creature.CreatureId) ?
                    new AddKnownCreaturePacket(playerToSend, creature) :
                    new AddUnknownCreaturePacket(playerToSend, creature));
            }

            connection.Send();
        }
    }
}