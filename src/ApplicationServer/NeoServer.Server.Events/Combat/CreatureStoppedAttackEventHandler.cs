using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Combat;

public class CreatureStoppedAttackEventHandler
{
    private readonly IGameServer game;

    public CreatureStoppedAttackEventHandler(IGameServer game)
    {
        this.game = game;
    }

    public void Execute(ICombatActor actor)
    {
        if (Guard.IsNull(actor)) return;

        if (!game.CreatureManager.GetPlayerConnection(actor.CreatureId, out var connection)) return;
        connection.OutgoingPackets.Enqueue(new CancelTargetPacket());
        connection.Send();
    }
}