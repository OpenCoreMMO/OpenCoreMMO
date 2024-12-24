using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Effects.Parsers;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Item.Structs;
using NeoServer.Game.Common.Parsers;
using NeoServer.Game.Items.Helpers;

namespace NeoServer.Game.Items;

public class ItemType : IItemType
{
    public ItemType()
    {
        ServerId = 0;
        Name = string.Empty;
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
    public string Name { get; private set; }

    public string FullName => string.IsNullOrWhiteSpace(Article)
        ? $"{Name}"
        : $"{Article} {Name}";

    /// <summary>
    ///     ItemType's description
    /// </summary>
    public string Description => Attributes.GetAttribute(ItemAttribute.Description);

    public ISet<ItemFlag> Flags { get; }

    public IItemAttributeList Attributes { get; }
    public IItemAttributeList OnUse { get; private set; }

    public ushort ClientId { get; private set; }
    public ushort TransformTo => Attributes.GetTransformationItem();

    public ItemGroup Group { get; private set; }

    public ushort Speed => Attributes.GetAttribute<ushort>(ItemAttribute.Speed);
    public string Article { get; private set; }
    public string Plural { get; private set; }

    public float Weight => Attributes.GetAttribute<float>(ItemAttribute.Weight);

    public void SetName(string name)
    {
        ThrowIfLocked();
        Name = name;
    }

    public void SetOnUse()
    {
        ThrowIfLocked();
        if (OnUse is not null) return;
        OnUse = new ItemAttributeList();
    }

    public void SetArticle(string article)
    {
        Article = article;
    }

    public void SetPlural(string plural)
    {
        Plural = plural;
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

    public bool IsFluidContainer()
        => Flags.Contains(ItemFlag.LiquidContainer);

    public bool IsSplash()
        => Group == ItemGroup.Splash;

    public bool IsStackable()
        => Group == ItemGroup.Splash;

    public ushort Charges 
        => Attributes.GetAttribute<ushort>(ItemAttribute.Charges);

    public ushort Count
        => Attributes.GetAttribute<ushort>(ItemAttribute.Count);

    public bool HasSubType()
        => IsFluidContainer() || IsSplash() || IsStackable() || Charges != 0;

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
        Attributes.SetAttribute(ItemAttribute.Speed, speed);
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

    private void ThrowIfLocked()
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
}