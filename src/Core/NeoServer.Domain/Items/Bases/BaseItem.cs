using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Factories.AttributeFactory;
using NeoServer.Domain.Items.Inspection;
using NeoServer.Domain.Items.Items;
using NeoServer.Domain.Items.Items.Attributes;
using NeoServer.Domain.Items.Items.Containers;

namespace NeoServer.Domain.Items.Bases;

public abstract class BaseItem : IItem
{
    private IThing _owner;

    protected BaseItem(IItemType metadata, Location location)
    {
        Location = location;
        Metadata = metadata;

        Decay = DecayTrackerFactory.CreateIfItemIsDecayable(this);
        Attributes = new ItemAttributeList();
    }

    public static Func<IItem, IPlayer, bool> UseFunction { get; set; }
    public ItemAttributeList Attributes { get; set; }

    public ushort ActionId => Attributes.GetAttribute<ushort>(ItemAttribute.ActionId);
    public uint UniqueId => Attributes.GetAttribute<uint>(ItemAttribute.UniqueId);
    public string Name => Attributes.GetAttribute(ItemAttribute.Name) ?? Metadata.Name;
    public string Article => Attributes.GetAttribute(ItemAttribute.Article) ?? Metadata.Article;
    public string Plural => Attributes.GetAttribute(ItemAttribute.PluralName) ?? Metadata.PluralName;
    public virtual bool IsMailable => this is Parcel || this is Letter;

    public virtual float Weight =>
        Attributes.TryGetAttribute<float>(ItemAttribute.Weight, out var weight)
            ? weight
            : Metadata.Weight;

    public ushort AttackPower =>
        Attributes.TryGetAttribute<ushort>(ItemAttribute.Attack, out var attack)
            ? attack
            : Metadata.AttackPower;

    public ushort Defense =>
        Attributes.TryGetAttribute<ushort>(ItemAttribute.Defense, out var defense)
            ? defense
            : Metadata.Defense;

    public ushort ExtraDefense =>
        Attributes.TryGetAttribute<ushort>(ItemAttribute.ExtraDefense, out var extraDefense)
            ? extraDefense
            : Metadata.ExtraDefense;

    public ushort Armor =>
        Attributes.TryGetAttribute<ushort>(ItemAttribute.Armor, out var armor)
            ? armor
            : Metadata.Armor;

    public sbyte ExtraHitChance =>
        Attributes.TryGetAttribute<sbyte>(ItemAttribute.HitChance, out var hitChance)
            ? hitChance
            : Metadata.ExtraHitChance;

    public byte Range =>
        Attributes.TryGetAttribute<byte>(ItemAttribute.ShootRange, out var shootRange)
            ? shootRange
            : Metadata.Range;

    public string FullName
    {
        get
        {
            if (Attributes.HasAttribute(ItemAttribute.Article))
                return string.IsNullOrWhiteSpace(Attributes.GetAttribute(ItemAttribute.Article))
                    ? $"{Attributes.GetAttribute(ItemAttribute.Name)}"
                    : $"{Attributes.GetAttribute(ItemAttribute.Article)} {Attributes.GetAttribute(ItemAttribute.Name)}";

            return Metadata.FullName;
        }
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        OnDeleted?.Invoke(this);
    }

    public ChargeCounter Charges { get; init; }

    public bool IsDeleted { get; private set; }

    public void OnItemRemoved(IThing from)
    {
        OnRemoved?.Invoke(this, from);
    }

    public IItemType Metadata { get; private set; }

    public void UpdateMetadata(IItemType newMetadata)
    {
        Metadata = newMetadata;
    }

    public Location Location { get; set; }

    public void SetNewLocation(Location location, bool force = false)
    {
        if (!((IItem)this).CanBeMoved && !force) return;
        Location = location;
    }

    public virtual string GetLookText(
        bool isClose = false, bool showInternalDetails = false)
    {
        return InspectionTextBuilder.IsApplicable(this)
            ? InspectionTextBuilder.Build(this, isClose, showInternalDetails)
            : $"You see {Article} {Name}.";
    }

    public byte Amount => Attributes.TryGetAttribute<byte>(ItemAttribute.Count, out var count)
        ? count
        : (byte)Metadata.Count;

    public virtual void Use(IPlayer usedBy)
    {
        UseFunction?.Invoke(this, usedBy);
    }

    public IThing Parent { get; internal set; }

    public virtual void SetParent(IThing parent)
    {
        Parent = parent;
    }

    public void SetOwner(IThing owner)
    {
        Owner = owner;
    }

    public IThing Owner
    {
        get => _owner is IContainer container ? container.RootParent : _owner;
        private set => _owner = value;
    }

    #region Decay

    public DecayTracker Decay { get; protected set; }

    #endregion

    public override string ToString()
    {
        var plural = Plural ?? $"{Name}s";
        return Amount > 1 ? $"{Amount} {plural}" : FullName;
    }

    #region Events

    public event ItemDelete OnDeleted;
    public event ItemRemove OnRemoved;

    #endregion
}