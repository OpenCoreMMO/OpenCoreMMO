namespace NeoServer.Loaders.OTBM.Enums;

/// <summary>
///     This is a port of TFS's enum "AttrTypes_t" definied in item.h, line 71
///     Used to parse OTBM worlds.
/// </summary>
public enum ItemNodeAttribute : byte
{
    None = 0, // We added this value to the C# version, because ughz... That's why...
    AnotherDescription = 1, // //ATTR_DESCRIPTION = 1,
    ExtensionFile = 2, // //ATTR_EXT_FILE = 2,
    TileFlags = 3, // ATTR_TILE_FLAGS = 3,
    ActionId = 4, // ATTR_ACTION_ID = 4,
    UniqueId = 5, // ATTR_UNIQUE_ID = 5,
    Text = 6, // ATTR_TEXT = 6,
    Description, // ATTR_DESC = 7,
    TeleportDestination, // ATTR_TELE_DEST = 8,
    Item, // ATTR_ITEM = 9,
    DepotId, // ATTR_DEPOT_ID = 10,
    ExtensionFileForSpawns, // //ATTR_EXT_SPAWN_FILE = 11,
    RuneCharges, // ATTR_RUNE_CHARGES = 12,
    ExtensionFileForHouses, // //ATTR_EXT_HOUSE_FILE = 13,
    HouseDoorId, // ATTR_HOUSEDOORID = 14,
    Count, // ATTR_COUNT = 15,
    Duration, // ATTR_DURATION = 16,
    DecayingState, // ATTR_DECAYING_STATE = 17,
    WrittenDate, // ATTR_WRITTENDATE = 18,
    WrittenBy, // ATTR_WRITTENBY = 19,
    SleeperGUID, // ATTR_SLEEPERGUID = 20,
    SleepStart, // ATTR_SLEEPSTART = 21,
    Charges, // ATTR_CHARGES = 22,
    ContainerItems, // ATTR_CONTAINER_ITEMS = 23,
    Name, // ATTR_NAME = 24,
    Article, // ATTR_ARTICLE = 25,
    PluralName, // ATTR_PLURALNAME = 26,
    Weight, // ATTR_WEIGHT = 27,
    Attack, // ATTR_ATTACK = 28,
    Defense, // ATTR_DEFENSE = 29,
    ExtraDefense, // ATTR_EXTRADEFENSE = 30,
    Armor, // ATTR_ARMOR = 31,
    HitChance, // ATTR_HITCHANCE = 32,
    ShootRange, // ATTR_SHOOTRANGE = 33,
    CustomAttributes, // ATTR_CUSTOM_ATTRIBUTES = 34
    DecayTo // ATTR_DECAYTO = 35
}