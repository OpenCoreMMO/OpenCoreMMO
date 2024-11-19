using NeoServer.BuildingBlocks.Application.IntegrationEvents.Players;
using NeoServer.BuildingBlocks.Infrastructure;
using NeoServer.Game.Common.Creatures.Players;

namespace NeoServer.Modules.Movement.Creature.Follow.EventHandlers;

public class PlayerChangedChaseModeEventHandler(FollowService followService)
    : IIntegrationEventHandler<PlayerChangedChaseModeIntegrationEvent>
{
    public void Handle(PlayerChangedChaseModeIntegrationEvent @event)
    {
        var player = @event.Player;
        if (player.ChaseMode is ChaseMode.Follow && player.HasTarget)
        {
            followService.Follow(player, player.CurrentTarget);
        }
    }
}