namespace NeoServer.Domain.Common.Item;

public enum ItemGroup : byte
{
    None = 0,
    Ground = 1,
    Container = 2,
    Splash = 11,
    Fluid = 12,
    Deprecated = 13,
    ItemGroupLast = 14,
    Ammo,
    DistanceWeapon,
    MagicWeapon,
    MeleeWeapon,
    ThrowableDistanceWeapon,
    BodyDefenseEquipment,
    FloorChanger,
    MagicField,
    Paper,
    Sign,
    Teleport,
    Depot,
    Coin,
    Food,
    Healing,
    Cumulative,
    AttackRune,
    FieldRune,
    UsableFloorChanger,
    UsableOn
}