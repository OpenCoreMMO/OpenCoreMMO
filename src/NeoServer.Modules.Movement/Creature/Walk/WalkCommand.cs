using Mediator;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Location;

namespace NeoServer.Modules.Movement.Creature.Walk;

public record WalkCommand(IWalkableCreature Creature, Direction[] Directions) : ICommand;
public record StepCommand(IWalkableCreature Creature, Direction Direction) : ICommand;

public class WalkCommandHandler(CreatureWalkScheduler walkScheduler) : ICommandHandler<WalkCommand>
{
    public ValueTask<Unit> Handle(WalkCommand command, CancellationToken cancellationToken)
    {
        var (creature, directions) = command;

        creature.AddSteps(directions);
        walkScheduler.ScheduleSteps(creature);
        return Unit.ValueTask;
    }
}

public class StepCommandHandler(CreatureWalkScheduler walkScheduler) : ICommandHandler<StepCommand>
{
    public ValueTask<Unit> Handle(StepCommand command, CancellationToken cancellationToken)
    {
        var (creature, direction) = command;

        creature.AddStep(direction);
        walkScheduler.ScheduleSteps(creature);
        return Unit.ValueTask;
    }
}