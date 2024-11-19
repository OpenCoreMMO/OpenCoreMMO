using Mediator;
using NeoServer.BuildingBlocks.Application.Contracts;
using NeoServer.BuildingBlocks.Infrastructure.Threading.Event;
using NeoServer.BuildingBlocks.Infrastructure.Threading.Scheduler;
using NeoServer.Modules.Combat.PlayerAttack;
using NeoServer.Networking.Packets.Network;

namespace NeoServer.PacketHandler.Modules.Combat.AutoAttack;

public class PlayerAttackPacketHandler(IGameServer game, IMediator mediator) : PacketHandler
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

        if (!game.CreatureManager.TryGetCreature(targetId, out var target)) return;

        game.Scheduler.AddEvent(new SchedulerEvent(200, () => mediator.Send(new SetAttackTargetCommand(player, target))));
    }
}