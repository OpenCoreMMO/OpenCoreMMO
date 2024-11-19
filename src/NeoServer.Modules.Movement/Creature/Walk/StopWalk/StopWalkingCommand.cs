using Mediator;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Modules.Movement.Creature.StopWalk;

namespace NeoServer.Modules.Movement.Creature.Walk.StopWalk;

public record StopWalkingCommand(ICreature Creature): ICommand;

public class StopWalkingCommandHandler(StopWalkingPacketDispatcher stopWalkingPacketDispatcher) : ICommandHandler<StopWalkingCommand>
{
    public ValueTask<Unit> Handle(StopWalkingCommand command, CancellationToken cancellationToken)
    {
        var creature = command.Creature as IWalkableCreature;
        Guard.ThrowIfAnyNull(creature);

        if (!creature.HasNextStep) return Unit.ValueTask;
        
        creature.StopWalking();
        stopWalkingPacketDispatcher.Send(creature);

        return Unit.ValueTask;
    }
}