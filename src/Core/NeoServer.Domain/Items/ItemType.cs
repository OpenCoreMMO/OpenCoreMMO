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
        Attributes = new ItemTypeAttributeList();
        Locked = false;
    }

    public bool Locked { get; private set; }
    public ushort WareId { get; }
    public LightBlock LightBlock { get; private set; }

    /// <summary>
    ///     Server Id
    /// </summary>
    public ushort ServerId { get; private set; }

    public ushort ClientId { get; private set; }

    /// <summary>
    ///     ItemType's name
    /// </summary>
    public string Name => Attributes.GetAttribute(ItemTypeAttribute.Name);

    public string Article => Attributes.GetAttribute(ItemTypeAttribute.Article);
    public string Plural => Attributes.GetAttribute(ItemTypeAttribute.PluralName);
    public float Weight => Attributes.GetAttribute<float>(ItemTypeAttribute.Weight);
    public ushort AttackPower => Attributes.GetAttribute<ushort>(ItemTypeAttribute.Attack);
    public ushort Defense => Attributes.GetAttribute<ushort>(ItemTypeAttribute.Defense);
    public ushort ExtraDefense => Attributes.GetAttribute<ushort>(ItemTypeAttribute.ExtraDefense);
    public ushort Armor => Attributes.GetAttribute<ushort>(ItemTypeAttribute.Armor);
    public sbyte ExtraHitChance => Attributes.GetAttribute<sbyte>(ItemTypeAttribute.HitChance);
    public byte Range => Attributes.GetAttribute<byte>(ItemTypeAttribute.Range);

    public ushort Speed => Attributes.GetAttribute<ushort>(ItemTypeAttribute.Speed);

    public ushort Charges
        => Attributes.GetAttribute<ushort>(ItemTypeAttribute.Charges);

    public ushort Count
        => Attributes.GetAttribute<ushort>(ItemTypeAttribute.Count) > 0 ? Attributes.GetAttribute<ushort>(ItemTypeAttribute.Count) : (ushort)1;

    /// <summary>
    ///     ItemType's description
    /// </summary>
    public string Description => Attributes.GetAttribute(ItemTypeAttribute.Description);

    public string FullName => string.IsNullOrWhiteSpace(Article)
        ? $"{Name}"
        : $"{Article} {Name}";

    public ISet<ItemFlag> Flags { get; set; }

    public ItemTypeAttributeList Attributes { get; set; }
    public ItemTypeAttributeList OnUse { get; private set; }

    public ushort TransformTo => Attributes.GetTransformationItem();
    public ushort DestroyTo => Attributes.GetDestructionItem();

    public ItemGroup Group { get; private set; }

    public void SetName(string name)
    {
        Attributes.SetAttribute(ItemTypeAttribute.Name, name);
        ThrowIfLocked();
    }

    public void SetOnUse()
    {
        ThrowIfLocked();
        if (OnUse is not null) return;
        OnUse = new ItemTypeAttributeList();
    }

    public void SetArticle(string article)
    {
        Attributes.SetAttribute(ItemTypeAttribute.Article, article);
        ThrowIfLocked();
    }

    public void SetPlural(string plural)
    {
        Attributes.SetAttribute(ItemTypeAttribute.PluralName, plural);
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

    public AmmoType AmmoType => Attributes?.GetAttribute(ItemTypeAttribute.AmmoType) switch
    {
        "bolt" => AmmoType.Bolt,
        "arrow" => AmmoType.Arrow,
        _ => AmmoType.None
    };

    public Slot BodyPosition => SlotTypeParser.Parse(Attributes);
    public ShootType ShootType => ShootTypeParser.Parse(Attributes?.GetAttribute(ItemTypeAttribute.ShootType));
    public WeaponType WeaponType => WeaponTypeParser.Parse(Attributes?.GetAttribute(ItemTypeAttribute.WeaponType));
    public DamageType DamageType => DamageTypeParser.Parse(Attributes?.GetAttribute(ItemTypeAttribute.Damage));
    public EffectT EffectT => EffectParser.Parse(Attributes?.GetAttribute(ItemTypeAttribute.Effect));

    public void SetGroupIfNone()
    {
        if (Group is not ItemGroup.None) return;

        Group = ItemGroupQuery.Find(this);
    }

    public void ThrowIfLocked()
    {
        if (Locked) throw new InvalidOperationException("This ItemType is locked and cannot be altered.");
    }

    public void SetSpeed(ushort speed)
    {
        Attributes.SetAttribute(ItemTypeAttribute.AttackSpeed, speed);
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

        if (HasOTFlag(flags, 1 << 23)) // lookTrough
            SetFlag(ItemFlag.LookTrough);

        if (HasOTFlag(flags, 1 << 24)) // isAnimation
            SetFlag(ItemFlag.Animation);

        if (HasOTFlag(flags, 1 << 26)) // forceUse
            SetFlag(ItemFlag.ForceUse);
    }

    private bool HasOTFlag(uint flags, uint flag)
    {
        return (flags & flag) != 0;
    }
}