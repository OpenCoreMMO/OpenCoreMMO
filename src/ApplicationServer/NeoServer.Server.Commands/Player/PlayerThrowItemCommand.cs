using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Commands.Movements;
using NeoServer.Server.Commands.Movements.ToContainer;
using NeoServer.Server.Commands.Movements.ToInventory;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Commands;

namespace NeoServer.Server.Commands.Player;

public class PlayerThrowItemCommand : ICommand
{
    private readonly MapToContainerMovementOperation _mapToContainerMovementOperation;
    private readonly MapToInventoryMovementOperation _mapToInventoryMovementOperation;
    private readonly IGameServer game;
    private readonly IToMapMovementService toMapMovementService;

    public PlayerThrowItemCommand(IGameServer game, IToMapMovementService toMapMovementService,
        MapToContainerMovementOperation mapToContainerMovementOperation,
        MapToInventoryMovementOperation mapToInventoryMovementOperation
    )
    {
        this.game = game;
        this.toMapMovementService = toMapMovementService;
        _mapToContainerMovementOperation = mapToContainerMovementOperation;
        _mapToInventoryMovementOperation = mapToInventoryMovementOperation;
    }

    public void Execute(IPlayer player, ItemThrowPacket itemThrow)
    {
        if (ContainerToContainerMovementOperation.IsApplicable(itemThrow))
            ContainerToContainerMovementOperation.Execute(player, itemThrow);
        else if (MapToInventoryMovementOperation.IsApplicable(itemThrow))
            _mapToInventoryMovementOperation.Execute(player, game.Map, itemThrow);
        else if (ToMapMovementOperation.IsApplicable(itemThrow))
            ToMapMovementOperation.Execute(player, itemThrow, toMapMovementService);
        else if (InventoryToContainerMovementOperation.IsApplicable(itemThrow))
            InventoryToContainerMovementOperation.Execute(player, itemThrow);
        else if (ContainerToInventoryMovementOperation.IsApplicable(itemThrow))
            ContainerToInventoryMovementOperation.Execute(player, itemThrow);
        else if (MapToContainerMovementOperation.IsApplicable(itemThrow))
            _mapToContainerMovementOperation.Execute(player, game, game.Map, itemThrow);
        else if (InventoryToInventoryOperation.IsApplicable(itemThrow))
            InventoryToInventoryOperation.Execute(player, itemThrow);
    }
}