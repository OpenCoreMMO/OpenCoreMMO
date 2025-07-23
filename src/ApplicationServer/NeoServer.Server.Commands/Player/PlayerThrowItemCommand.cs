using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Commands.Movements;
using NeoServer.Server.Commands.Movements.ToContainer;
using NeoServer.Server.Commands.Movements.ToInventory;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Commands;
using NeoServer.Server.Common.Contracts.Scripts;

namespace NeoServer.Server.Commands.Player;

public class PlayerThrowItemCommand(
    IGameServer game,
    IToMapMovementService toMapMovementService,
    MapToContainerMovementOperation mapToContainerMovementOperation,
    MapToInventoryMovementOperation mapToInventoryMovementOperation,
    IScriptManager scriptManager
    ) : ICommand
{
    public void Execute(IPlayer player, ItemThrowPacket itemThrow)
    {
        if (ContainerToContainerMovementOperation.IsApplicable(itemThrow))
            ContainerToContainerMovementOperation.Execute(player, itemThrow);
        else if (MapToInventoryMovementOperation.IsApplicable(itemThrow))
            mapToInventoryMovementOperation.Execute(player, game.Map, itemThrow, scriptManager);
        else if (ToMapMovementOperation.IsApplicable(itemThrow))
            ToMapMovementOperation.Execute(player, itemThrow, toMapMovementService);
        else if (InventoryToContainerMovementOperation.IsApplicable(itemThrow))
            InventoryToContainerMovementOperation.Execute(player, itemThrow, scriptManager);
        else if (ContainerToInventoryMovementOperation.IsApplicable(itemThrow))
            ContainerToInventoryMovementOperation.Execute(player, itemThrow, scriptManager);
        else if (MapToContainerMovementOperation.IsApplicable(itemThrow))
            mapToContainerMovementOperation.Execute(player, game, game.Map, itemThrow);
        else if (InventoryToInventoryOperation.IsApplicable(itemThrow))
            InventoryToInventoryOperation.Execute(player, itemThrow);
    }
}