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

    ushort ActionId { get; }
    uint UniqueId { get; }
    string Article { get; }
    string Plural { get; }

    float Weight { get; }

    ushort AttackPower { get; }

    ushort Defense { get; }

    ushort ExtraDefense { get; }

    ushort Armor { get; }

    sbyte ExtraHitChance { get; }

    byte Range { get; }

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