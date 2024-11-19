using Mediator;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Modules.Movement.Item.FromContainerToInventory;

public sealed record MoveFromContainerToInventoryCommand(
    IPlayer Player,
    Location FromLocation,
    Location ToLocation,
    byte Amount) : ICommand;

public class MoveFromContainerToInventoryCommandHandler : ICommandHandler<MoveFromContainerToInventoryCommand>
{
    public ValueTask<Unit> Handle(MoveFromContainerToInventoryCommand command, CancellationToken cancellationToken)
    {
        var container = command.Player.Containers[command.FromLocation.ContainerId];

        var item = container[command.FromLocation.ContainerSlot];

        if (item is null) return Unit.ValueTask;

        if (!item.IsPickupable) return Unit.ValueTask;

        command.Player.MoveItem(item, container, command.Player.Inventory, command.Amount,
            (byte)command.FromLocation.ContainerSlot, (byte)command.ToLocation.Slot);
        return Unit.ValueTask;
    }
}