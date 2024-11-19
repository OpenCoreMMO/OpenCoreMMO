using Mediator;
using NeoServer.BuildingBlocks.Application.IntegrationEvents.Combat;
using NeoServer.BuildingBlocks.Infrastructure;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Modules.Combat.PlayerAttack;

public record SetAttackTargetCommand(ICombatActor Aggressor, ICreature Target) : ICommand;

public class SetAttackTargetCommandHandler(IEventBus eventBus) : ICommandHandler<SetAttackTargetCommand>
{
    public ValueTask<Unit> Handle(SetAttackTargetCommand command, CancellationToken cancellationToken)
    {
        var result = command.Aggressor.SetAttackTarget(command.Target);
        
        if (result.Succeeded)
        {
            eventBus.Publish(new CreatureSetAttackTargetEvent(command.Aggressor, command.Target));
        }

        return Unit.ValueTask;
    }
}