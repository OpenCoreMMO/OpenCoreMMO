using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Items;

namespace NeoServer.Domain.Common.Contracts.Items;

public interface IItemType
{
    ushort ServerId { get; }
    ushort ClientId { get; }

    string Name { get; }
    string Article { get; }
    string Plural { get; }
    float Weight { get; }
    ushort AttackPower { get; }
    ushort Defense { get; }
    ushort ExtraDefense { get; }
    ushort Armor { get; }
    sbyte ExtraHitChance { get; }
    byte Range { get; }

    ushort Charges { get; }

    ushort Count { get; }

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
    bool IsAnimation() => Flags.Contains(ItemFlag.Animation);
}