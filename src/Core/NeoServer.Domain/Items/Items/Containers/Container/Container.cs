using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Items.Bases;
using NeoServer.Domain.Items.Items.Containers.Container.Builders;
using NeoServer.Domain.Items.Items.Containers.Container.Calculations;
using NeoServer.Domain.Items.Items.Containers.Container.Operations;
using NeoServer.Domain.Items.Items.Containers.Container.Operations.Add;
using NeoServer.Domain.Items.Items.Containers.Container.Operations.Remove;
using NeoServer.Domain.Items.Items.Containers.Container.Operations.Update;
using NeoServer.Domain.Items.Items.Containers.Container.Queries;
using NeoServer.Domain.Items.Items.Containers.Container.Rules;

namespace NeoServer.Domain.Items.Items.Containers.Container;

public class Container : BaseItem, IContainer
{
    public Container(IItemType type, Location location, IEnumerable<IItem> children = null) : base(
        type, location)
    {
        _containerWeight = new ContainerWeight(this);

        Items = new List<IItem>(Capacity);

        SubscribeToEvents();

        AddItemOperation.AddChildren(this, children);
    }

    public byte? Id { get; private set; }
    public byte LastFreeSlot => IsFull ? (byte)0 : SlotsUsed;
    public uint FreeSlotsCount => (uint)(Capacity - SlotsUsed);
    public new static Func<IItem, IPlayer, byte, bool> UseFunction { get; set; }
    public byte SlotsUsed { get; internal set; }
    public uint TotalOfFreeSlots => ContainerSlotsCalculation.CalculateFreeSlots(this);
    public bool IsFull => SlotsUsed >= Capacity;
    public IThing Parent { get; internal set; }
    public bool HasParent => Parent != null;
    public byte Capacity => Metadata.Attributes.GetAttribute<byte>(ItemTypeAttribute.Capacity);
    public List<IItem> Items { get; }
    public IItem this[int index] => Items.Count > index ? Items[index] : null;
    public bool HasItems => SlotsUsed > 0;

    /// <summary>
    ///     Gets all items recursively from this container, including the ones inside inner containers.
    /// </summary>
    public List<IItem> RecursiveItems => GetRecursiveItemsQuery.Get(this);

    public IThing RootParent => FindRootParentQuery.Find(this);

    public virtual void ClosedBy(IPlayer player)
    {
    }

    public IDictionary<ushort, uint> Map => ContainerMapBuilder.Build(this);

    public void SetParent(IThing parent)
    {
        SetContainerParentOperation.SetParent(this, parent);
    }

    public void UpdateId(byte id)
    {
        Id = id;
        ItemsLocationOperation.Update(this, id);
    }

    public void RemoveId()
    {
        Id = null;
    }

    public void Clear()
    {
        ContainerClearOperation.Clear(this);
    }

    public uint PossibleAmountToAdd(IItem item, byte? toPosition = null)
    {
        return PossibleAmountToAddCalculation.Calculate(this, item);
    }

    public void OnMoved(IThing to)
    {
        OnContainerMoved?.Invoke(this);
    }

    public virtual void Use(IPlayer usedBy, byte openAtIndex)
    {
        if (UseFunction?.Invoke(this, usedBy, openAtIndex) is true)
            return;

        usedBy.Containers.OpenContainerAt(this, openAtIndex);
    }

    private void SubscribeToEvents()
    {
        OnItemAdded += OnItemAddedToContainer;
    }

    private void OnItemAddedToContainer(IItem item, IContainer container)
    {
        if (item.CanBeMoved) item.SetOwner(RootParent);
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Group == ItemGroup.Container;
    }

    internal void OnItemReduced(ICumulative item, byte amount)
    {
        if (item.Amount == 0)
        {
            RemoveBySlotIndexOperation.Remove(this, (byte)item.Location.ContainerSlot, amount);
            return;
        }

        OnItemUpdated?.Invoke(this, (byte)item.Location.ContainerSlot, item, (sbyte)-amount);
    }

    internal void DetachEvents(IContainer container)
    {
        container.OnItemUpdated -= OnItemUpdated;
        container.OnItemAdded -= OnItemAdded;
        container.OnItemRemoved -= OnItemRemoved;
    }

    public override string ToString()
    {
        return StringContentBuilder.Build(this);
    }

    #region Weight

    private readonly ContainerWeight _containerWeight;
    public override float Weight => _containerWeight.Weight;

    internal void OnChildWeightUpdated(float change)
    {
        _containerWeight.UpdateWeight(this, change);
    }

    public void SubscribeToWeightChangeEvent(WeightChange weightChange)
    {
        _containerWeight.SubscribeToWeightChangeEvent(weightChange);
    }

    public void UnsubscribeFromWeightChangeEvent(WeightChange weightChange)
    {
        _containerWeight.UnsubscribeFromWeightChangeEvent(weightChange);
    }

    #endregion

    #region Queries

    public (IItem ItemFound, IContainer Container, byte SlotIndex) GetFirstItem(ushort clientId)
    {
        return FindFirstItemByClientIdQuery.Find(this, clientId);
    }

    public bool GetContainerAt(byte index, out IContainer container)
    {
        return FindContainerAtIndexQuery.Find(this, index, out container);
    }

    #endregion

    #region Remove item

    public void RemoveItem(IItemType itemToRemove, byte amount)
    {
        RemoveByItemTypeOperation.Remove(this, itemToRemove, amount);
    }

    public void RemoveItem(IItem item, byte amount)
    {
        RemoveByItemOperation.Remove(this, item, amount);
    }

    public Result<OperationResultList<IItem>> RemoveItem(byte fromPosition, byte amount, out IItem removedThing)
    {
        amount = amount == 0 ? (byte)1 : amount;
        removedThing = RemoveBySlotIndexOperation.Remove(this, fromPosition, amount);
        return new Result<OperationResultList<IItem>>(new OperationResultList<IItem>(Operation.Removed, removedThing,
            fromPosition));
    }

    #endregion

    #region Rules

    public Result<uint> CanAddItem(IItemType itemType)
    {
        return CanAddItemToContainerRule.CanAdd(this, itemType);
    }

    public Result CanAddItem(IItem item, byte amount = 1, byte? slot = null)
    {
        return CanAddItemToContainerRule.CanAdd(this, item, slot);
    }

    public bool CanBeDressed(IPlayer player)
    {
        return true;
    }

    #endregion

    #region Add item

    public Result<OperationResultList<IItem>> AddItem(IItem item, bool includeChildren)
    {
        if (item is null) return Result<OperationResultList<IItem>>.NotPossible;

        Result<OperationResultList<IItem>> result = new(AddItemOperation.TryAddItem(this, item).Reason);
        if (result.Succeeded) return result;

        if (!includeChildren) return result;

        foreach (var currentItem in Items)
        {
            if (currentItem is not IContainer container) continue;
            result = container.AddItem(item, true);

            if (result.Succeeded) return result;
        }

        return result;
    }

    public Result<OperationResultList<IItem>> AddItem(IItem item, byte? position = null)
    {
        if (item is null) return Result<OperationResultList<IItem>>.NotPossible;

        return new Result<OperationResultList<IItem>>(AddItemOperation.TryAddItem(this, item, position).Reason);
    }

    public bool UpdateItem(IItem item, IItemType newType)
    {
        var result = ReplaceItemOperation.Replace(this, item, newType);
        if (!result) return false;

        InvokeItemUpdatedEvent((byte)item.Location.ContainerSlot, (sbyte)item.Amount);
        return true;
    }

    #endregion

    #region Events

    public event RemoveItem OnItemRemoved;
    public event AddItem OnItemAdded;
    public event UpdateItem OnItemUpdated;
    public event Move OnContainerMoved;

    internal void InvokeItemUpdatedEvent(byte slotIndex, sbyte amount)
    {
        OnItemUpdated?.Invoke(this, slotIndex, Items[slotIndex], amount);
    }

    internal void InvokeItemRemovedEvent(byte slotIndex, IItem removedItem, byte amount)
    {
        OnItemRemoved?.Invoke(this, slotIndex, removedItem, amount);
    }

    internal void InvokeItemAddedEvent(IItem removedItem, IContainer container)
    {
        OnItemAdded?.Invoke(removedItem, container);
    }

    #endregion
}

public static class ReplaceItemOperation
{
    public static bool Replace(Container container, IItem fromItem, IItemType toItemType)
    {
        if (toItemType is null) return false;
        if (fromItem.Metadata.Group != toItemType.Group) return false;

        fromItem.UpdateMetadata(toItemType);
        return true;
    }
}