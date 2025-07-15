using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Items;

namespace NeoServer.Domain.Common.Contracts.Items;

public interface IItemType
{
    ushort ServerId { get; }
    ushort ClientId { get; }

    string Name => Attributes.GetAttribute(ItemTypeAttribute.Name);
    string Article => Attributes.GetAttribute(ItemTypeAttribute.Article);
    string Plural => Attributes.GetAttribute(ItemTypeAttribute.PluralName);
    float Weight => Attributes.GetAttribute<float>(ItemTypeAttribute.Weight);
    ushort AttackPower => Attributes.GetAttribute<ushort>(ItemTypeAttribute.Attack);
    ushort Defense => Attributes.GetAttribute<ushort>(ItemTypeAttribute.Defense);
    ushort ExtraDefense => Attributes.GetAttribute<ushort>(ItemTypeAttribute.ExtraDefense);
    ushort Armor => Attributes.GetAttribute<ushort>(ItemTypeAttribute.Armor);
    sbyte ExtraHitChance => Attributes.GetAttribute<sbyte>(ItemTypeAttribute.HitChance);
    byte Range => Attributes.GetAttribute<byte>(ItemTypeAttribute.Range);

    ushort Charges
        => Attributes.GetAttribute<ushort>(ItemTypeAttribute.Charges);

    ushort Count
        => Attributes.GetAttribute<ushort>(ItemTypeAttribute.Count);

    string Description { get; }

    string FullName { get; }
    string PluralName => Plural ?? $"{Name}s";

    ISet<ItemFlag> Flags { get; }

    ItemGroup Group { get; }

    ushort Speed { get; }
    ItemTypeAttributeList Attributes { get; }
    ShootType ShootType { get; }
    AmmoType AmmoType { get; }
    WeaponType WeaponType { get; }
    Slot BodyPosition { get; }
    ushort TransformTo { get; }
    ushort DestroyTo { get; }
    ItemTypeAttributeList OnUse { get; }
    DamageType DamageType { get; }
    EffectT EffectT { get; }

    void SetName(string value);
    void SetArticle(string article);
    void SetPlural(string plural);

    bool HasFlag(ItemFlag flag);
    void SetOnUse();
    bool HasAtLeastOneFlag(params ItemFlag[] flags);
    void SetGroupIfNone();

    bool IsCorpse() => Attributes.HasAttribute(ItemTypeAttribute.CorpseType);

    bool IsMovable() =>  Flags.Contains(ItemFlag.Movable);

    bool IsFluidContainer() => Flags.Contains(ItemFlag.LiquidContainer);

    bool IsSplash() => Group == ItemGroup.Splash;

    bool IsStackable() => Group == ItemGroup.Splash;

    bool IsKey() => Flags.Contains(ItemFlag.Key);

    bool HasSubType() => IsFluidContainer() || IsSplash() || IsStackable() || Charges != 0;
    void ThrowIfLocked();
}