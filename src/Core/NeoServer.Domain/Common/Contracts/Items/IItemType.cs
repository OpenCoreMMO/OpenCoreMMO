using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Items;

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
    ItemAttributeList Attributes { get; }
    ShootType ShootType { get; }
    AmmoType AmmoType { get; }
    WeaponType WeaponType { get; }
    Slot BodyPosition { get; }
    float Weight { get; }
    ushort TransformTo { get; }
    ushort DestroyTo { get; }
    string Plural { get; }
    ItemAttributeList OnUse { get; }
    DamageType DamageType { get; }
    EffectT EffectT { get; }

    ushort Charges
        => Attributes.GetAttribute<ushort>(ItemAttribute.Charges);

    ushort Count
        => Attributes.GetAttribute<ushort>(ItemAttribute.ShowCount);

    void SetArticle(string article);
    void SetPlural(string plural);

    void UpdateName(string value);
    bool HasFlag(ItemFlag flag);
    void SetOnUse();
    bool HasAtLeastOneFlag(params ItemFlag[] flags);
    void SetGroupIfNone();

    bool IsCorpse() => Attributes.HasAttribute(ItemAttribute.CorpseType);

    bool IsMovable() =>  Flags.Contains(ItemFlag.Movable);

    bool IsFluidContainer() => Flags.Contains(ItemFlag.LiquidContainer);

    bool IsSplash() => Group == ItemGroup.Splash;

    bool IsStackable() => Group == ItemGroup.Splash;

    bool IsKey() => Flags.Contains(ItemFlag.Key);

    bool HasSubType() => IsFluidContainer() || IsSplash() || IsStackable() || Charges != 0;
}