using NeoServer.Server.Commands.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Player;

public class PlayerFollowHandler(IGameServer game, PlayerFollowCommand playerFollowCommand) : PacketHandler
{
    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var targetId = message.GetUInt32();

        if (!game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;
        game.CreatureManager.TryGetCreature(targetId, out var target);
        
        game.Scheduler.AddEvent(new SchedulerEvent(200, () =>
        {
            playerFollowCommand.Execute(player, target);
        }));
    }
}