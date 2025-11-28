using System.Collections.Generic;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Tasks;

namespace NeoServer.Server.Events.Creature;

public class CreatureStartedWalkingEventHandler(IGameServer game, ICreatureMovementService creatureMovementService)
{
    private readonly IDictionary<uint, uint> _eventWalks = new Dictionary<uint, uint>();

    public void Execute(IWalkableCreature creature)
    {
        _eventWalks.TryGetValue(creature.CreatureId, out var eventWalk);

        if (eventWalk != 0) return;

        var eventId = game.Scheduler.AddEvent(new SchedulerEvent(creature.StepDelay, () => Move(creature)));
        _eventWalks.AddOrUpdate(creature.CreatureId, eventId);
    }

    private void Move(IWalkableCreature creature)
    {
        _eventWalks.TryGetValue(creature.CreatureId, out var eventWalk);

        if (creature.HasNextStep)
        {
            var nextStep = creature.GetNextStep();
            if (nextStep.IsDrunk()) creature.Say("Hicks!", SpeechType.MonsterSay);

            var nextDirection = nextStep.GetOriginalDirection();

            creatureMovementService.MoveCreature(creature, nextDirection);
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
        Execute(creature);
    }
}