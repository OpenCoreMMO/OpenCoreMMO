using NeoServer.BuildingBlocks.Application.IntegrationEvents.Creatures;
using NeoServer.BuildingBlocks.Infrastructure;

namespace NeoServer.Modules.Movement.Creature.Follow.EventHandlers;

public class CreatureCheckedEventHandler: IIntegrationEventHandler<CreatureCheckedIntegrationEvent>
{
    private readonly FollowRoutine _followRoutine;

    public CreatureCheckedEventHandler(FollowRoutine followRoutine)
    {
        _followRoutine = followRoutine;
    }
    public void Handle(CreatureCheckedIntegrationEvent @event) => _followRoutine.Run(@event.Creature);
}