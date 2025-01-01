## Revscript support using native LuaJIT
Simple documentation with all scripts and functions developed using LuaJIT to working with Revscript TFS and Canary retrocompatibility.

### Implemented Scripts (21)

**Actions Items (9)**

- crowbar.lua
- fishing.lua
- kitchen_knife.lua
- ladder_up.lua
- machete.lua
- pick.lua
- rope.lua
- shovel.lua
- scythe.lua

**Actions Quests (4)**

- dark_helmet.lua
- dwarven_shield.lua
- spike_sword.lua
- combat_knife_quest.lua

**Talk Actions (11)**

- reload.lua `/reload scripts` 
- up.lua `/up` 
- down.lua `/down` 
- create_monster.lua `/m scarab` 
- create_summon.lua `/m rat` 
- create_npc.lua `/n Eryn` 
- create_item.lua `/i Rope` 
- teleport_to_creature.lua `/goto` 
- ghost.lua `/ghost` 
- teleport_ntiles.lua `/n 5` 
- position.lua `!position`

**Global Events (3)**

- example_one.lua
- example_two.lua
- online_record.lua

### Implemented Libs using lua (8)

- creature.lua
- item.lua
- load.lua
- player.lua
- position.lua
- revscriptsys.lua
- tile.lua
- actions.lua

### Tables (18) and Functions (124)

**Action (7)**

- Action()
- action:onUse(callback)
- action:register()
- action:id(ids)
- action:aid(ids)
- action:uid(ids)
- action:allowFarUse(bool)

**ConfigManager (4)**

- configManager:getString(type)
- configManager:getNumber(type)
- configManager:getBoolean(type)
- configManager:getFloat(type)

**Creature (7)**

- Creature(id or name or userdata)
- creature:getId()
- creature:getName()
- creature:getPosition()
- creature:getDirection()
- creature:isCreature()
- creature:isInGhostMode()

**Container (3)**

- Container(uid)
- container:getSize()
- container:getItem()

**Game (6)**

- Game.getReturnMessage(value)
- Game.createItem(itemId or name, count, position)
- Game.createMonster(monsterName, position, extended = false, force = false, master = nil)
- Game.createNpc(monsterName, position, extended = false, force = false)
- Game.reload(reloadType)
- Game.getPlayers()

**Global (3)**

- rawgetmetatable(metatableName)
- addEvent(callback, delay, ...)
- stopEvent(eventid)

**GlobalEvent (12)**

- GlobalEvent()
- globalevent:type(callback)
- globalevent:register()
- globalevent:time(time)
- globalevent:interval(interval)
- globalevent:onThink(callback)
- globalevent:onTime(callback)
- globalevent:onStartup(callback)
- globalevent:onShutdown(callback)
- globalevent:onRecord(callback)
- globalevent:onPeriodChange(callback)
- globalevent:onSave(callback)

**Group(8)**

- Group()
- group:getId()
- group:getName()
- group:getFlags()
- group:getAccess()
- group:getMaxDepotItems()
- group:getMaxVipEntries()
- group:hasFlag(flag)

**Item (17)**

- Item(uid)
- item:getId()
- item:getActionId()
- item:getUniqueId()
- item:getSubType()
- item:hasProperty()
- item:moveTo(position or cylinder, flags)
- item:transform(itemId, count/subType = -1)
- item:decay(decayId)
- item:isItem()
- item:getId()
- item:getName()
- item:getPluralName()
- item:getArticle()
- item:getPosition()
- item:getTile()
- item:hasAttribute(ITEM_ATTRIBUTE)

**ItemType (10)**

- ItemType(id or name)
- itemType:getType()
- itemType:getId()
- itemType:getName()
- itemType:isMovable()
- itemType:isStackable()
- itemType:isFluidContainer()
- itemType:isKey()
- itemType:getWeight()
- itemType:getDestroyId()

**Logger (4)**

- logger.info(text)
- logger.info(text)
- logger.info(text)
- logger.info(text)

**Monster (1)**

- Monster(id or userdata)

**Npc (1)**

- Npc(id or userdata)

**Player (15)**

- Player(id or guid or name or userdata)
- player:teleportTo(position, pushMovement = false)
- player:getFreeCapacity()
- player:getStorageValue(key)
- player:setStorageValue(key, value)
- player:getSkillLevel(skillType)
- player:getEffectiveSkillLevel(skillType)
- player:getSkillPercent(skillType)
- player:getSkillTries(skillType)
- player:addSkillTries(skillType, tries)
- player:addItem(itemId, count = 1, canDropOnMap = true, subType = 1, slot = CONST_SLOT_BACKPACK)
- player:removeItem(itemId, count, subType = -1, ignoreEquipped = false)
- player:sendTextMessage(type, text)
- player:isPzLocked()
- player:setGhostMode(positionEx)

**Position (8)**

- Position(x = 0, y = 0, z = 0, stackpos = 0) or Position(position)
- positionValue = position + positionEx
- positionValue = position - positionEx
- position:sendMagicEffect(magicEffect, player = nullptr)
- position:toString()
- position:getDistance(positionEx)
- position:getPathTo(positionEx, minTargetDist = 0, maxTargetDist = 1, fullPathSearch = true, clearSight = true, maxSearchDist = 0)
- position:isSightClear(positionEx, sameFloor = true)

**TalkAction (5)**

- TalkAction(words) or TalkAction(word1, word2, word3)
- talkAction:onSay(callback)
- talkAction:register()
- talkAction:separator(sep)
- talkAction:getName()

**Teleport (1)**

- Teleport(uid)

**Tile (11)**

- Tile(x, y, z) or Tile(position)
- tile:getThing(index)
- tile:getThingCount()
- tile:getTopVisibleThing(creature)
- tile:getPosition()
- tile:getGround()
- tile:getItems()
- tile:getItemCount()
- tile:hasProperty(property, item)
- tile:hasFlag(flag)
- tile:queryAdd(thing, flags)

### Enums (11) / Constants (200++)

**DirectionsType**

-  DIRECTION_NORTH,
-  DIRECTION_EAST,
-  DIRECTION_SOUTH,
-  DIRECTION_WEST,
-  DIRECTION_SOUTHWEST,
-  DIRECTION_SOUTHEAST,
-  DIRECTION_NORTHWEST,
-  DIRECTION_NORTHEAST,
-  DIRECTION_NONE,

**MagicEffectClassesType**

- CONST_ME_NONE,
- CONST_ME_DRAWBLOOD = 1,
- CONST_ME_LOSEENERGY = 2,
- CONST_ME_POFF = 3,
- CONST_ME_BLOCKHIT = 4,
- CONST_ME_EXPLOSIONAREA = 5,
- CONST_ME_EXPLOSIONHIT = 6,
- CONST_ME_FIREAREA = 7,
- CONST_ME_YELLOW_RINGS = 8,
- CONST_ME_GREEN_RINGS = 9,
- CONST_ME_HITAREA = 10,
- CONST_ME_TELEPORT = 11,
- CONST_ME_ENERGYHIT = 12,
- CONST_ME_MAGIC_BLUE = 13,
- CONST_ME_MAGIC_RED = 14,
- CONST_ME_MAGIC_GREEN = 15,
- CONST_ME_HITBYFIRE = 16,
- CONST_ME_HITBYPOISON = 17,
- CONST_ME_MORTAREA = 18,
- CONST_ME_SOUND_GREEN = 19,
- CONST_ME_SOUND_RED = 20,
- CONST_ME_POISONAREA = 21,
- CONST_ME_SOUND_YELLOW = 22,
- CONST_ME_SOUND_PURPLE = 23,
- CONST_ME_SOUND_BLUE = 24,
- CONST_ME_SOUND_WHITE = 25,
- CONST_ME_BUBBLES = 26,
- CONST_ME_CRAPS = 27,
- CONST_ME_GIFT_WRAPS = 28,
- CONST_ME_FIREWORK_YELLOW = 29,
- CONST_ME_FIREWORK_RED = 30,
- CONST_ME_FIREWORK_BLUE = 31,
- CONST_ME_STUN = 32,
- CONST_ME_SLEEP = 33,
- CONST_ME_WATERCREATURE = 34,
- CONST_ME_GROUNDSHAKER = 35,
- CONST_ME_HEARTS = 36,
- CONST_ME_FIREATTACK = 37,
- CONST_ME_ENERGYAREA = 38,
- CONST_ME_SMALLCLOUDS = 39,
- CONST_ME_HOLYDAMAGE = 40,
- CONST_ME_BIGCLOUDS = 41,
- CONST_ME_ICEAREA = 42,
- CONST_ME_ICETORNADO = 43,
- CONST_ME_ICEATTACK = 44,
- CONST_ME_STONES = 45,
- CONST_ME_SMALLPLANTS = 46,
- CONST_ME_CARNIPHILA = 47,
- CONST_ME_PURPLEENERGY = 48,
- CONST_ME_YELLOWENERGY = 49,
- CONST_ME_HOLYAREA = 50,
- CONST_ME_BIGPLANTS = 51,
- CONST_ME_CAKE = 52,
- CONST_ME_GIANTICE = 53,
- CONST_ME_WATERSPLASH = 54,
- CONST_ME_PLANTATTACK = 55,
- CONST_ME_TUTORIALARROW = 56,
- CONST_ME_TUTORIALSQUARE = 57,
- CONST_ME_MIRRORHORIZONTAL = 58,
- CONST_ME_MIRRORVERTICAL = 59,
- CONST_ME_SKULLHORIZONTAL = 60,
- CONST_ME_SKULLVERTICAL = 61,
- CONST_ME_ASSASSIN = 62,
- CONST_ME_STEPSHORIZONTAL = 63,
- CONST_ME_BLOODYSTEPS = 64,
- CONST_ME_STEPSVERTICAL = 65,
- CONST_ME_YALAHARIGHOST = 66,
- CONST_ME_BATS = 67,
- CONST_ME_SMOKE = 68,
- CONST_ME_INSECTS = 69,
- CONST_ME_DRAGONHEAD = 70

**MessageClassesType**

- MESSAGE_STATUS_CONSOLE_RED = 18, /*Red message in the console*/
- MESSAGE_EVENT_ORANGE = 19, /*Orange message in the console*/
- MESSAGE_STATUS_CONSOLE_ORANGE = 20,  /*Orange message in the console*/
- MESSAGE_STATUS_WARNING = 21, /*Red message in game window and in the console*/
- MESSAGE_EVENT_ADVANCE = 22, /*White message in game window and in the console*/
- MESSAGE_EVENT_DEFAULT = 23, /*White message at the bottom of the game window and in the console*/
- MESSAGE_STATUS_DEFAULT = 24, /*White message at the bottom of the game window and in the console*/
- MESSAGE_INFO_DESCR = 25, /*Green message in game window and in the console*/
- MESSAGE_STATUS_SMALL = 26, /*White message at the bottom of the game window"*/
- MESSAGE_STATUS_CONSOLE_BLUE = 27, /*FIXME Blue message in the console*/

**ReloadType**

- RELOAD_TYPE_NONE,
- RELOAD_TYPE_ALL,
- RELOAD_TYPE_CHAT,
- RELOAD_TYPE_CONFIG,
- RELOAD_TYPE_EVENTS,
- RELOAD_TYPE_CORE,
- RELOAD_TYPE_IMBUEMENTS,
- RELOAD_TYPE_ITEMS,
- RELOAD_TYPE_MODULES,
- RELOAD_TYPE_MONSTERS,
- RELOAD_TYPE_MOUNTS,
- RELOAD_TYPE_NPCS,
- RELOAD_TYPE_RAIDS,
- RELOAD_TYPE_SCRIPTS,
- RELOAD_TYPE_GROUPS,
- RELOAD_TYPE_LAST

**ReturnValueType**

- RETURNVALUE_NOERROR,
- RETURNVALUE_NOTPOSSIBLE,
- RETURNVALUE_NOTENOUGHROOM,
- RETURNVALUE_PLAYERISPZLOCKED,
- RETURNVALUE_PLAYERISNOTINVITED,
- RETURNVALUE_CANNOTTHROW,
- RETURNVALUE_THEREISNOWAY,
- RETURNVALUE_DESTINATIONOUTOFREACH,
- RETURNVALUE_CREATUREBLOCK,
- RETURNVALUE_NOTMOVEABLE,
- RETURNVALUE_DROPTWOHANDEDITEM,
- RETURNVALUE_BOTHHANDSNEEDTOBEFREE,
- RETURNVALUE_CANONLYUSEONEWEAPON,
- RETURNVALUE_NEEDEXCHANGE,
- RETURNVALUE_CANNOTBEDRESSED,
- RETURNVALUE_PUTTHISOBJECTINYOURHAND,
- RETURNVALUE_PUTTHISOBJECTINBOTHHANDS,
- RETURNVALUE_TOOFARAWAY,
- RETURNVALUE_FIRSTGODOWNSTAIRS,
- RETURNVALUE_FIRSTGOUPSTAIRS,
- RETURNVALUE_CONTAINERNOTENOUGHROOM,
- RETURNVALUE_NOTENOUGHCAPACITY,
- RETURNVALUE_CANNOTPICKUP,
- RETURNVALUE_THISISIMPOSSIBLE,
- RETURNVALUE_DEPOTISFULL,
- RETURNVALUE_CREATUREDOESNOTEXIST,
- RETURNVALUE_CANNOTUSETHISOBJECT,
- RETURNVALUE_PLAYERWITHTHISNAMEISNOTONLINE,
- RETURNVALUE_NOTREQUIREDLEVELTOUSERUNE,
- RETURNVALUE_YOUAREALREADYTRADING,
- RETURNVALUE_THISPLAYERISALREADYTRADING,
- RETURNVALUE_YOUMAYNOTLOGOUTDURINGAFIGHT,
- RETURNVALUE_DIRECTPLAYERSHOOT,
- RETURNVALUE_NOTENOUGHLEVEL,
- RETURNVALUE_NOTENOUGHMAGICLEVEL,
- RETURNVALUE_NOTENOUGHMANA,
- RETURNVALUE_NOTENOUGHSOUL,
- RETURNVALUE_YOUAREEXHAUSTED,
- RETURNVALUE_YOUCANNOTUSEOBJECTSTHATFAST,
- RETURNVALUE_PLAYERISNOTREACHABLE,
- RETURNVALUE_CANONLYUSETHISRUNEONCREATURES,
- RETURNVALUE_ACTIONNOTPERMITTEDINPROTECTIONZONE,
- RETURNVALUE_YOUMAYNOTATTACKTHISPLAYER,
- RETURNVALUE_YOUMAYNOTATTACKAPERSONINPROTECTIONZONE,
- RETURNVALUE_YOUMAYNOTATTACKAPERSONWHILEINPROTECTIONZONE,
- RETURNVALUE_YOUMAYNOTATTACKTHISCREATURE,
- RETURNVALUE_YOUCANONLYUSEITONCREATURES,
- RETURNVALUE_CREATUREISNOTREACHABLE,
- RETURNVALUE_TURNSECUREMODETOATTACKUNMARKEDPLAYERS,
- RETURNVALUE_YOUNEEDPREMIUMACCOUNT,
- RETURNVALUE_YOUNEEDTOLEARNTHISSPELL,
- RETURNVALUE_YOURVOCATIONCANNOTUSETHISSPELL,
- RETURNVALUE_YOUNEEDAWEAPONTOUSETHISSPELL,
- RETURNVALUE_PLAYERISPZLOCKEDLEAVEPVPZONE,
- RETURNVALUE_PLAYERISPZLOCKEDENTERPVPZONE,
- RETURNVALUE_ACTIONNOTPERMITTEDINANOPVPZONE,
- RETURNVALUE_YOUCANNOTLOGOUTHERE,
- RETURNVALUE_YOUNEEDAMAGICITEMTOCASTSPELL,
- RETURNVALUE_CANNOTCONJUREITEMHERE,
- RETURNVALUE_YOUNEEDTOSPLITYOURSPEARS,
- RETURNVALUE_NAMEISTOOAMBIGUOUS,
- RETURNVALUE_CANONLYUSEONESHIELD,
- RETURNVALUE_NOPARTYMEMBERSINRANGE,
- RETURNVALUE_YOUARENOTTHEOWNER,
- RETURNVALUE_NOSUCHRAIDEXISTS,
- RETURNVALUE_ANOTHERRAIDISALREADYEXECUTING,
- RETURNVALUE_TRADEPLAYERFARAWAY,
- RETURNVALUE_YOUDONTOWNTHISHOUSE,
- RETURNVALUE_TRADEPLAYERALREADYOWNSAHOUSE,
- RETURNVALUE_TRADEPLAYERHIGHESTBIDDER,
- RETURNVALUE_YOUCANNOTTRADETHISHOUSE,
- RETURNVALUE_YOUDONTHAVEREQUIREDPROFESSION,
- RETURNVALUE_ITEMCANNOTBEMOVEDTHERE,
- RETURNVALUE_YOUCANNOTUSETHISBED,

**TileFlagsType**

-  TILESTATE_NONE = 0,
-  TILESTATE_FLOORCHANGE_DOWN = 1 << 0,
-  TILESTATE_FLOORCHANGE_NORTH = 1 << 1,
-  TILESTATE_FLOORCHANGE_SOUTH = 1 << 2,
-  TILESTATE_FLOORCHANGE_EAST = 1 << 3,
-  TILESTATE_FLOORCHANGE_WEST = 1 << 4,
-  TILESTATE_FLOORCHANGE_SOUTH_ALT = 1 << 5,
-  TILESTATE_FLOORCHANGE_EAST_ALT = 1 << 6,
-  TILESTATE_PROTECTIONZONE = 1 << 7,
-  TILESTATE_NOPVPZONE = 1 << 8,
-  TILESTATE_NOLOGOUT = 1 << 9,
-  TILESTATE_PVPZONE = 1 << 10,
-  TILESTATE_TELEPORT = 1 << 11,
-  TILESTATE_MAGICFIELD = 1 << 12,
-  TILESTATE_MAILBOX = 1 << 13,
-  TILESTATE_TRASHHOLDER = 1 << 14,
-  TILESTATE_BED = 1 << 15,
-  TILESTATE_DEPOT = 1 << 16,
-  TILESTATE_BLOCKSOLID = 1 << 17,
-  TILESTATE_BLOCKPATH = 1 << 18,
-  TILESTATE_IMMOVABLEBLOCKSOLID = 1 << 19,
-  TILESTATE_IMMOVABLEBLOCKPATH = 1 << 20,
-  TILESTATE_IMMOVABLENOFIELDBLOCKPATH = 1 << 21,
-  TILESTATE_NOFIELDBLOCKPATH = 1 << 22,
-  TILESTATE_SUPPORTS_HANGABLE = 1 << 23,
-  TILESTATE_FLOORCHANGE = TILESTATE_FLOORCHANGE_DOWN | TILESTATE_FLOORCHANGE_NORTH | TILESTATE_FLOORCHANGE_SOUTH | TILESTATE_FLOORCHANGE_EAST | TILESTATE_FLOORCHANGE_WEST | TILESTATE_FLOORCHANGE_SOUTH_ALT | TILESTATE_FLOORCHANGE_EAST_ALT,

**ItemAttributeType**

- ITEM_ATTRIBUTE_NONE,
- ITEM_ATTRIBUTE_ACTIONID = 1 << 0,
- ITEM_ATTRIBUTE_UNIQUEID = 1 << 1,
- ITEM_ATTRIBUTE_DESCRIPTION = 1 << 2,
- ITEM_ATTRIBUTE_TEXT = 1 << 3,
- ITEM_ATTRIBUTE_DATE = 1 << 4,
- ITEM_ATTRIBUTE_WRITER = 1 << 5,
- ITEM_ATTRIBUTE_NAME = 1 << 6,
- ITEM_ATTRIBUTE_ARTICLE = 1 << 7,
- ITEM_ATTRIBUTE_PLURALNAME = 1 << 8,
- ITEM_ATTRIBUTE_WEIGHT = 1 << 9,
- ITEM_ATTRIBUTE_ATTACK = 1 << 10,
- ITEM_ATTRIBUTE_DEFENSE = 1 << 11,
- ITEM_ATTRIBUTE_EXTRADEFENSE = 1 << 12,
- ITEM_ATTRIBUTE_ARMOR = 1 << 13,
- ITEM_ATTRIBUTE_HITCHANCE = 1 << 14,
- ITEM_ATTRIBUTE_SHOOTRANGE = 1 << 15,
- ITEM_ATTRIBUTE_OWNER = 1 << 16,
- ITEM_ATTRIBUTE_DURATION = 1 << 17,
- ITEM_ATTRIBUTE_DECAYSTATE = 1 << 18,
- ITEM_ATTRIBUTE_CORPSEOWNER = 1 << 19,
- ITEM_ATTRIBUTE_CHARGES = 1 << 20,
- ITEM_ATTRIBUTE_FLUIDTYPE = 1 << 21,
- ITEM_ATTRIBUTE_DOORID = 1 << 22,
- ITEM_ATTRIBUTE_DECAYTO = 1 << 23,
- ITEM_ATTRIBUTE_WRAPID = 1 << 24,
- ITEM_ATTRIBUTE_STOREITEM = 1 << 25,
- ITEM_ATTRIBUTE_ATTACK_SPEED = 1 << 26,
- ITEM_ATTRIBUTE_CUSTOM = 1U << 31

**ItemIdType**

- ITEM_FIREFIELD_PVP_FULL = 1487,
- ITEM_FIREFIELD_PVP_MEDIUM = 1488,
- ITEM_FIREFIELD_PVP_SMALL = 1489,
- ITEM_FIREFIELD_PERSISTENT_FULL = 1492,
- ITEM_FIREFIELD_PERSISTENT_MEDIUM = 1493,
- ITEM_FIREFIELD_PERSISTENT_SMALL = 1494,
- ITEM_FIREFIELD_NOPVP = 1500,
- ITEM_POISONFIELD_PVP = 1490,
- ITEM_POISONFIELD_PERSISTENT = 1496,
- ITEM_POISONFIELD_NOPVP = 1503,
- ITEM_ENERGYFIELD_PVP = 1491,
- ITEM_ENERGYFIELD_PERSISTENT = 1495,
- ITEM_ENERGYFIELD_NOPVP = 1504,
- ITEM_MAGICWALL = 1497,
- ITEM_MAGICWALL_PERSISTENT = 1498,
- ITEM_MAGICWALL_SAFE = 11098,
- ITEM_WILDGROWTH = 1499,
- ITEM_WILDGROWTH_PERSISTENT = 2721,
- ITEM_WILDGROWTH_SAFE = 11099,
- ITEM_BAG = 1987,
- ITEM_BACKPACK = 1988,
- ITEM_GOLD_COIN = 2148,
- ITEM_PLATINUM_COIN = 2152,
- ITEM_CRYSTAL_COIN = 2160,
- ITEM_DEPOT = 2594,
- ITEM_LOCKER = 2589,
- ITEM_MALE_CORPSE = 3058,
- ITEM_FEMALE_CORPSE = 3065,
- ITEM_FULLSPLASH = 2016,
- ITEM_SMALLSPLASH = 2019,
- ITEM_PARCEL = 2595,
- ITEM_LETTER = 2597,
- ITEM_LETTER_STAMPED = 2598,
- ITEM_LABEL = 2599,
- ITEM_AMULETOFLOSS = 2173,
- ITEM_DOCUMENT_RO = 1968, //read-only

**SkillType**

- SKILL_FIST = 0,
- SKILL_CLUB = 1,
- SKILL_SWORD = 2,
- SKILL_AXE = 3,
- SKILL_DISTANCE = 4,
- SKILL_SHIELD = 5,
- SKILL_FISHING = 6,
- SKILL_MAGLEVEL = 7,
- SKILL_LEVEL = 8,
- SKILL_FIRST = SKILL_FIST,
- SKILL_LAST = SKILL_FISHING

**PlayerFlag**

- PlayerFlag_CannotUseCombat 
- PlayerFlag_CannotAttackPlayer
- PlayerFlag_CannotAttackMonster
- PlayerFlag_CannotBeAttacked
- PlayerFlag_CanConvinceAll
- PlayerFlag_CanSummonAll
- PlayerFlag_CanIllusionAll
- PlayerFlag_CanSenseInvisibility
- PlayerFlag_IgnoredByMonsters 
- PlayerFlag_NotGainInFight
- PlayerFlag_HasInfiniteMana
- PlayerFlag_HasInfiniteSoul 
- PlayerFlag_HasNoExhaustion 
- PlayerFlag_CannotUseSpells
- PlayerFlag_CannotPickupItem 
- PlayerFlag_CanAlwaysLogin 
- PlayerFlag_CanBroadcast
- PlayerFlag_CanEditHouses
- PlayerFlag_CannotBeBanned
- PlayerFlag_CannotBePushed
- PlayerFlag_HasInfiniteCapacity
- PlayerFlag_CanPushAllCreatures
- PlayerFlag_CanTalkRedPrivate
- PlayerFlag_CanTalkRedChannel
- PlayerFlag_TalkOrangeHelpChannel
- PlayerFlag_NotGainExperience
- PlayerFlag_NotGainMana
- PlayerFlag_NotGainHealth
- PlayerFlag_NotGainSkill 
- PlayerFlag_SetMaxSpeed
- PlayerFlag_SpecialVIP
- PlayerFlag_NotGenerateLoot 
- PlayerFlag_CanTalkRedChannelAnonymous
- PlayerFlag_IgnoreProtectionZone
- PlayerFlag_IgnoreSpellCheck
- PlayerFlag_IgnoreWeaponCheck
- PlayerFlag_CannotBeMuted 
- PlayerFlag_IsAlwaysPremium
- PlayerFlag_IgnoreYellCheck
- PlayerFlag_IgnoreSendPrivateCheck

**ItemPropertyType**

- CONST_PROP_BLOCKSOLID = 0,
- CONST_PROP_HASHEIGHT,
- CONST_PROP_BLOCKPROJECTILE,
- CONST_PROP_BLOCKPATH,
- CONST_PROP_ISVERTICAL,
- CONST_PROP_ISHORIZONTAL,
- CONST_PROP_MOVEABLE,
- CONST_PROP_IMMOVABLEBLOCKSOLID,
- CONST_PROP_IMMOVABLEBLOCKPATH,
- CONST_PROP_IMMOVABLENOFIELDBLOCKPATH,
- CONST_PROP_NOFIELDBLOCKPATH,
- CONST_PROP_SUPPORTHANGABLE

### Demonstrations

**fishing**
https://github.com/user-attachments/assets/ca8a7648-6fdf-49ab-a27f-cd2937b78538