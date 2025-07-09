using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Networking.Packets.Outgoing.Creature;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Creature;

public class CreatureChangedLightEventHandler(IMap map, IGameServer game)
{
    public void Execute(ICreature creature)
    {
        foreach (var spectator in map.GetPlayersAtPositionZone(creature.Location))
        {
            if (!creature.CanSee(spectator.Location)) continue;

            if (!game.CreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;
            connection.OutgoingPackets.Enqueue(new CreatureLightPacket(creature as IPlayer));
            connection.Send();
        }
    }
}