namespace NeoServer.Scripts.LuaJIT;

// Enums
public enum IconsType
{
    ICON_POISON = 1 << 0,
    ICON_BURN = 1 << 1,
    ICON_ENERGY = 1 << 2,
    ICON_DRUNK = 1 << 3,
    ICON_MANASHIELD = 1 << 4,
    ICON_PARALYZE = 1 << 5,
    ICON_HASTE = 1 << 6,
    ICON_SWORDS = 1 << 7,
    ICON_DROWNING = 1 << 8,
    ICON_FREEZING = 1 << 9,
    ICON_DAZZLED = 1 << 10,
    ICON_CURSED = 1 << 11,
    ICON_PARTY_BUFF = 1 << 12,
    ICON_REDSWORDS = 1 << 13,
    ICON_PIGEON = 1 << 14,
    ICON_BLEEDING = 1 << 15,
    ICON_LESSERHEX = 1 << 16,
    ICON_INTENSEHEX = 1 << 17,
    ICON_GREATERHEX = 1 << 18,
    ICON_ROOTED = 1 << 19,
    ICON_FEARED = 1 << 20,
    ICON_GOSHNAR1 = 1 << 21,
    ICON_GOSHNAR2 = 1 << 22,
    ICON_GOSHNAR3 = 1 << 23,
    ICON_GOSHNAR4 = 1 << 24,
    ICON_GOSHNAR5 = 1 << 25,
    ICON_NEWMANASHIELD = 1 << 26
}

public enum WieldInfoType
{
    WIELDINFO_NONE = 0,
    WIELDINFO_LEVEL = 1 << 0,
    WIELDINFO_MAGLV = 1 << 1,
    WIELDINFO_VOCREQ = 1 << 2,
    WIELDINFO_PREMIUM = 1 << 3
}

public enum SpawnTypeType
{
    RESPAWN_IN_ALL = 0,
    RESPAWN_IN_DAY = 1,
    RESPAWN_IN_NIGHT = 2,
    RESPAWN_IN_DAY_CAVE = 3,
    RESPAWN_IN_NIGHT_CAVE = 4
}

public enum Cipbia_ElementalsType : byte
{
    CIPBIA_ELEMENTAL_PHYSICAL = 0,
    CIPBIA_ELEMENTAL_FIRE = 1,
    CIPBIA_ELEMENTAL_EARTH = 2,
    CIPBIA_ELEMENTAL_ENERGY = 3,
    CIPBIA_ELEMENTAL_ICE = 4,
    CIPBIA_ELEMENTAL_HOLY = 5,
    CIPBIA_ELEMENTAL_DEATH = 6,
    CIPBIA_ELEMENTAL_HEALING = 7,
    CIPBIA_ELEMENTAL_DROWN = 8,
    CIPBIA_ELEMENTAL_LIFEDRAIN = 9,
    CIPBIA_ELEMENTAL_MANADRAIN = 10,
    CIPBIA_ELEMENTAL_AGONY = 11,
    CIPBIA_ELEMENTAL_UNDEFINED = 12
}

public enum MagicEffectClassesType : byte
{
    CONST_ME_NONE,

    CONST_ME_DRAWBLOOD = 1,
    CONST_ME_LOSEENERGY = 2,
    CONST_ME_POFF = 3,
    CONST_ME_BLOCKHIT = 4,
    CONST_ME_EXPLOSIONAREA = 5,
    CONST_ME_EXPLOSIONHIT = 6,
    CONST_ME_FIREAREA = 7,
    CONST_ME_YELLOW_RINGS = 8,
    CONST_ME_GREEN_RINGS = 9,
    CONST_ME_HITAREA = 10,
    CONST_ME_TELEPORT = 11,
    CONST_ME_ENERGYHIT = 12,
    CONST_ME_MAGIC_BLUE = 13,
    CONST_ME_MAGIC_RED = 14,
    CONST_ME_MAGIC_GREEN = 15,
    CONST_ME_HITBYFIRE = 16,
    CONST_ME_HITBYPOISON = 17,
    CONST_ME_MORTAREA = 18,
    CONST_ME_SOUND_GREEN = 19,
    CONST_ME_SOUND_RED = 20,
    CONST_ME_POISONAREA = 21,
    CONST_ME_SOUND_YELLOW = 22,
    CONST_ME_SOUND_PURPLE = 23,
    CONST_ME_SOUND_BLUE = 24,
    CONST_ME_SOUND_WHITE = 25,
    CONST_ME_BUBBLES = 26,
    CONST_ME_CRAPS = 27,
    CONST_ME_GIFT_WRAPS = 28,
    CONST_ME_FIREWORK_YELLOW = 29,
    CONST_ME_FIREWORK_RED = 30,
    CONST_ME_FIREWORK_BLUE = 31,
    CONST_ME_STUN = 32,
    CONST_ME_SLEEP = 33,
    CONST_ME_WATERCREATURE = 34,
    CONST_ME_GROUNDSHAKER = 35,
    CONST_ME_HEARTS = 36,
    CONST_ME_FIREATTACK = 37,
    CONST_ME_ENERGYAREA = 38,
    CONST_ME_SMALLCLOUDS = 39,
    CONST_ME_HOLYDAMAGE = 40,
    CONST_ME_BIGCLOUDS = 41,
    CONST_ME_ICEAREA = 42,
    CONST_ME_ICETORNADO = 43,
    CONST_ME_ICEATTACK = 44,
    CONST_ME_STONES = 45,
    CONST_ME_SMALLPLANTS = 46,
    CONST_ME_CARNIPHILA = 47,
    CONST_ME_PURPLEENERGY = 48,
    CONST_ME_YELLOWENERGY = 49,
    CONST_ME_HOLYAREA = 50,
    CONST_ME_BIGPLANTS = 51,
    CONST_ME_CAKE = 52,
    CONST_ME_GIANTICE = 53,
    CONST_ME_WATERSPLASH = 54,
    CONST_ME_PLANTATTACK = 55,
    CONST_ME_TUTORIALARROW = 56,
    CONST_ME_TUTORIALSQUARE = 57,
    CONST_ME_MIRRORHORIZONTAL = 58,
    CONST_ME_MIRRORVERTICAL = 59,
    CONST_ME_SKULLHORIZONTAL = 60,
    CONST_ME_SKULLVERTICAL = 61,
    CONST_ME_ASSASSIN = 62,
    CONST_ME_STEPSHORIZONTAL = 63,
    CONST_ME_BLOODYSTEPS = 64,
    CONST_ME_STEPSVERTICAL = 65,
    CONST_ME_YALAHARIGHOST = 66,
    CONST_ME_BATS = 67,
    CONST_ME_SMOKE = 68,
    CONST_ME_INSECTS = 69,
    CONST_ME_DRAGONHEAD = 70
}

public enum ShootTypeType : byte
{
    CONST_ANI_NONE,

    CONST_ANI_SPEAR = 1,
    CONST_ANI_BOLT = 2,
    CONST_ANI_ARROW = 3,
    CONST_ANI_FIRE = 4,
    CONST_ANI_ENERGY = 5,
    CONST_ANI_POISONARROW = 6,
    CONST_ANI_BURSTARROW = 7,
    CONST_ANI_THROWINGSTAR = 8,
    CONST_ANI_THROWINGKNIFE = 9,
    CONST_ANI_SMALLSTONE = 10,
    CONST_ANI_DEATH = 11,
    CONST_ANI_LARGEROCK = 12,
    CONST_ANI_SNOWBALL = 13,
    CONST_ANI_POWERBOLT = 14,
    CONST_ANI_POISON = 15,
    CONST_ANI_INFERNALBOLT = 16,
    CONST_ANI_HUNTINGSPEAR = 17,
    CONST_ANI_ENCHANTEDSPEAR = 18,
    CONST_ANI_REDSTAR = 19,
    CONST_ANI_GREENSTAR = 20,
    CONST_ANI_ROYALSPEAR = 21,
    CONST_ANI_SNIPERARROW = 22,
    CONST_ANI_ONYXARROW = 23,
    CONST_ANI_PIERCINGBOLT = 24,
    CONST_ANI_WHIRLWINDSWORD = 25,
    CONST_ANI_WHIRLWINDAXE = 26,
    CONST_ANI_WHIRLWINDCLUB = 27,
    CONST_ANI_ETHEREALSPEAR = 28,
    CONST_ANI_ICE = 29,
    CONST_ANI_EARTH = 30,
    CONST_ANI_HOLY = 31,
    CONST_ANI_SUDDENDEATH = 32,
    CONST_ANI_FLASHARROW = 33,
    CONST_ANI_FLAMMINGARROW = 34,
    CONST_ANI_SHIVERARROW = 35,
    CONST_ANI_ENERGYBALL = 36,
    CONST_ANI_SMALLICE = 37,
    CONST_ANI_SMALLHOLY = 38,
    CONST_ANI_SMALLEARTH = 39,
    CONST_ANI_EARTHARROW = 40,
    CONST_ANI_EXPLOSION = 41,
    CONST_ANI_CAKE = 42,

    CONST_ANI_TARSALARROW = 44,
    CONST_ANI_VORTEXBOLT = 45,

    CONST_ANI_PRISMATICBOLT = 48,
    CONST_ANI_CRYSTALLINEARROW = 49,
    CONST_ANI_DRILLBOLT = 50,
    CONST_ANI_ENVENOMEDARROW = 51,

    CONST_ANI_GLOOTHSPEAR = 53,
    CONST_ANI_SIMPLEARROW = 54,

    CONST_ANI_LEAFSTAR = 56,
    CONST_ANI_DIAMONDARROW = 57,
    CONST_ANI_SPECTRALBOLT = 58,
    CONST_ANI_ROYALSTAR = 59,

    CONST_ANI_LAST = CONST_ANI_ROYALSTAR,

    // for internal use, don't send to client
    CONST_ANI_WEAPONTYPE = 0xFE // 254
}

//public enum SpeakClassesType : byte
//{
//    TALKTYPE_SAY = 1,
//    TALKTYPE_WHISPER = 2,
//    TALKTYPE_YELL = 3,
//    TALKTYPE_PRIVATE_FROM = 4,
//    TALKTYPE_PRIVATE_TO = 5,
//    TALKTYPE_CHANNEL_MANAGER = 6,
//    TALKTYPE_CHANNEL_Y = 7,
//    TALKTYPE_CHANNEL_O = 8,
//    TALKTYPE_SPELL_USE = 9,
//    TALKTYPE_PRIVATE_NP = 10,
//    TALKTYPE_NPC_UNKOWN = 11, /* no effect (?)*/
//    TALKTYPE_PRIVATE_PN = 12,
//    TALKTYPE_BROADCAST = 13,
//    TALKTYPE_CHANNEL_R1 = 14, // red - #c text
//    TALKTYPE_PRIVATE_RED_FROM = 15, //@name@text
//    TALKTYPE_PRIVATE_RED_TO = 16, //@name@text
//    TALKTYPE_MONSTER_SAY = 36,
//    TALKTYPE_MONSTER_YELL = 37,

//    TALKTYPE_MONSTER_LAST_OLDPROTOCOL = 38, /* Dont forget about the CHANNEL_R2*/
//    TALKTYPE_CHANNEL_R2 = 0xFF // #d
//};

public enum MessageClassesType : byte
{
    MESSAGE_STATUS_CONSOLE_RED = 18, /*Red message in the console*/
    MESSAGE_EVENT_ORANGE = 19, /*Orange message in the console*/
    MESSAGE_STATUS_CONSOLE_ORANGE = 20, /*Orange message in the console*/
    MESSAGE_STATUS_WARNING = 21, /*Red message in game window and in the console*/
    MESSAGE_EVENT_ADVANCE = 22, /*White message in game window and in the console*/
    MESSAGE_EVENT_DEFAULT = 23, /*White message at the bottom of the game window and in the console*/
    MESSAGE_STATUS_DEFAULT = 24, /*White message at the bottom of the game window and in the console*/
    MESSAGE_INFO_DESCR = 25, /*Green message in game window and in the console*/
    MESSAGE_STATUS_SMALL = 26, /*White message at the bottom of the game window"*/
    MESSAGE_STATUS_CONSOLE_BLUE = 27 /*FIXME Blue message in the console*/
}

public enum FluidsType : byte
{
    FLUID_NONE = 0, /* Blue */
    FLUID_WATER = 1, /* Blue */
    FLUID_WINE = 2, /* Purple */
    FLUID_BEER = 3, /* Brown */
    FLUID_MUD = 4, /* Brown */
    FLUID_BLOOD = 5, /* Red */
    FLUID_SLIME = 6, /* Green */
    FLUID_OIL = 7, /* Brown */
    FLUID_URINE = 8, /* Yellow */
    FLUID_MILK = 9, /* White */
    FLUID_MANA = 10, /* Purple */
    FLUID_LIFE = 11, /* Red */
    FLUID_LEMONADE = 12, /* Yellow */
    FLUID_RUM = 13, /* Brown */
    FLUID_FRUITJUICE = 14, /* Yellow */
    FLUID_COCONUTMILK = 15, /* White */
    FLUID_MEAD = 16, /* Brown */
    FLUID_TEA = 17, /* Brown */

    FLUID_INK = 18 /* Black */
    // 12.85 last fluid is 18, 19+ is a loop from 0 to 18 over and over again
}

public enum SquareColorType : byte
{
    SQ_COLOR_BLACK = 0
}

public enum TextColorType : byte
{
    TEXTCOLOR_BLUE = 5,
    TEXTCOLOR_LIGHTGREEN = 30,
    TEXTCOLOR_LIGHTBLUE = 35,
    TEXTCOLOR_DARKBROWN = 78,
    TEXTCOLOR_DARKGREY = 86,
    TEXTCOLOR_MAYABLUE = 95,
    TEXTCOLOR_DARKRED = 108,
    TEXTCOLOR_NEUTRALDAMAGE = 109,
    TEXTCOLOR_LIGHTGREY = 129,
    TEXTCOLOR_SKYBLUE = 143,
    TEXTCOLOR_PURPLE = 154,
    TEXTCOLOR_ELECTRICPURPLE = 155,
    TEXTCOLOR_RED = 180,
    TEXTCOLOR_PASTELRED = 194,
    TEXTCOLOR_ORANGE = 198,
    TEXTCOLOR_LIGHTPURPLE = 199,
    TEXTCOLOR_YELLOW = 210,
    TEXTCOLOR_WHITE_EXP = 215,
    TEXTCOLOR_NONE = 255
}

public enum WeaponTypeType : byte
{
    WEAPON_NONE,
    WEAPON_SWORD,
    WEAPON_CLUB,
    WEAPON_AXE,
    WEAPON_SHIELD,
    WEAPON_DISTANCE,
    WEAPON_WAND,
    WEAPON_AMMO,
    WEAPON_MISSILE
}

public enum AmmoType : byte
{
    AMMO_NONE,
    AMMO_BOLT,
    AMMO_ARROW,
    AMMO_SPEAR,
    AMMO_THROWINGSTAR,
    AMMO_THROWINGKNIFE,
    AMMO_STONE,
    AMMO_SNOWBALL
}

public enum WeaponActionType : byte
{
    WEAPONACTION_NONE,
    WEAPONACTION_REMOVECOUNT,
    WEAPONACTION_REMOVECHARGE,
    WEAPONACTION_MOVE
}

public enum PartyAnalyzerActionType : byte
{
    PARTYANALYZERACTION_RESET = 0,
    PARTYANALYZERACTION_PRICETYPE = 1,
    PARTYANALYZERACTION_PRICEVALUE = 2
}

public enum SkullsType : byte
{
    SKULL_NONE = 0,
    SKULL_YELLOW = 1,
    SKULL_GREEN = 2,
    SKULL_WHITE = 3,
    SKULL_RED = 4,
    SKULL_BLACK = 5,
    SKULL_ORANGE = 6
}

public enum PartyShieldsType : byte
{
    SHIELD_NONE = 0,
    SHIELD_WHITEYELLOW = 1,
    SHIELD_WHITEBLUE = 2,
    SHIELD_BLUE = 3,
    SHIELD_YELLOW = 4,
    SHIELD_BLUE_SHAREDEXP = 5,
    SHIELD_YELLOW_SHAREDEXP = 6,
    SHIELD_BLUE_NOSHAREDEXP_BLINK = 7,
    SHIELD_YELLOW_NOSHAREDEXP_BLINK = 8,
    SHIELD_BLUE_NOSHAREDEXP = 9,
    SHIELD_YELLOW_NOSHAREDEXP = 10,
    SHIELD_GRAY = 11
}

public enum GuildEmblemsType : byte
{
    GUILDEMBLEM_NONE = 0,
    GUILDEMBLEM_ALLY = 1,
    GUILDEMBLEM_ENEMY = 2,
    GUILDEMBLEM_NEUTRAL = 3,
    GUILDEMBLEM_MEMBER = 4,
    GUILDEMBLEM_OTHER = 5
}

public enum ItemIdType : ushort
{
    ITEM_FIREFIELD_PVP_FULL = 1487,
    ITEM_FIREFIELD_PVP_MEDIUM = 1488,
    ITEM_FIREFIELD_PVP_SMALL = 1489,
    ITEM_FIREFIELD_PERSISTENT_FULL = 1492,
    ITEM_FIREFIELD_PERSISTENT_MEDIUM = 1493,
    ITEM_FIREFIELD_PERSISTENT_SMALL = 1494,
    ITEM_FIREFIELD_NOPVP = 1500,

    ITEM_POISONFIELD_PVP = 1490,
    ITEM_POISONFIELD_PERSISTENT = 1496,
    ITEM_POISONFIELD_NOPVP = 1503,

    ITEM_ENERGYFIELD_PVP = 1491,
    ITEM_ENERGYFIELD_PERSISTENT = 1495,
    ITEM_ENERGYFIELD_NOPVP = 1504,

    ITEM_MAGICWALL = 1497,
    ITEM_MAGICWALL_PERSISTENT = 1498,
    ITEM_MAGICWALL_SAFE = 11098,

    ITEM_WILDGROWTH = 1499,
    ITEM_WILDGROWTH_PERSISTENT = 2721,
    ITEM_WILDGROWTH_SAFE = 11099,

    ITEM_BAG = 1987,
    ITEM_BACKPACK = 1988,

    ITEM_GOLD_COIN = 2148,
    ITEM_PLATINUM_COIN = 2152,
    ITEM_CRYSTAL_COIN = 2160,

    ITEM_DEPOT = 2594,
    ITEM_LOCKER = 2589,

    ITEM_MALE_CORPSE = 3058,
    ITEM_FEMALE_CORPSE = 3065,

    ITEM_FULLSPLASH = 2016,
    ITEM_SMALLSPLASH = 2019,

    ITEM_PARCEL = 2595,
    ITEM_LETTER = 2597,
    ITEM_LETTER_STAMPED = 2598,
    ITEM_LABEL = 2599,

    ITEM_AMULETOFLOSS = 2173,

    ITEM_DOCUMENT_RO = 1968
};

public enum PlayerFlagsType : byte
{
    CannotUseCombat,
    CannotAttackPlayer,
    CannotAttackMonster,
    CannotBeAttacked,
    CanConvinceAll,
    CanSummonAll,
    CanIllusionAll,
    CanSenseInvisibility,
    IgnoredByMonsters,
    NotGainInFight,
    HasInfiniteMana,
    HasInfiniteSoul,
    HasNoExhaustion,
    CannotUseSpells,
    CannotPickupItem,
    CanAlwaysLogin,
    CanBroadcast,
    CanEditHouses,
    CannotBeBanned,
    CannotBePushed,
    HasInfiniteCapacity,
    CanPushAllCreatures,
    CanTalkRedPrivate,
    CanTalkRedChannel,
    TalkOrangeHelpChannel,
    NotGainExperience,
    NotGainMana,
    NotGainHealth,
    NotGainSkill,
    SetMaxSpeed,
    SpecialVIP,
    NotGenerateLoot,
    CanTalkRedChannelAnonymous,
    IgnoreProtectionZone,
    IgnoreSpellCheck,
    IgnoreWeaponCheck,
    CannotBeMuted,
    IsAlwaysPremium,
    CanMapClickTeleport,
    IgnoredByNpcs,

    // Must always be the last
    FlagLast
}

public enum BlessingsType : byte
{
    TWIST_OF_FATE = 1,
    WISDOM_OF_SOLITUDE = 2,
    SPARK_OF_THE_PHOENIX = 3,
    FIRE_OF_THE_SUNS = 4,
    SPIRITUAL_SHIELDING = 5,
    EMBRACE_OF_TIBIA = 6,
    BLOOD_OF_THE_MOUNTAIN = 7,
    HEARTH_OF_THE_MOUNTAIN = 8
}

public enum BedItemPartType : byte
{
    BED_NONE_PART,
    BED_PILLOW_PART,
    BED_BLANKET_PART
}

public enum AttrSubIdType
{
    None,
    TrainParty,
    ProtectParty,
    EnchantParty,
    JeanPierreMagic,
    JeanPierreMelee,
    JeanPierreDistance,
    JeanPierreDefense,
    JeanPierreFishing,
    BloodRageProtector,
    Sharpshooter
}

public enum ConcoctionType : ushort
{
    KooldownAid = 36723,
    StaminaExtension = 36725,
    StrikeEnhancement = 36724,
    CharmUpgrade = 36726,
    WealthDuplex = 36727,
    BestiaryBetterment = 36728,
    FireResilience = 36729,
    IceResilience = 36730,
    EarthResilience = 36731,
    EnergyResilience = 36732,
    HolyResilience = 36733,
    DeathResilience = 36734,
    PhysicalResilience = 36735,
    FireAmplification = 36736,
    IceAmplification = 36737,
    EarthAmplification = 36738,
    EnergyAmplification = 36739,
    HolyAmplification = 36740,
    DeathAmplification = 36741,
    PhysicalAmplification = 36742
}