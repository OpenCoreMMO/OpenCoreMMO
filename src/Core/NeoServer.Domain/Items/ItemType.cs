using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Effects.Parsers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Item.Structs;
using NeoServer.Domain.Common.Parsers;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Items.Helpers;

namespace NeoServer.Domain.Items;

public class ItemType : IItemType
{
    public ItemType()
    {
        ServerId = 0;
        Flags = new HashSet<ItemFlag>();
        Attributes = new ItemAttributeList();
        Locked = false;
    }

    public bool Locked { get; private set; }
    public ushort WareId { get; }
    public LightBlock LightBlock { get; private set; }

    /// <summary>
    ///     Server Id
    /// </summary>
    public ushort ServerId { get; private set; }

    /// <summary>
    ///     ItemType's name
    /// </summary>
    public string Name => Attributes.GetAttribute(ItemAttribute.Name);

    public string FullName => string.IsNullOrWhiteSpace(Article)
        ? $"{Name}"
        : $"{Article} {Name}";

    /// <summary>
    ///     ItemType's description
    /// </summary>
    public string Description => Attributes.GetAttribute(ItemAttribute.Description);

    public ISet<ItemFlag> Flags { get; set; }

    public ItemAttributeList Attributes { get; set; }
    public ItemAttributeList OnUse { get; private set; }

    public ushort ClientId { get; private set; }
    public ushort TransformTo => Attributes.GetTransformationItem();
    public ushort DestroyTo => Attributes.GetDestructionItem();

    public ItemGroup Group { get; private set; }

    public ushort Speed => Attributes.GetAttribute<ushort>(ItemAttribute.AttackSpeed);
    public string Article => Attributes.GetAttribute(ItemAttribute.Article);
    public string Plural => Attributes.GetAttribute(ItemAttribute.PluralName);
    public float Weight => Attributes.GetAttribute<float>(ItemAttribute.Weight);

    public void SetName(string name)
    {
        Attributes.SetAttribute(ItemAttribute.Name, name);
        ThrowIfLocked();
    }

    public void SetOnUse()
    {
        ThrowIfLocked();
        if (OnUse is not null) return;
        OnUse = new ItemAttributeList();
    }

    public void SetArticle(string article)
    {
        Attributes.SetAttribute(ItemAttribute.Article, article);
        ThrowIfLocked();
    }

    public void SetPlural(string plural)
    {
        Attributes.SetAttribute(ItemAttribute.PluralName, plural);
        ThrowIfLocked();
    }

    public bool HasFlag(ItemFlag flag)
    {
        return Flags.Contains(flag);
    }

    public bool HasAtLeastOneFlag(params ItemFlag[] flags)
    {
        foreach (var flag in flags)
            if (Flags.Contains(flag))
                return true;

        return false;
    }

    public AmmoType AmmoType => Attributes?.GetAttribute(ItemAttribute.AmmoType) switch
    {
        "bolt" => AmmoType.Bolt,
        "arrow" => AmmoType.Arrow,
        _ => AmmoType.None
    };

    public Slot BodyPosition => SlotTypeParser.Parse(Attributes);
    public ShootType ShootType => ShootTypeParser.Parse(Attributes?.GetAttribute(ItemAttribute.ShootType));
    public WeaponType WeaponType => WeaponTypeParser.Parse(Attributes?.GetAttribute(ItemAttribute.WeaponType));
    public DamageType DamageType => DamageTypeParser.Parse(Attributes?.GetAttribute(ItemAttribute.Damage));
    public EffectT EffectT => EffectParser.Parse(Attributes?.GetAttribute(ItemAttribute.Effect));

    public void SetGroupIfNone()
    {
        if (Group is not ItemGroup.None) return;

        Group = ItemGroupQuery.Find(this);
    }

    public void SetSpeed(ushort speed)
    {
        Attributes.SetAttribute(ItemAttribute.AttackSpeed, speed);
        ThrowIfLocked();
    }

    public void SetLight(LightBlock lightBlock)
    {
        ThrowIfLocked();
        LightBlock = lightBlock;
    }

    public void LockChanges()
    {
        Locked = true;
    }

    public void ThrowIfLocked()
    {
        if (Locked) throw new InvalidOperationException("This ItemType is locked and cannot be altered.");
    }

    public void SetGroup(byte type)
    {
        ThrowIfLocked();
        Group = (ItemGroup)type;
    }

    public IItemType SetId(ushort typeId)
    {
        ThrowIfLocked();

        ServerId = typeId;

        return this;
    }

    public void SetClientId(ushort clientId)
    {
        ClientId = clientId;
    }

    public void SetFlag(ItemFlag flag)
    {
        ThrowIfLocked();

        Flags.Add(flag);
    }

    public void ParseOTFlags(uint flags)
    {
        if (HasOTFlag(flags, 1 << 0)) // blockSolid
            SetFlag(ItemFlag.Unpassable);

        if (HasOTFlag(flags, 1 << 1)) // blockProjectile
            SetFlag(ItemFlag.BlockProjectTile);

        if (HasOTFlag(flags, 1 << 2)) // blockPathFind
            SetFlag(ItemFlag.BlockPathFind);

        if (HasOTFlag(flags, 1 << 3)) // hasElevation
            SetFlag(ItemFlag.HasHeight);

        if (HasOTFlag(flags, 1 << 4)) // isUsable
            SetFlag(ItemFlag.Usable);

        if (HasOTFlag(flags, 1 << 5)) // isPickupable
            SetFlag(ItemFlag.Pickupable);

        if (HasOTFlag(flags, 1 << 6)) // isMoveable
            SetFlag(ItemFlag.Movable);

        if (HasOTFlag(flags, 1 << 7)) // isStackable
            SetFlag(ItemFlag.Stackable);

        if (HasOTFlag(flags, 1 << 13)) // alwaysOnTop
            SetFlag(ItemFlag.AlwaysOnTop);

        if (HasOTFlag(flags, 1 << 14)) // isReadable
            SetFlag(ItemFlag.Readable);

        if (HasOTFlag(flags, 1 << 15)) // isRotatable
            SetFlag(ItemFlag.Rotatable);

        if (HasOTFlag(flags, 1 << 16)) // isHangable
            SetFlag(ItemFlag.Hangable);

        if (HasOTFlag(flags, 1 << 17)) // isVertical
            SetFlag(ItemFlag.Vertical);

        if (HasOTFlag(flags, 1 << 18)) // isHorizontal
            SetFlag(ItemFlag.Horizontal);

        //if (HasFlag(flags, 1 << 19)) // cannotDecay -- unused

        if (HasOTFlag(flags, 1 << 20)) // allowDistRead
            SetFlag(ItemFlag.AllowDistRead);

        //if (HasFlag(flags, 1 << 21)) // unused -- unused

        //if (HasFlag(flags, 1 << 22)) // isAnimation -- unused

        if (HasOTFlag(flags, 1 << 23)) // lookTrough
            SetFlag(ItemFlag.LookTrough);

        if (HasOTFlag(flags, 1 << 26)) // forceUse
            SetFlag(ItemFlag.ForceUse);
    }

    private bool HasOTFlag(uint flags, uint flag)
    {
        return (flags & flag) != 0;
    }

    public IItemType Clone()
    {
        var clone = new ItemType
        {
            // Copia campos simples
            ServerId = this.ServerId,
            ClientId = this.ClientId,
            Group = this.Group,
            LightBlock = this.LightBlock,
            Locked = this.Locked,
            Flags = new HashSet<ItemFlag>(this.Flags),
            Attributes = this.Attributes?.Clone(),
            OnUse = this.OnUse?.Clone()
        };
        return clone;
    }
}