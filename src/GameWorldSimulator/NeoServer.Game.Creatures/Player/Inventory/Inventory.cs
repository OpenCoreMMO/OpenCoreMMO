using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Contracts.Items.Types.Body;
using NeoServer.Game.Common.Contracts.Items.Types.Containers;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Results;
using NeoServer.Game.Creatures.Player.Inventory.Calculations;
using NeoServer.Game.Creatures.Player.Inventory.Operations;
using NeoServer.Game.Creatures.Player.Inventory.Rules;

namespace NeoServer.Game.Creatures.Player.Inventory;

public class Inventory : IInventory
{
    private float _totalWeight;

    public Inventory(IPlayer player, IDictionary<Slot, (IItem Item, ushort Id)> items)
    {
        InventoryMap = new InventoryMap(this);
        Owner = player;

        OnItemAddedToSlot += OnItemAddedToInventorySlot;

        AddItemsToInventory(items);
    }

    internal InventoryMap InventoryMap { get; }

    internal IAmmoEquipment Ammo => InventoryMap.GetItem<IAmmoEquipment>(Slot.Ammo);
    internal IDefenseEquipment Shield => InventoryMap.GetItem<IDefenseEquipment>(Slot.Right);
    public IWeapon Weapon => InventoryMap.GetItem<IWeapon>(Slot.Left);
    public bool IsUsingWeapon => InventoryMap.HasItemOnSlot(Slot.Left);
    public bool HasShield => InventoryMap.HasItemOnSlot(Slot.Right);
    public ushort TotalAttack => this.CalculateTotalAttack();
    public ushort TotalDefense => InventoryMap.CalculateTotalDefense();
    public ushort TotalArmor => InventoryMap.CalculateTotalArmor();
    public byte AttackRange => InventoryMap.CalculateAttackRange();

    public ulong GetTotalMoney(ICoinTypeStore coinTypeStore)
    {
        return this.CalculateTotalMoney(coinTypeStore);
    }

    /// <summary>
    ///     Gets all items that player is wearing except the bag
    /// </summary>
    public IEnumerable<IItem> DressingItems =>
        new List<IItem>
        {
            this[Slot.Head], this[Slot.Necklace], this[Slot.Body], this[Slot.Right], this[Slot.Left], this[Slot.Legs],
            this[Slot.Ring], this[Slot.Ammo], this[Slot.Feet]
        };

    public IDictionary<ushort, uint> Map => InventoryMap.Map;
    public IPlayer Owner { get; }
    public IItem this[Slot slot] => InventoryMap.GetItem<IItem>(slot);

    public T TryGetItem<T>(Slot slot)
    {
        return this[slot] is T item ? item : default;
    }

    public IContainer BackpackSlot => this[Slot.Backpack] as IContainer;

    public float TotalWeight
    {
        get => _totalWeight;
        internal set
        {
            _totalWeight = value;
            OnWeightChanged?.Invoke(this);
        }
    }

    public Result CanAddItem(IItem thing, byte amount = 1, byte? slot = null)
    {
        return this.CanAddItem(slot is null ? Slot.None : (Slot)slot, thing, amount);
    }

    public Result<uint> CanAddItem(IItemType itemType)
    {
        return AddToSlotRule.CanAddItem(this, itemType);
    }

    public uint PossibleAmountToAdd(IItem item, byte? toPosition = null)
    {
        return PossibleAmountToAddCalculation.Calculate(this, item, toPosition);
    }

    public bool CanRemoveItem(IItem item)
    {
        return true;
    }

    private void AddItemsToInventory(IDictionary<Slot, (IItem Item, ushort Id)> items)
    {
        foreach (var (slot, (item, _)) in items) TryAddItemToSlot(slot, item);
    }

    #region Operations

    private Result<IItem> TryAddItemToSlot(Slot slot, IItem item)
    {
        var result = AddToSlotOperation.Add(this, slot, item);

        if (result.Succeeded)
        {
            TotalWeight += item.Weight;
            OnItemAddedToSlot?.Invoke(this, item, slot);
            return result;
        }

        OnFailedToAddToSlot?.Invoke(Owner, result.Error);
        return result;
    }

    public Result<OperationResultList<IItem>> AddItem(IItem item, Slot slot = Slot.None)
    {
        return AddItem(item, slot is Slot.None ? null : (byte)slot);
    }

    public bool UpdateItem(IItem item, IItemType newType)
    {
        var result = ReplaceItemOperation.Replace(this, item, newType);
        if (!result) return false;

        OnItemAddedToSlot?.Invoke(this, item, item.Metadata.BodyPosition);
        return true;
    }

    public Result<OperationResultList<IItem>> AddItem(IItem item, byte? position = null)
    {
        if (!item.IsPickupable) return Result<OperationResultList<IItem>>.NotPossible;

        position ??= (byte)item.Metadata.BodyPosition;

        var swappedItem = TryAddItemToSlot((Slot)position, item);

        if (swappedItem.Failed) return new Result<OperationResultList<IItem>>(swappedItem.Error);

        if (swappedItem.Value is null) return Result<OperationResultList<IItem>>.Success;

        return new Result<OperationResultList<IItem>>(new OperationResultList<IItem>(Operation.Removed,
            swappedItem.Value));
    }

    public Result<IItem> RemoveItem(Slot slot, byte amount)
    {
        var result = RemoveFromSlotOperation.Remove(this, slot, amount);
        var removedItem = result.Value;

        if (result.Failed) return Result<IItem>.Fail(result.Error);

        TotalWeight -= removedItem.Weight;

        OnItemRemovedFromSlot?.Invoke(this, removedItem, slot, amount);
        return Result<IItem>.Ok(removedItem);
    }

    public Result<OperationResultList<IItem>> RemoveItem(IItem thing, byte amount, byte fromPosition,
        out IItem removedThing)
    {
        removedThing = null;
        var result = RemoveItem((Slot)fromPosition, amount);
        if (result.Failed)
            return Result<OperationResultList<IItem>>.Fail(result.Error);

        removedThing = result.Value;
        return Result<OperationResultList<IItem>>.Ok(new OperationResultList<IItem>(removedThing));
    }

    public Result<IItem> RemoveItem(ushort itemId, byte amount, bool ignoreEquipped)
    {
        var (slot, item) = InventoryMap.GetSlotAndItemFromItemId(itemId);

        if (slot != Slot.None && slot != Slot.Backpack && !ignoreEquipped)
            return Result<IItem>.Fail(Common.InvalidOperation.NotPossible);

        if (slot == Slot.Backpack)
        {
            var backpack = InventoryMap.GetItem(Slot.Backpack);
            var backpackContainer = (IContainer)backpack.Item1;
            backpackContainer.RemoveItem(item, amount);
            return Result<IItem>.Ok(item);
        }

        return RemoveItem(slot, amount);
    }

    #endregion

    #region Event Handlers

    private void OnItemAddedToInventorySlot(IInventory inventory, IItem item, Slot slot, byte amount)
    {
        if (item.CanBeMoved) item.SetOwner(Owner);
    }

    internal void OnItemReduced(ICumulative item, Slot slot, byte amount)
    {
        if (item.Amount == 0)
        {
            RemoveItem(slot, amount);
            return;
        }

        TotalWeight -= item.Weight / item.Amount * amount;

        OnItemRemovedFromSlot?.Invoke(this, item, slot, amount);
    }

    internal void ContainerOnOnWeightChanged(float change)
    {
        TotalWeight += change;
    }

    #endregion

    #region Events

    public event AddItemToSlot OnItemAddedToSlot;
    public event RemoveItemFromSlot OnItemRemovedFromSlot;
    public event FailAddItemToSlot OnFailedToAddToSlot;
    public event ChangeInventoryWeight OnWeightChanged;

    #endregion
}