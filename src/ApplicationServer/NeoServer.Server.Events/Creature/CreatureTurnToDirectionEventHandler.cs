using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Location;
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

            connection.OutgoingPackets.Enqueue(new TransformThingPacket(creature, stackPosition));
            connection.OutgoingPackets.Enqueue(new TurnToDirectionPacket(creature, direction));

            connection.Send();
        }
    }
}