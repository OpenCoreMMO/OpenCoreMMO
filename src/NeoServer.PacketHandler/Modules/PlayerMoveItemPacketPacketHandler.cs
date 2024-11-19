using Mediator;
using NeoServer.BuildingBlocks.Application.Contracts;
using NeoServer.BuildingBlocks.Domain;
using NeoServer.BuildingBlocks.Infrastructure.Threading.Event;
using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Location;
using NeoServer.Modules.Movement.Item.FromContainerToContainer;
using NeoServer.Modules.Movement.Item.FromContainerToInventory;
using NeoServer.Modules.Movement.Item.FromInventoryToContainer;
using NeoServer.Modules.Movement.Item.FromMapToBackpack;
using NeoServer.Modules.Movement.Item.FromMapToContainer;
using NeoServer.Modules.Movement.Item.ToMap;
using NeoServer.Modules.Players.DressEquipment;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Networking.Packets.Network;

namespace NeoServer.PacketHandler.Modules;

public class PlayerMoveItemPacketPacketHandler : PacketHandler
{
    private readonly IGameServer _game;
    private readonly ItemFinder _itemFinder;
    private readonly IMediator _mediator;

    public PlayerMoveItemPacketPacketHandler(IGameServer game,
        IMediator mediator,
        ItemFinder itemFinder)
    {
        _game = game;
        _mediator = mediator;
        _itemFinder = itemFinder;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var itemThrowPacket = new ItemThrowPacket(message);
        if (!_game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        var item = _itemFinder.Find(player, itemThrowPacket.FromLocation, itemThrowPacket.ItemClientId);
        var fromPlace = item.Parent ?? _game.Map[item.Location];
        var amount = itemThrowPacket.Count;

        var fromLocation = itemThrowPacket.FromLocation;
        var toLocation = itemThrowPacket.ToLocation;

        ICommand command = GetOperation(itemThrowPacket) switch
        {
            ItemMovementOperation.ToEquipmentSlot => new DressEquipmentCommand(player, item, fromPlace as IHasItem,
                itemThrowPacket.FromStackPosition, amount, toLocation.Slot),
            ItemMovementOperation.ToMap => new MoveItemToMapCommand(player, fromLocation, toLocation, amount),
            ItemMovementOperation.FromContainerToContainer => new MoveFromContainerToContainerCommand(player,
                fromLocation, toLocation, amount),
            ItemMovementOperation.FromInventoryToContainer => new MoveFromInventoryToContainerCommand(player,
                fromLocation, toLocation, amount),
            ItemMovementOperation.FromContainerToInventory => new MoveFromContainerToInventoryCommand(player,
                fromLocation, toLocation, amount),
            ItemMovementOperation.FromInventoryToInventory => new MoveFromInventoryToContainerCommand(player,
                fromLocation, toLocation, amount),
            ItemMovementOperation.FromMapToBackpackSlot => new MoveFromMapToBackpackSlotCommand(player, fromLocation,
                toLocation, amount),
            ItemMovementOperation.FromMapToContainer => new MoveFromMapToContainerCommand(player, fromLocation,
                toLocation, amount),
            _ => null
        };

        ArgumentNullException.ThrowIfNull(command);

        _game.Dispatcher.AddEvent(new Event(2000,
            () => _ = _mediator.Send(command))); //todo create a const for 2000 expiration time
    }

    private static ItemMovementOperation GetOperation(ItemThrowPacket itemThrowPacket)
    {
        var fromLocation = itemThrowPacket.FromLocation;
        var toLocation = itemThrowPacket.ToLocation;

        if (toLocation.Type is LocationType.Slot && toLocation.Slot is not Slot.Backpack)
            return ItemMovementOperation.ToEquipmentSlot;

        if (toLocation.Type is LocationType.Ground) return ItemMovementOperation.ToMap;

        if (fromLocation.Type is LocationType.Container && toLocation.Type is LocationType.Container)
            return ItemMovementOperation.FromContainerToContainer;

        if (fromLocation.Type is LocationType.Slot && toLocation.Type is LocationType.Container)
            return ItemMovementOperation.FromInventoryToContainer;

        if (fromLocation.Type is LocationType.Container && toLocation.Type is LocationType.Slot)
            return ItemMovementOperation.FromContainerToInventory;

        if (fromLocation.Type is LocationType.Slot && toLocation.Type is LocationType.Slot)
            return ItemMovementOperation.FromInventoryToInventory;

        if (fromLocation.Type is LocationType.Ground && toLocation.Slot is Slot.Backpack)
            return ItemMovementOperation.FromMapToBackpackSlot;

        if (fromLocation.Type is LocationType.Ground && toLocation.Type is LocationType.Container)
            return ItemMovementOperation.FromMapToContainer;

        return ItemMovementOperation.None;
    }

    private enum ItemMovementOperation
    {
        ToEquipmentSlot,
        ToMap,
        FromContainerToContainer,
        FromInventoryToContainer,
        FromContainerToInventory,
        FromInventoryToInventory,
        FromMapToBackpackSlot,
        FromMapToContainer,
        None
    }
}