using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Networking.Packets.Outgoing.Creature;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Creature;

public class CreatureHealedEventHandler
{
    private readonly IGameServer game;
    private readonly IMap map;

    public CreatureHealedEventHandler(IMap map, IGameServer game)
    {
        this.map = map;
        this.game = game;
    }

    public void Execute(ICreature healedCreature, ICreature healer, ushort amount)
    {
        foreach (var spectator in map.GetPlayersAtPositionZone(healedCreature.Location))
        {
            if (!game.CreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;

            if (Equals(healedCreature, spectator)) //myself
                connection.OutgoingPackets.Enqueue(new PlayerStatusPacket((IPlayer)healedCreature));

            connection.OutgoingPackets.Enqueue(new CreatureHealthPacket(healedCreature));

            connection.Send();
        }
    }
}