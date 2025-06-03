using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location;
using NeoServer.Networking.Packets.Outgoing.Creature;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Creature;

public class CreatureTurnedToDirectionEventHandler
{
    private readonly IGameServer game;
    private readonly IMap map;

    public CreatureTurnedToDirectionEventHandler(IMap map, IGameServer game)
    {
        this.map = map;
        this.game = game;
    }

    public void Execute(IWalkableCreature creature, Direction direction)
    {
        if (Guard.AnyNull(creature, direction)) return;

        if (creature.IsInvisible) return;

        foreach (var spectator in map.GetSpectators(creature.Location, true))
        {
            if (!spectator.CanSee(creature.Location)) continue;

            if (!game.CreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;

            if (!game.CreatureManager.TryGetPlayer(spectator.CreatureId, out var player)) continue;

            if (!creature.Tile.TryGetStackPositionOfThing(player, creature, out var stackPosition)) continue;

            connection.OutgoingPackets.Enqueue(new TurnToDirectionPacket(creature, direction, stackPosition));

            connection.Send();
        }
    }
}