using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Player;

public class PlayerAttackHandler(IGameServer game) : PacketHandler
{
    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var targetId = message.GetUInt32();

        if (!game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        if (targetId == 0)
        {
            game.Dispatcher.AddEvent(new Event(() => player.StopAttack()));
            return;
        }

        if (!game.CreatureManager.TryGetCreature(targetId, out var creature)) return;

        game.Scheduler.AddEvent(new SchedulerEvent(200, () => player.SetAttackTarget(creature)));
    }
}