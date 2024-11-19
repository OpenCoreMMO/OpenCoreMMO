using Mediator;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items.Types.Containers;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Modules.Movement.Item.FromInventoryToContainer;

public sealed record MoveFromInventoryToContainerCommand(
    IPlayer Player,
    Location FromLocation,
    Location ToLocation,
    byte Amount) : ICommand;

public class MoveFromInventoryToContainerCommandHandler : ICommandHandler<MoveFromInventoryToContainerCommand>
{
    public ValueTask<Unit> Handle(MoveFromInventoryToContainerCommand command, CancellationToken cancellationToken)
    {
        var container = command.ToLocation.Type is LocationType.Slot
            ? command.Player.Inventory[command.ToLocation.Slot] as IContainer
            : command.Player.Containers[command.ToLocation.ContainerId];

        if (container is null) return Unit.ValueTask;

        var item = command.Player.Inventory[command.FromLocation.Slot];

        if (item is null) return Unit.ValueTask;
        if (!item.IsPickupable) return Unit.ValueTask;

        command.Player.MoveItem(item, command.Player.Inventory, container, command.Amount,
            (byte)command.FromLocation.Slot,
            (byte)command.ToLocation.ContainerSlot);

        return Unit.ValueTask;
    }
}