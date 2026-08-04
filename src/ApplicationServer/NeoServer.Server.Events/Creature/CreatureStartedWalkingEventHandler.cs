using System.Collections.Generic;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Tasks;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Tasks;

namespace NeoServer.Server.Events.Creature;

public class CreatureStartedWalkingEventHandler(IGameServer game, ICreatureMovementService creatureMovementService, ICreatureSpeechService creatureSpeechService)
    : IApplicationEventHandler<CreatureStartedWalkingEvent>
{
    private readonly IDictionary<uint, uint> _eventWalks = new Dictionary<uint, uint>();

    public void Handle(CreatureStartedWalkingEvent @event)
    {
        if (@event is null) return;

        var creature = @event.Creature;

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
            if (nextStep.IsDrunk())
            {
                creatureSpeechService.Speak(creature, "Hicks!", SpeechType.MonsterSay);
            }
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
        Handle(new CreatureStartedWalkingEvent(creature));
    }
}
