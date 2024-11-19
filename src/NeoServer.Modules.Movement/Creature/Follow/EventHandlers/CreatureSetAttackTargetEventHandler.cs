using NeoServer.BuildingBlocks.Application.IntegrationEvents.Combat;
using NeoServer.BuildingBlocks.Infrastructure;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures.Players;

namespace NeoServer.Modules.Movement.Creature.Follow.EventHandlers;

public class CreatureSetAttackTargetEventHandler(FollowService followService): IIntegrationEventHandler<CreatureSetAttackTargetEvent>
{
    public void Handle(CreatureSetAttackTargetEvent @event)
    {
        var aggressor = @event.Aggressor;
        
        if (aggressor is not IWalkableCreature follower) return;
        
        if (aggressor is IPlayer { ChaseMode: ChaseMode.Follow, HasTarget: true } player)
        {
            followService.Follow(player, @event.Target);
        }
    }
}