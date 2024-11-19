using NeoServer.BuildingBlocks.Application;
using NeoServer.BuildingBlocks.Application.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Networking.Packets.Outgoing.Player;

namespace NeoServer.Modules.Movement.Creature.StopWalk;

public class StopWalkingPacketDispatcher(IGameCreatureManager gameCreatureManager) : ISingleton
{
    public void Send(ICreature creature)
    {
        if (creature is not IPlayer player) return;

        if (!gameCreatureManager.GetPlayerConnection(player.CreatureId, out var connection)) return;

        connection.OutgoingPackets.Enqueue(new PlayerWalkCancelPacket(player));
        connection.Send();
    }
}