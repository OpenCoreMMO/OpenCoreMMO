using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
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
                connection.OutgoingPackets.Enqueue(new RemoveTileThingPacket(creature.Tile, stackPosition));
            }
            else
            {
                game.CreatureManager.TryGetLoggedPlayer(spectator.CreatureId, out var loggedPlayer);
                connection.OutgoingPackets.Enqueue(new AddAtStackPositionPacket(map, creature, stackPosition, loggedPlayer));
                connection.OutgoingPackets.Enqueue(new AddCreaturePacket((IPlayer)spectator, creature));
            }

            connection.Send();
        }
    }
}