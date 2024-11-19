using Mediator;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.World.Models.Tiles;

namespace NeoServer.Modules.Movement.Item.FromMapToBackpack;

public sealed record MoveFromMapToBackpackSlotCommand(
    IPlayer Player,
    Location FromLocation,
    Location ToLocation,
    byte Amount) : ICommand;

public class MoveFromMapToBackpackSlotCommandHandler : ICommandHandler<MoveFromMapToBackpackSlotCommand>
{
    private readonly IItemMovementService _itemMovementService;
    private readonly IMap _map;

    public MoveFromMapToBackpackSlotCommandHandler(IMap map, IItemMovementService itemMovementService)
    {
        _map = map;
        _itemMovementService = itemMovementService;
    }

    public ValueTask<Unit> Handle(MoveFromMapToBackpackSlotCommand command, CancellationToken cancellationToken)
    {
        if (command.FromLocation.Type != LocationType.Ground) return Unit.ValueTask;
        if (command.ToLocation.Slot is not Slot.Backpack) return Unit.ValueTask;

        if (_map[command.FromLocation] is not DynamicTile fromTile) return Unit.ValueTask;

        if (fromTile.TopItemOnStack is not { } item) return Unit.ValueTask;

        _itemMovementService.Move(command.Player, item, fromTile, command.Player.Inventory, command.Amount, 0,
            (byte)Slot.Backpack);

        return Unit.ValueTask;
    }
}