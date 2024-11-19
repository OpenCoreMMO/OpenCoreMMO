using Mediator;
using NeoServer.BuildingBlocks.Application.IntegrationEvents.Players;
using NeoServer.BuildingBlocks.Infrastructure;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures.Players;

namespace NeoServer.Modules.Players.ChangeMode;

public record ChangeModeCommand(IPlayer Player, FightMode FightMode, ChaseMode ChaseMode, byte SecureMode): ICommand;

public class ChangeModeCommandHandler(IEventBus eventBus) : ICommandHandler<ChangeModeCommand>
{
    public ValueTask<Unit> Handle(ChangeModeCommand command, CancellationToken cancellationToken)
    {
        var (player, fightMode, chaseMode, secureMode) = command;
        
        player.ChangeFightMode(fightMode);
        player.ChangeSecureMode(secureMode);

        if (player.ChaseMode != chaseMode)
        {
            player.ChangeChaseMode(chaseMode);
            eventBus.Publish(new PlayerChangedChaseModeIntegrationEvent(player));
        }

        return Unit.ValueTask;
    }
}