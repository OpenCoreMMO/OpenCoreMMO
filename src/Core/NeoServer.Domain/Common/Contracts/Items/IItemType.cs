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
    byte TopOrder { get; set; }

    void SetName(string value);
    void SetArticle(string article);
    void SetPlural(string plural);

    bool HasFlag(ItemFlag flag);
    void SetOnUse();
    bool HasAtLeastOneFlag(params ItemFlag[] flags);
    void SetGroupIfNone();

    bool IsCorpse()
    {
        return Attributes.HasAttribute(ItemTypeAttribute.CorpseType);
    }

    bool IsMovable()
    {
        return Flags.Contains(ItemFlag.Movable);
    }

    bool IsFluidContainer()
    {
        return Flags.Contains(ItemFlag.LiquidContainer);
    }

    bool IsSplash()
    {
        return Group == ItemGroup.Splash;
    }

    bool IsStackable()
    {
        return Flags.Contains(ItemFlag.Stackable);
    }

    bool IsKey()
    {
        return Flags.Contains(ItemFlag.Key);
    }

    bool HasSubType()
    {
        return IsFluidContainer() || IsSplash() || IsStackable() || Charges != 0;
    }

    void ThrowIfLocked();
}