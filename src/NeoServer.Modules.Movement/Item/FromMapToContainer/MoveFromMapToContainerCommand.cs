using Mediator;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Modules.Movement.Item.FromMapToContainer;

public sealed record MoveFromMapToContainerCommand(
    IPlayer Player,
    Location FromLocation,
    Location ToLocation,
    byte Amount) : ICommand;

public class MoveFromMapToContainerCommandHandler : ICommandHandler<MoveFromMapToContainerCommand>
{
    private readonly IItemMovementService _itemMovementService;
    private readonly IMap _map;

    public MoveFromMapToContainerCommandHandler(IMap map, IItemMovementService itemMovementService)
    {
        _map = map;
        _itemMovementService = itemMovementService;
    }

    public ValueTask<Unit> Handle(MoveFromMapToContainerCommand command, CancellationToken cancellationToken)
    {
        var tile = _map[command.FromLocation];

        if (tile is not IDynamicTile fromTile) return Unit.ValueTask;
        var item = fromTile.TopItemOnStack;

        if (item is null) return Unit.ValueTask;
        if (!item.IsPickupable) return Unit.ValueTask;

        var container = command.Player.Containers[command.ToLocation.ContainerId];
        if (container is null) return Unit.ValueTask;

        _itemMovementService.Move(command.Player, item, fromTile, container,
            command.Amount, 0, (byte)command.ToLocation.ContainerSlot);

        return Unit.ValueTask;
    }
}