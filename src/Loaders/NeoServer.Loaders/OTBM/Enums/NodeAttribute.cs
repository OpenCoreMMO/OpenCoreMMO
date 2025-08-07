namespace NeoServer.Loaders.OTBM.Enums;

/// <summary>
///     This enum is used to identify the contents of a OTBNode used to describe
///     world nodes.
/// </summary>
public enum NodeAttribute : byte
{
    None = 0,
    WorldDescription = 1, // OTBM_ATTR_DESCRIPTION = 1,
    ExtensionFile = 2, // OTBM_ATTR_EXT_FILE = 2,
    TileFlags = 3, // OTBM_ATTR_TILE_FLAGS = 3,
    ActionId = 4, // OTBM_ATTR_ACTION_ID = 4,
    UniqueId = 5, // OTBM_ATTR_UNIQUE_ID = 5,
    Text = 6, // OTBM_ATTR_TEXT = 6,
    NotTheMapDescription = 7, // OTBM_ATTR_DESC = 7,
    TeleportDestination = 8, // OTBM_ATTR_TELE_DEST = 8,
    Item = 9, // OTBM_ATTR_ITEM = 9,
    DepotId = 10, // OTBM_ATTR_DEPOT_ID = 10,
    ExtensionFileForSpawns = 11, // OTBM_ATTR_EXT_SPAWN_FILE = 11,
    RuneCharges = 12, // OTBM_ATTR_RUNE_CHARGES = 12,
    ExtensionFileForHouses = 13, // OTBM_ATTR_EXT_HOUSE_FILE = 13,
    HouseDoorId = 14, // OTBM_ATTR_HOUSEDOORID = 14,
    Count = 15, // OTBM_ATTR_COUNT = 15,
    Duration = 16, // OTBM_ATTR_DURATION = 16,
    DecayingState = 17, // OTBM_ATTR_DECAYING_STATE = 17,
    WrittenDate = 18, // OTBM_ATTR_WRITTENDATE = 18,
    WrittenBy = 19, // OTBM_ATTR_WRITTENBY = 19,
    SleeperGuid = 20, // OTBM_ATTR_SLEEPERGUID = 20,
    SleepStart = 21, // OTBM_ATTR_SLEEPSTART = 21,
    Charges = 22, // OTBM_ATTR_CHARGES = 22
    ContainerItems = 23, // OTBM_ATTR_CONTAINER_ITEMS = 23
    Name = 24, // OTBM_ATTR_NAME = 24
    Article = 25, // OTBM_ATTR_ARTICLE = 25
    PluralName = 26, // OTBM_ATTR_PLURALNAME = 26
    Weight = 27, // OTBM_ATTR_WEIGHT = 27
    Attack = 28, // OTBM_ATTR_ATTACK = 28
    Defense = 29, // OTBM_ATTR_DEFENSE = 29
    ExtraDefense = 30, // OTBM_ATTR_EXTRADEFENSE = 30
    Armor = 31, // OTBM_ATTR_ARMOR = 31
    HitChance = 32, // OTBM_ATTR_HITCHANCE = 32
    ShootRange = 33, // OTBM_ATTR_SHOOTRANGE = 33
    CustomAttributes = 34, // OTBM_ATTR_CUSTOM_ATTRIBUTES = 34
    DecayTo = 35, // OTBM_ATTR_DECAYTO = 35
    WrapId = 36, // OTBM_ATTR_WRAPID = 36
    StoreItem = 37, // OTBM_ATTR_STOREITEM = 37
    AttackSpeed = 38 // OTBM_ATTR_ATTACK_SPEED = 38
}