using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Services;
using NeoServer.Domain.Common.Texts;
using NeoServer.Domain.Creatures.Player.Inventory;

namespace NeoServer.Domain.Creatures.Player.Container;

public class PlayerContainerList : IPlayerContainerList
{
    private readonly Dictionary<byte, PlayerContainer> openedContainers = new();
    private readonly IPlayer player;

    public PlayerContainerList(IPlayer player)
    {
        this.player = player;
        MoveOpenedContainer += CloseDistantContainer;
    }

    public MoveOpenedContainer MoveOpenedContainer { get; }
    public event ClosedContainer OnClosedContainer;
    public event ClosedDepot OnClosedDepot;
    public event OpenedContainer OnOpenedContainer;
    public RemoveItemFromOpenedContainer RemoveItemAction { get; set; }
    public AddItemOnOpenedContainer AddItemAction { get; set; }
    public UpdateItemOnOpenedContainer UpdateItemAction { get; set; }

    public bool HasAnyDepotOpened
    {
        get
        {
            foreach (var container in openedContainers.Values)
                if (container.Container.RootParent is Depot.Depot)
                    return true;
            return false;
        }
    }

    public bool IsOpened(byte containerId)
    {
        return openedContainers.ContainsKey(containerId);
    }

    public void CloseDistantContainers()
    {
        if (openedContainers.Count == 0) return;
        foreach (var container in openedContainers.Values) CloseDistantContainer(container.Id, container.Container);
    }

    public void CloseAll()
    {
        if (openedContainers.Count == 0) return;
        foreach (var container in openedContainers.Values) CloseContainer(container.Id);
    }

    public IContainer this[byte id] => openedContainers.ContainsKey(id) ? openedContainers[id]?.Container : null;

    public void GoBackContainer(byte containerId)
    {
        if (openedContainers.TryGetValue(containerId, out var playerContainer))
        {
            var parentContainer = playerContainer.Container.Parent;

            if (parentContainer is not IContainer)
            {
                CloseContainer(containerId);
                return;
            }

            InsertOrOverrideOpenedContainer(containerId,
                new PlayerContainer(parentContainer as IContainer, player));

            OnOpenedContainer?.Invoke(player, containerId, parentContainer as IContainer);
        }
    }

    public void OpenContainerAt(IContainer containerToOpen, byte containerLevel)
    {
        PlayerContainer playerContainer = null;
        var location = containerToOpen.Location;

        if (containerToOpen is Depot.Depot depot && !depot.CanBeOpenedBy(player))
        {
            OperationFailService.Send(player.CreatureId, TextConstants.DEPOT_ALREADY_OPENED);
            return;
        }

        if (location.Type == LocationType.Ground)
        {
            if (!player.Location.IsNextTo(containerToOpen.Location))
            {
                player.WalkTo(containerToOpen.Location,
                    _ => OpenContainerAt(containerToOpen, containerLevel));
                return;
            }

            if (containerToOpen is ILootContainer lootContainer && !lootContainer.CanBeOpenedBy(player))
            {
                OperationFailService.Send(player.CreatureId, TextConstants.YOU_ARE_NOT_THE_OWNER);
                return;
            }

            playerContainer = new PlayerContainer(containerToOpen, player);
        }
        else if (location.Slot == Slot.Backpack)
        {
            playerContainer = new PlayerContainer(player.Inventory.BackpackSlot, player);
        }
        else if (location.Type == LocationType.Container)
        {
            var parentContainer = openedContainers[location.ContainerId]?.Container;
            parentContainer.GetContainerAt((byte)location.ContainerSlot, out var container);
            if (container is null) return;
            playerContainer = new PlayerContainer(container, player);
        }

        var containerAlreadyOpened = openedContainers.Values.FirstOrDefault(v => v.Equals(playerContainer))?.Id;
        if (containerAlreadyOpened.HasValue) //if container is already opened
        {
            CloseContainer(containerAlreadyOpened.Value); //just close container and return
            return;
        }

        InsertOrOverrideOpenedContainer(containerLevel, playerContainer);

        if (containerToOpen is Depot.Depot toOpen) toOpen.SetAsOpened(player);

        OnOpenedContainer?.Invoke(player, playerContainer.Id, playerContainer.Container);
        playerContainer.Container.UpdateId(playerContainer.Id);
    }

    public void MoveItemBetweenContainers(Location fromLocation, Location toLocation, byte amount = 1)
    {
        var fromContainer = openedContainers[fromLocation.ContainerId];
        var toContainer = openedContainers[toLocation.ContainerId];

        var item = fromContainer.Container[fromLocation.ContainerSlot];

        if (item == toContainer.Container) return;

        player.MoveItem(item, fromContainer.Container, toContainer.Container, amount,
            (byte)fromLocation.ContainerSlot, (byte)toLocation.ContainerSlot);
    }

    public void CloseContainer(byte containerId)
    {
        if (!openedContainers.Remove(containerId, out var playerContainer)) return;

        playerContainer.DetachContainerEvents();
        OnClosedContainer?.Invoke(player, containerId, playerContainer.Container);

        if (playerContainer.Container is Depot.Depot depot)
            //call depot event if container is a depot
            OnClosedDepot?.Invoke(player, containerId, depot);

        playerContainer.Container.ClosedBy(player);

        //check if container is within a depot
        if (playerContainer.Container.RootParent is Depot.Depot rootDepot && playerContainer.Container != rootDepot)
        {
            //if so emit event and call ClosedBy method
            OnClosedDepot?.Invoke(player, containerId, rootDepot);
            rootDepot.ClosedBy(player);
        }

        playerContainer.Container.RemoveId();
    }

    private void CloseDistantContainer(byte containerId, IContainer container)
    {
        if (openedContainers.Count == 0) return;

        var containerLocation = container.RootParent?.Location;

        if (containerLocation is null) return;

        if (containerLocation.Value.Type == LocationType.Ground &&
            containerLocation.Value.IsNextTo(player.Location) is false)
        {
            CloseContainer(containerId);
            return;
        }

        if (container.RootParent is IPlayer playerOwner && playerOwner != player) CloseContainer(containerId);
    }

    private void InsertOrOverrideOpenedContainer(byte containerLevel, PlayerContainer playerContainer)
    {
        playerContainer.Id = containerLevel;

        if (
            openedContainers
            .ContainsKey(
                containerLevel)) //when opening container in the same level. Just override the opened container
        {
            openedContainers[containerLevel]
                .DetachContainerEvents(); //detach all container events from the old container
            openedContainers[containerLevel] = playerContainer; // set the new container
        }
        else
        {
            openedContainers.TryAdd(playerContainer.Id, playerContainer);
        }

        playerContainer.AttachActions(RemoveItemAction, AddItemAction, UpdateItemAction, MoveOpenedContainer);
        playerContainer.AttachContainerEvent();
    }
}