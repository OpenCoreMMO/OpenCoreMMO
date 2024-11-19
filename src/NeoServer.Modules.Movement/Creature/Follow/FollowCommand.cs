using Mediator;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Modules.Movement.Creature.Follow;

public record FollowCommand(IWalkableCreature Creature, ICreature Target) : ICommand;

public class FollowCommandHandler(FollowService followService) : ICommandHandler<FollowCommand>
{
    public ValueTask<Unit> Handle(FollowCommand command, CancellationToken cancellationToken)
    {
        if (command.Target is null) return Unit.ValueTask;
        followService.Follow(command.Creature, command.Target);
        return Unit.ValueTask;
    }
}