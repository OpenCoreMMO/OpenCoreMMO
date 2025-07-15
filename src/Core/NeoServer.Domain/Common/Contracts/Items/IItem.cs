using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Items;

namespace NeoServer.Domain.Common.Contracts.Items;

public delegate void ItemDelete(IItem item);

public delegate void ItemRemove(IItem item, IThing from);

public interface IItem : IThing, IHasDecay
{
    /// <summary>
    ///     Item metadata. Contains a lot of information about item
    /// </summary>
    IItemType Metadata { get; }
    ItemAttributeList Attributes { get; }

    ushort ActionId => Attributes.GetAttribute<ushort>(ItemAttribute.ActionId);
    uint UniqueId => Attributes.GetAttribute<uint>(ItemAttribute.UniqueId);
    string IThing.Name => Attributes.GetAttribute(ItemAttribute.Name) ?? Metadata.Name;
    string Article => Attributes.GetAttribute(ItemAttribute.Article) ?? Metadata.Article;
    string Plural => Attributes.GetAttribute(ItemAttribute.PluralName) ?? Metadata.PluralName;

    float Weight =>
        Attributes.TryGetAttribute<float>(ItemAttribute.Weight, out var weight)
            ? weight
            : Metadata.Weight;

    ushort Attack =>
        Attributes.TryGetAttribute<ushort>(ItemAttribute.Attack, out var attack)
            ? attack
            : Metadata.AttackPower;

    ushort Defense =>
        Attributes.TryGetAttribute<ushort>(ItemAttribute.Defense, out var defense)
            ? defense
            : Metadata.Defense;

    ushort ExtraDefense =>
        Attributes.TryGetAttribute<ushort>(ItemAttribute.ExtraDefense, out var extraDefense)
            ? extraDefense
            : Metadata.ExtraDefense;

    ushort Armor =>
        Attributes.TryGetAttribute<ushort>(ItemAttribute.Armor, out var armor)
            ? armor
            : Metadata.Armor;

    sbyte ExtraHitChance =>
        Attributes.TryGetAttribute<sbyte>(ItemAttribute.HitChance, out var hitChance)
            ? hitChance
            : Metadata.ExtraHitChance;

    byte Range =>
        Attributes.TryGetAttribute<byte>(ItemAttribute.ShootRange, out var shootRange)
            ? shootRange
            : Metadata.Range;

    public bool IsDeleted { get; }
    IThing Owner { get; }

    IThing Parent { get; }

    string InspectionText => string.Empty;
    string CloseInspectionText => string.Empty;
    ushort ClientId => Metadata.ClientId;
    ushort ServerId => Metadata.ServerId;
    ushort CanTransformTo => Metadata.Attributes.GetTransformationItem();
    bool CanBeMoved => Metadata.HasFlag(ItemFlag.Movable);
    bool IsBlockeable => Metadata.HasFlag(ItemFlag.Unpassable);
    bool IsTransformable => CanTransformTo != default;
    bool BlockPathFinding => Metadata.HasFlag(ItemFlag.BlockPathFind);
    bool IsCumulative => Metadata.HasFlag(ItemFlag.Stackable);

    bool IsAlwaysOnTop => Metadata.HasFlag(ItemFlag.AlwaysOnTop) ||
                          Metadata.HasFlag(ItemFlag.Hangable);

    bool CanHang => Metadata.HasFlag(ItemFlag.Horizontal) || Metadata.HasFlag(ItemFlag.Vertical);

    bool IsPickupable => Metadata.HasFlag(ItemFlag.Pickupable);
    bool IsUsable => Metadata.HasFlag(ItemFlag.Usable);
    bool IsAntiProjectile => Metadata.HasFlag(ItemFlag.BlockProjectTile);
    bool IsContainer => Metadata.Group == ItemGroup.Container;
    bool IsTeleport => Metadata.Group == ItemGroup.Teleport;

    bool AllowFarUse => Metadata.Attributes.GetAttribute<bool>(ItemTypeAttribute.AllowFarUse);

    FloorChangeDirection FloorDirection => Metadata.Attributes.GetFloorChangeDirection();

    bool HasDecayBehavior
    {
        get
        {
            var hasShowDuration =
                Metadata.Attributes.TryGetAttribute<ushort>(ItemTypeAttribute.ShowDuration, out _);
            var hasDuration = Metadata.Attributes.TryGetAttribute<ushort>(ItemTypeAttribute.Duration, out _);

            return hasShowDuration || hasDuration;
        }
    }

    public string FullName
    {
        get
        {
            if (Attributes.HasAttribute(ItemAttribute.Article))
            {
               return string.IsNullOrWhiteSpace(Attributes.GetAttribute(ItemAttribute.Article))
                ? $"{Attributes.GetAttribute(ItemAttribute.Name)}"
                : $"{Attributes.GetAttribute(ItemAttribute.Article)} {Attributes.GetAttribute(ItemAttribute.Name)}";
            }

            return Metadata.FullName;
        }
    }

    void UpdateMetadata(IItemType newMetadata);
    void MarkAsDeleted();

    Span<byte> GetRaw()
    {
        return BitConverter.GetBytes(ClientId);
    }

    void SetOwner(IThing owner);
    event ItemDelete OnDeleted;
    void SetParent(IThing parent);
    event ItemRemove OnRemoved;
    void OnItemRemoved(IThing from);

    ushort GetSubType()
    {
        if (Metadata.HasFlag(ItemFlag.LiquidContainer))
        {
            var fluidType = Metadata.Attributes.GetAttribute<ushort>(ItemTypeAttribute.ContainerLiquidType);
            return fluidType;
        }

        if (Metadata.HasFlag(ItemFlag.Stackable))
        {
            var count = Metadata.Attributes.GetAttribute<ushort>(ItemTypeAttribute.Count);
            return count;
        }

        var charges = Metadata.Attributes.GetAttribute<ushort>(ItemTypeAttribute.Charges);
        return charges;
    }
}