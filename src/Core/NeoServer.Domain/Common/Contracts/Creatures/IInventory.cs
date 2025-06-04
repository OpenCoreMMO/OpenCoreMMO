using NeoServer.Domain.Common.Combat;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.Items.Types.Body;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Items.Items.Weapons;

namespace NeoServer.Domain.Common.Contracts.Creatures;

public delegate void RemoveItemFromSlot(IInventory inventory, IItem item, Slot slot, byte amount = 1);

public delegate void AddItemToSlot(IInventory inventory, IItem item, Slot slot, byte amount = 1);

public delegate void FailAddItemToSlot(IPlayer player, InvalidOperation invalidOperation);

public delegate void ChangeInventoryWeight(IInventory inventory);

public interface IInventory : IHasItem
{
    IPlayer Owner { get; }
    ushort TotalAttack { get; }

    ushort TotalDefense { get; }

    ushort TotalArmor { get; }

    byte AttackRange { get; }
    IContainer BackpackSlot { get; }
    IWeapon Weapon { get; }
    bool HasShield { get; }
    float TotalWeight { get; }
    IDictionary<ushort, uint> Map { get; }
    IEnumerable<IItem> DressingItems { get; }
    bool IsUsingWeapon { get; }
    Ammo Ammo { get; }
    float AttackRate { get; }
    ElementalDamage TotalElementalAttack { get; }
    IItem this[Slot slot] { get; }
    ulong GetTotalMoney(ICoinTypeStore coinTypeStore);
    Result<IItem> RemoveItem(Slot slot, byte amount);
    Result<IItem> RemoveItem(ushort itemId, byte amount, bool ignoreEquipped);
    T TryGetItem<T>(Slot slot);
    Result<OperationResultList<IItem>> AddItem(IItem item, Slot slot = Slot.None);

    bool UpdateItem(IItem item, IItemType newType);

    void Protect(CombatDamage damage);

    #region Events

    event AddItemToSlot OnItemAddedToSlot;
    event FailAddItemToSlot OnFailedToAddToSlot;
    event RemoveItemFromSlot OnItemRemovedFromSlot;
    event ChangeInventoryWeight OnWeightChanged;

    #endregion
}