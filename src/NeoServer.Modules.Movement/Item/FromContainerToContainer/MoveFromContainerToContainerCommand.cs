using Mediator;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Services;

namespace NeoServer.Modules.Movement.Item.FromContainerToContainer;

public sealed record MoveFromContainerToContainerCommand(
    IPlayer Player,
    Location FromLocation,
    Location ToLocation,
    byte Amount) : ICommand;

public class MoveFromContainerToContainerCommandHandler : ICommandHandler<MoveFromContainerToContainerCommand>
{
    public ValueTask<Unit> Handle(MoveFromContainerToContainerCommand command, CancellationToken cancellationToken)
    {
        command.Deconstruct(out var player, out var fromLocation, out var toLocation, out var amount);

        Guard.ThrowIfAnyNull(player);

        if (fromLocation.Type is not LocationType.Container ||
            toLocation.Type is not LocationType.Container)
        {
            OperationFailService.Send(player, InvalidOperation.NotPossible);
            return Unit.ValueTask;
        }

        var fromContainerId = fromLocation.ContainerId;
        var toContainerId = toLocation.ContainerId;
        var itemIndex = fromLocation.ContainerSlot;

        var sameIds = fromContainerId == toContainerId;
        var sameContainers = player.Containers[fromContainerId][itemIndex] == player.Containers[toContainerId];

        var tryingMoveContainerToItself = !sameIds && sameContainers;

        if (tryingMoveContainerToItself)
            //this is impossible error
            return Unit.ValueTask;

        player.Containers.MoveItemBetweenContainers(fromLocation, toLocation,
            amount);

        return Unit.ValueTask;
    }
}