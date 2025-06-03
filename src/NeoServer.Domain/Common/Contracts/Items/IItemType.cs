using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Creatures.Players;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Common.Contracts.Items;

public interface IItemType
{
    ushort ServerId { get; }

    string Name { get; }
    string FullName { get; }
    string PluralName => Plural ?? $"{Name}s";

    string Description { get; }

    ISet<ItemFlag> Flags { get; }

    ItemGroup Group { get; }

    ushort ClientId { get; }

    ushort Speed { get; }
    string Article { get; }
    IItemAttributeList Attributes { get; }
    ShootType ShootType { get; }
    AmmoType AmmoType { get; }
    WeaponType WeaponType { get; }
    Slot BodyPosition { get; }
    float Weight { get; }
    ushort TransformTo { get; }
    ushort DestroyTo { get; }
    string Plural { get; }
    IItemAttributeList OnUse { get; }
    DamageType DamageType { get; }
    EffectT EffectT { get; }

    ushort Charges
        => Attributes.GetAttribute<ushort>(ItemAttribute.Charges);

    ushort Count
        => Attributes.GetAttribute<ushort>(ItemAttribute.Count);

    void SetArticle(string article);
    void SetPlural(string plural);

    void UpdateName(string value);
    bool HasFlag(ItemFlag flag);
    void SetOnUse();
    bool HasAtLeastOneFlag(params ItemFlag[] flags);
    void SetGroupIfNone();

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
        return Group == ItemGroup.Splash;
    }

    bool IsKey()
    {
        return Flags.Contains(ItemFlag.Key);
    }

    bool HasSubType()
    {
        return IsFluidContainer() || IsSplash() || IsStackable() || Charges != 0;
    }
}