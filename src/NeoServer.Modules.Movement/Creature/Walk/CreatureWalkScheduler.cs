using NeoServer.BuildingBlocks.Application;
using NeoServer.BuildingBlocks.Application.Contracts;
using NeoServer.BuildingBlocks.Infrastructure.Threading.Scheduler;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Location;

namespace NeoServer.Modules.Movement.Creature.Walk;

public class CreatureWalkScheduler(IGameServer game, WalkService walkService)
    : ISingleton
{
    private readonly IDictionary<uint, uint> _eventWalks = new Dictionary<uint, uint>();

    public void ScheduleSteps(IWalkableCreature creature)
    {
        _eventWalks.TryGetValue(creature.CreatureId, out var eventWalk);

        if (eventWalk != 0) return;

        var eventId =
            game.Scheduler.AddEvent(new SchedulerEvent(creature.StepDelay, () => ScheduleNextMove(creature, creature.NextStep)));
        _eventWalks.AddOrUpdate(creature.CreatureId, eventId);
    }

    private void ScheduleNextMove(IWalkableCreature creature, Direction nextStep)
    {
        _eventWalks.TryGetValue(creature.CreatureId, out var eventWalk);

        if (nextStep != Direction.None)
        {
            walkService.Walk(creature, nextStep);
        }
        else
        {
            if (eventWalk != 0)
            {
                game.Scheduler.CancelEvent(eventWalk);

                eventWalk = 0;
                _eventWalks.Remove(creature.CreatureId);
            }
        }

        if (eventWalk == 0) return;

        _eventWalks.Remove(creature.CreatureId);
        ScheduleSteps(creature);
    }
}