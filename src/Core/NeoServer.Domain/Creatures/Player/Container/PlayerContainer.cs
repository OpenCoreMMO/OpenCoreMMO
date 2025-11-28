using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;

namespace NeoServer.Domain.Creatures.Player.Container;

internal class PlayerContainer : IEquatable<PlayerContainer>
{
    private bool eventsAttached;

    public PlayerContainer(IContainer container, IPlayer player)
    {
        Container = container;
        Player = player;
    }

    public IPlayer Player { get; }

    public byte Id { get; set; }

    public IContainer Container { get; }

    public RemoveItemFromOpenedContainer RemoveItem { get; private set; }
    public AddItemOnOpenedContainer AddItem { get; private set; }
    public UpdateItemOnOpenedContainer UpdateItem { get; private set; }
    public MoveOpenedContainer MoveOpenedContainer { get; private set; }

    public bool Equals(PlayerContainer obj)
    {
        return Container == obj.Container;
    }

    public void ItemAdded(IItem item, IContainer container)
    {
        AddItem?.Invoke(Player, Id, item);
    }

    public void ItemRemovedEvent(IContainer fromContainer, byte slotIndex, IItem item, byte amountReduced)
    {
        RemoveItem?.Invoke(Player, Id, slotIndex, item);
    }

    public void ItemUpdated(IContainer onContainer, byte slotIndex, IItem item, sbyte amount)
    {
        UpdateItem?.Invoke(Player, Id, slotIndex, item, amount);
    }

    public void ContainerMoved(IContainer container)
    {
        MoveOpenedContainer?.Invoke(Id, container);
    }

    public void AttachActions(RemoveItemFromOpenedContainer removeItemAction,
        AddItemOnOpenedContainer addItemAction, UpdateItemOnOpenedContainer updateItemAction,
        MoveOpenedContainer moveOpenedContainer)
    {
        RemoveItem ??= removeItemAction;
        AddItem ??= addItemAction;
        UpdateItem ??= updateItemAction;
        MoveOpenedContainer ??= moveOpenedContainer;
    }

    public void AttachContainerEvent()
    {
        if (eventsAttached) return;

        Container.OnItemAdded += ItemAdded;
        Container.OnItemRemovedEvent += ItemRemovedEvent;
        Container.OnItemUpdated += ItemUpdated;
        Container.OnContainerMoved += ContainerMoved;

        eventsAttached = true;
    }

    internal void DetachContainerEvents()
    {
        Container.OnItemRemovedEvent -= ItemRemovedEvent;
        Container.OnItemAdded -= ItemAdded;
        Container.OnItemUpdated -= ItemUpdated;
        Container.OnContainerMoved -= ContainerMoved;
    }
}