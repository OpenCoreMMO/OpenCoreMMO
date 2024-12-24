using LuaNET;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class ConfigManager : IConfigManager
{
    #region Instance

    private static ConfigManager _instance;
    public static ConfigManager GetInstance() => _instance == null ? _instance = new ConfigManager() : _instance;

    #endregion

    #region Members

    private readonly string[] stringConfig = new string[(int)StringConfigType.LAST_STRING_CONFIG];
    private readonly int[] integerConfig = new int[(int)IntegerConfigType.LAST_INTEGER_CONFIG];
    private readonly bool[] booleanConfig = new bool[(int)BooleanConfigType.LAST_BOOLEAN_CONFIG];
    private readonly float[] floatingConfig = new float[(int)FloatingConfigType.LAST_FLOATING_CONFIG];

    private bool loaded = false;

    #endregion

    #region Injection

    /// <summary>
    /// A reference to the logger in use.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    public ConfigManager(ILogger logger)
    {
        _instance = this;

        _logger = logger.ForContext<ConfigManager>();
    }

    public bool Load(string file)
    {
        configFileLua = file.Split("\\").LastOrDefault();
        var L = Lua.NewState();
        if (L.pointer == 0)
        {
            throw new IOException("Failed to allocate memory");
        }

        Lua.OpenLibs(L);

        if (Lua.DoFile(L, file) > 0)
        {
            _logger.Error("[ConfigManager::load] - {0}", Lua.ToString(L, -1));
            Lua.Close(L);
            return false;
        }

        //#if !DEBUG_LOG
        //            g_logger().setLevel(getGlobalString(L, "logLevel", "info"));
        //#endif

        // Parse config
        // Info that must be loaded one time (unless we reset the modules involved)
        if (!loaded)
        {
            //booleanConfig[(int)BooleanConfig.BIND_ONLY_GLOBAL_ADDRESS] = GetGlobalBoolean(L, "bindOnlyGlobalAddress", false);
            //bool[OPTIMIZE_DATABASE] = getGlobalBoolean(L, "startupDatabaseOptimization", true);
            //bool[TOGGLE_MAP_CUSTOM] = getGlobalBoolean(L, "toggleMapCustom", true);
            //bool[TOGGLE_MAINTAIN_MODE] = getGlobalBoolean(L, "toggleMaintainMode", false);
            //string[MAINTAIN_MODE_MESSAGE] = getGlobalString(L, "maintainModeMessage", "");

            //string[IP] = getGlobalString(L, "ip", "127.0.0.1");
            //string[MAP_NAME] = getGlobalString(L, "mapName", "canary");
            //string[MAP_DOWNLOAD_URL] = getGlobalString(L, "mapDownloadUrl", "");
            //string[MAP_AUTHOR] = getGlobalString(L, "mapAuthor", "Eduardo Dantas");

            //string[MAP_CUSTOM_NAME] = getGlobalString(L, "mapCustomName", "");
            //string[MAP_CUSTOM_AUTHOR] = getGlobalString(L, "mapCustomAuthor", "OTServBR");

            //string[HOUSE_RENT_PERIOD] = getGlobalString(L, "houseRentPeriod", "never");
            //float[HOUSE_PRICE_RENT_MULTIPLIER] = getGlobalFloat(L, "housePriceRentMultiplier", 1.0);
            //float[HOUSE_RENT_RATE] = getGlobalFloat(L, "houseRentRate", 1.0);
            //string[MYSQL_HOST] = getGlobalString(L, "mysqlHost", "127.0.0.1");
            //string[MYSQL_USER] = getGlobalString(L, "mysqlUser", "root");
            //string[MYSQL_PASS] = getGlobalString(L, "mysqlPass", "");
            //string[MYSQL_DB] = getGlobalString(L, "mysqlDatabase", "canary");
            //string[MYSQL_SOCK] = getGlobalString(L, "mysqlSock", "");

            //string[AUTH_TYPE] = getGlobalString(L, "authType", "password");
            //bool[RESET_SESSIONS_ON_STARTUP] = getGlobalBoolean(L, "resetSessionsOnStartup", false);

            //integer[SQL_PORT] = getGlobalNumber(L, "mysqlPort", 3306);

            integerConfig[(int)IntegerConfigType.GAME_PORT] = GetGlobalNumber(L, "gameProtocolPort", 7172);
            integerConfig[(int)IntegerConfigType.LOGIN_PORT] = GetGlobalNumber(L, "loginProtocolPort", 7171);
            integerConfig[(int)IntegerConfigType.STATUS_PORT] = GetGlobalNumber(L, "statusProtocolPort", 7171);

            //integer[MARKET_OFFER_DURATION] = getGlobalNumber(L, "marketOfferDuration", 30 * 24 * 60 * 60);

            //integer[FREE_DEPOT_LIMIT] = getGlobalNumber(L, "freeDepotLimit", 2000);
            //integer[PREMIUM_DEPOT_LIMIT] = getGlobalNumber(L, "premiumDepotLimit", 8000);
            //integer[DEPOT_BOXES] = getGlobalNumber(L, "depotBoxes", 20);
            //integer[STASH_ITEMS] = getGlobalNumber(L, "stashItemCount", 5000);

            //bool[OLD_PROTOCOL] = getGlobalBoolean(L, "allowOldProtocol", true);
        }

        //bool[ALLOW_CHANGEOUTFIT] = getGlobalBoolean(L, "allowChangeOutfit", true);
        //bool[ONE_PLAYER_ON_ACCOUNT] = getGlobalBoolean(L, "onePlayerOnlinePerAccount", true);
        //bool[AIMBOT_HOTKEY_ENABLED] = getGlobalBoolean(L, "hotkeyAimbotEnabled", true);
        //bool[REMOVE_RUNE_CHARGES] = getGlobalBoolean(L, "removeChargesFromRunes", true);
        //bool[EXPERIENCE_FROM_PLAYERS] = getGlobalBoolean(L, "experienceByKillingPlayers", false);
        //bool[FREE_PREMIUM] = getGlobalBoolean(L, "freePremium", false);
        //bool[REPLACE_KICK_ON_LOGIN] = getGlobalBoolean(L, "replaceKickOnLogin", true);
        //bool[MARKET_PREMIUM] = getGlobalBoolean(L, "premiumToCreateMarketOffer", true);
        //bool[EMOTE_SPELLS] = getGlobalBoolean(L, "emoteSpells", false);
        //bool[STAMINA_SYSTEM] = getGlobalBoolean(L, "staminaSystem", true);
        //bool[WARN_UNSAFE_SCRIPTS] = getGlobalBoolean(L, "warnUnsafeScripts", true);
        //bool[CONVERT_UNSAFE_SCRIPTS] = getGlobalBoolean(L, "convertUnsafeScripts", true);
        //bool[CLASSIC_ATTACK_SPEED] = getGlobalBoolean(L, "classicAttackSpeed", false);
        //bool[TOGGLE_ATTACK_SPEED_ONFIST] = getGlobalBoolean(L, "toggleAttackSpeedOnFist", false);
        //integer[MULTIPLIER_ATTACKONFIST] = getGlobalNumber(L, "multiplierSpeedOnFist", 5);
        //integer[MAX_SPEED_ATTACKONFIST] = getGlobalNumber(L, "maxSpeedOnFist", 500);
        booleanConfig[(int)BooleanConfigType.SCRIPTS_CONSOLE_LOGS] = GetGlobalBoolean(L, "showScriptsLogInConsole", true);
        //bool[STASH_MOVING] = getGlobalBoolean(L, "stashMoving", false);
        //bool[ALLOW_BLOCK_SPAWN] = getGlobalBoolean(L, "allowBlockSpawn", true);
        //bool[REMOVE_WEAPON_AMMO] = getGlobalBoolean(L, "removeWeaponAmmunition", true);
        //bool[REMOVE_WEAPON_CHARGES] = getGlobalBoolean(L, "removeWeaponCharges", true);
        //bool[REMOVE_POTION_CHARGES] = getGlobalBoolean(L, "removeChargesFromPotions", true);
        //bool[GLOBAL_SERVER_SAVE_NOTIFY_MESSAGE] = getGlobalBoolean(L, "globalServerSaveNotifyMessage", true);
        //bool[GLOBAL_SERVER_SAVE_CLEAN_MAP] = getGlobalBoolean(L, "globalServerSaveCleanMap", false);
        //bool[GLOBAL_SERVER_SAVE_CLOSE] = getGlobalBoolean(L, "globalServerSaveClose", false);
        //bool[FORCE_MONSTERTYPE_LOAD] = getGlobalBoolean(L, "forceMonsterTypesOnLoad", true);
        //bool[HOUSE_OWNED_BY_ACCOUNT] = getGlobalBoolean(L, "houseOwnedByAccount", false);
        //bool[CLEAN_PROTECTION_ZONES] = getGlobalBoolean(L, "cleanProtectionZones", false);
        //bool[GLOBAL_SERVER_SAVE_SHUTDOWN] = getGlobalBoolean(L, "globalServerSaveShutdown", true);
        //bool[PUSH_WHEN_ATTACKING] = getGlobalBoolean(L, "pushWhenAttacking", false);

        //bool[WEATHER_RAIN] = getGlobalBoolean(L, "weatherRain", false);
        //bool[WEATHER_THUNDER] = getGlobalBoolean(L, "thunderEffect", false);
        //bool[ALL_CONSOLE_LOG] = getGlobalBoolean(L, "allConsoleLog", false);
        //bool[TOGGLE_FREE_QUEST] = getGlobalBoolean(L, "toggleFreeQuest", true);
        //bool[AUTOLOOT] = getGlobalBoolean(L, "autoLoot", false);
        //bool[AUTOBANK] = getGlobalBoolean(L, "autoBank", false);
        //bool[STAMINA_TRAINER] = getGlobalBoolean(L, "staminaTrainer", false);
        //bool[STAMINA_PZ] = getGlobalBoolean(L, "staminaPz", false);
        //bool[SORT_LOOT_BY_CHANCE] = getGlobalBoolean(L, "sortLootByChance", false);
        booleanConfig[(int)BooleanConfigType.TOGGLE_SAVE_INTERVAL] = GetGlobalBoolean(L, "toggleSaveInterval", false);
        booleanConfig[(int)BooleanConfigType.TOGGLE_SAVE_INTERVAL_CLEAN_MAP] = GetGlobalBoolean(L, "toggleSaveIntervalCleanMap", false);
        //bool[TELEPORT_SUMMONS] = getGlobalBoolean(L, "teleportSummons", false);
        booleanConfig[(int)BooleanConfigType.ALLOW_RELOAD] = GetGlobalBoolean(L, "allowReload", true);

        //bool[ONLY_PREMIUM_ACCOUNT] = getGlobalBoolean(L, "onlyPremiumAccount", false);
        //bool[RATE_USE_STAGES] = getGlobalBoolean(L, "rateUseStages", false);
        //bool[TOGGLE_IMBUEMENT_SHRINE_STORAGE] = getGlobalBoolean(L, "toggleImbuementShrineStorage", true);
        //bool[TOGGLE_IMBUEMENT_NON_AGGRESSIVE_FIGHT_ONLY] = getGlobalBoolean(L, "toggleImbuementNonAggressiveFightOnly", false);

        //bool[TOGGLE_DOWNLOAD_MAP] = getGlobalBoolean(L, "toggleDownloadMap", false);
        //bool[USE_ANY_DATAPACK_FOLDER] = getGlobalBoolean(L, "useAnyDatapackFolder", false);
        //bool[INVENTORY_GLOW] = getGlobalBoolean(L, "inventoryGlowOnFiveBless", false);
        //bool[XP_DISPLAY_MODE] = getGlobalBoolean(L, "experienceDisplayRates", true);

        //string[DEFAULT_PRIORITY] = getGlobalString(L, "defaultPriority", "high");
        //string[SERVER_NAME] = getGlobalString(L, "serverName", "");
        //string[SERVER_MOTD] = getGlobalString(L, "serverMotd", "");
        //string[OWNER_NAME] = getGlobalString(L, "ownerName", "");
        //string[OWNER_EMAIL] = getGlobalString(L, "ownerEmail", "");
        //string[URL] = getGlobalString(L, "url", "");
        //string[LOCATION] = getGlobalString(L, "location", "");
        //string[WORLD_TYPE] = getGlobalString(L, "worldType", "pvp");
        //string[STORE_IMAGES_URL] = getGlobalString(L, "coinImagesURL", "");
        //string[DISCORD_WEBHOOK_URL] = getGlobalString(L, "discordWebhookURL", "");
        stringConfig[(int)StringConfigType.SAVE_INTERVAL_TYPE] = GetGlobalString(L, "saveIntervalType", "");
        //string[GLOBAL_SERVER_SAVE_TIME] = getGlobalString(L, "globalServerSaveTime", "06:00");
        //string[DATA_DIRECTORY] = getGlobalString(L, "dataPackDirectory", "data-otservbr-global");
        stringConfig[(int)StringConfigType.CORE_DIRECTORY] = GetGlobalString(L, "coreDirectory", "data");

        //string[FORGE_FIENDISH_INTERVAL_TYPE] = getGlobalString(L, "forgeFiendishIntervalType", "hour");
        //string[FORGE_FIENDISH_INTERVAL_TIME] = getGlobalString(L, "forgeFiendishIntervalTime", "1");

        //integer[MAX_PLAYERS] = getGlobalNumber(L, "maxPlayers");
        //integer[PZ_LOCKED] = getGlobalNumber(L, "pzLocked", 60000);
        //integer[DEFAULT_DESPAWNRANGE] = getGlobalNumber(L, "deSpawnRange", 2);
        //integer[DEFAULT_DESPAWNRADIUS] = getGlobalNumber(L, "deSpawnRadius", 50);
        //integer[RATE_EXPERIENCE] = getGlobalNumber(L, "rateExp", 1);
        //integer[RATE_SKILL] = getGlobalNumber(L, "rateSkill", 1);
        //integer[RATE_LOOT] = getGlobalNumber(L, "rateLoot", 1);
        //integer[RATE_MAGIC] = getGlobalNumber(L, "rateMagic", 1);
        //integer[RATE_SPAWN] = getGlobalNumber(L, "rateSpawn", 1);
        //integer[RATE_KILLING_IN_THE_NAME_OF_POINTS] = getGlobalNumber(L, "rateKillingInTheNameOfPoints", 1);

        //integer[HOUSE_PRICE_PER_SQM] = getGlobalNumber(L, "housePriceEachSQM", 1000);
        //integer[HOUSE_BUY_LEVEL] = getGlobalNumber(L, "houseBuyLevel", 0);
        //bool[HOUSE_PURSHASED_SHOW_PRICE] = getGlobalBoolean(L, "housePurchasedShowPrice", false);
        //bool[ONLY_INVITED_CAN_MOVE_HOUSE_ITEMS] = getGlobalBoolean(L, "onlyInvitedCanMoveHouseItems", true);

        //integer[ACTIONS_DELAY_INTERVAL] = getGlobalNumber(L, "timeBetweenActions", 200);
        //integer[EX_ACTIONS_DELAY_INTERVAL] = getGlobalNumber(L, "timeBetweenExActions", 1000);
        //integer[MAX_MESSAGEBUFFER] = getGlobalNumber(L, "maxMessageBuffer", 4);
        //integer[KICK_AFTER_MINUTES] = getGlobalNumber(L, "kickIdlePlayerAfterMinutes", 15);
        //integer[PROTECTION_LEVEL] = getGlobalNumber(L, "protectionLevel", 1);
        //integer[DEATH_LOSE_PERCENT] = getGlobalNumber(L, "deathLosePercent", -1);
        //integer[STATUSQUERY_TIMEOUT] = getGlobalNumber(L, "statusTimeout", 5000);
        //integer[FRAG_TIME] = getGlobalNumber(L, "timeToDecreaseFrags", 24 * 60 * 60 * 1000);
        //integer[WHITE_SKULL_TIME] = getGlobalNumber(L, "whiteSkullTime", 15 * 60 * 1000);
        //integer[STAIRHOP_DELAY] = getGlobalNumber(L, "stairJumpExhaustion", 2000);
        //integer[MAX_CONTAINER] = getGlobalNumber(L, "maxContainer", 500);
        //integer[MAX_CONTAINER_ITEM] = getGlobalNumber(L, "maxItem", 5000);
        //integer[EXP_FROM_PLAYERS_LEVEL_RANGE] = getGlobalNumber(L, "expFromPlayersLevelRange", 75);
        //integer[CHECK_EXPIRED_MARKET_OFFERS_EACH_MINUTES] = getGlobalNumber(L, "checkExpiredMarketOffersEachMinutes", 60);
        //integer[MAX_MARKET_OFFERS_AT_A_TIME_PER_PLAYER] = getGlobalNumber(L, "maxMarketOffersAtATimePerPlayer", 100);
        //integer[MAX_PACKETS_PER_SECOND] = getGlobalNumber(L, "maxPacketsPerSecond", 25);
        //integer[COMPRESSION_LEVEL] = getGlobalNumber(L, "packetCompressionLevel", 6);
        //integer[STORE_COIN_PACKET] = getGlobalNumber(L, "coinPacketSize", 25);
        //integer[DAY_KILLS_TO_RED] = getGlobalNumber(L, "dayKillsToRedSkull", 3);
        //integer[WEEK_KILLS_TO_RED] = getGlobalNumber(L, "weekKillsToRedSkull", 5);
        //integer[MONTH_KILLS_TO_RED] = getGlobalNumber(L, "monthKillsToRedSkull", 10);
        //integer[RED_SKULL_DURATION] = getGlobalNumber(L, "redSkullDuration", 30);
        //integer[BLACK_SKULL_DURATION] = getGlobalNumber(L, "blackSkullDuration", 45);
        //integer[ORANGE_SKULL_DURATION] = getGlobalNumber(L, "orangeSkullDuration", 7);
        //integer[GLOBAL_SERVER_SAVE_NOTIFY_DURATION] = getGlobalNumber(L, "globalServerSaveNotifyDuration", 5);

        //integer[PARTY_LIST_MAX_DISTANCE] = getGlobalNumber(L, "partyListMaxDistance", 0);

        //integer[PUSH_DELAY] = getGlobalNumber(L, "pushDelay", 1000);
        //integer[PUSH_DISTANCE_DELAY] = getGlobalNumber(L, "pushDistanceDelay", 1500);

        //integer[STAMINA_ORANGE_DELAY] = getGlobalNumber(L, "staminaOrangeDelay", 1);
        //integer[STAMINA_GREEN_DELAY] = getGlobalNumber(L, "staminaGreenDelay", 5);
        //integer[STAMINA_PZ_GAIN] = getGlobalNumber(L, "staminaPzGain", 1);
        //integer[STAMINA_TRAINER_DELAY] = getGlobalNumber(L, "staminaTrainerDelay", 5);
        //integer[STAMINA_TRAINER_GAIN] = getGlobalNumber(L, "staminaTrainerGain", 1);
        integerConfig[(int)IntegerConfigType.SAVE_INTERVAL_TIME] = GetGlobalNumber(L, "saveIntervalTime", 1);
        //integer[MAX_ALLOWED_ON_A_DUMMY] = getGlobalNumber(L, "maxAllowedOnADummy", 1);
        //integer[FREE_QUEST_STAGE] = getGlobalNumber(L, "freeQuestStage", 1);
        //integer[DEPOTCHEST] = getGlobalNumber(L, "depotChest", 4);
        //integer[CRITICALCHANCE] = getGlobalNumber(L, "criticalChance", 10);

        //integer[ADVENTURERSBLESSING_LEVEL] = getGlobalNumber(L, "adventurersBlessingLevel", 21);
        //integer[FORGE_MAX_ITEM_TIER] = getGlobalNumber(L, "forgeMaxItemTier", 10);
        //integer[FORGE_COST_ONE_SLIVER] = getGlobalNumber(L, "forgeCostOneSliver", 20);
        //integer[FORGE_SLIVER_AMOUNT] = getGlobalNumber(L, "forgeSliverAmount", 3);
        //integer[FORGE_CORE_COST] = getGlobalNumber(L, "forgeCoreCost", 50);
        //integer[FORGE_MAX_DUST] = getGlobalNumber(L, "forgeMaxDust", 225);
        //integer[FORGE_FUSION_DUST_COST] = getGlobalNumber(L, "forgeFusionCost", 100);
        //integer[FORGE_TRANSFER_DUST_COST] = getGlobalNumber(L, "forgeTransferCost", 100);
        //integer[FORGE_BASE_SUCCESS_RATE] = getGlobalNumber(L, "forgeBaseSuccessRate", 50);
        //integer[FORGE_BONUS_SUCCESS_RATE] = getGlobalNumber(L, "forgeBonusSuccessRate", 15);
        //integer[FORGE_TIER_LOSS_REDUCTION] = getGlobalNumber(L, "forgeTierLossReduction", 50);
        //integer[FORGE_AMOUNT_MULTIPLIER] = getGlobalNumber(L, "forgeAmountMultiplier", 3);
        //integer[FORGE_MIN_SLIVERS] = getGlobalNumber(L, "forgeMinSlivers", 3);
        //integer[FORGE_MAX_SLIVERS] = getGlobalNumber(L, "forgeMaxSlivers", 7);
        //integer[FORGE_INFLUENCED_CREATURES_LIMIT] = getGlobalNumber(L, "forgeInfluencedLimit", 300);
        //integer[FORGE_FIENDISH_CREATURES_LIMIT] = getGlobalNumber(L, "forgeFiendishLimit", 3);
        //integer[DISCORD_WEBHOOK_DELAY_MS] = getGlobalNumber(L, "discordWebhookDelayMs", Webhook::DEFAULT_DELAY_MS);

        //float[BESTIARY_RATE_CHARM_SHOP_PRICE] = getGlobalFloat(L, "bestiaryRateCharmShopPrice", 1.0);
        //float[RATE_HEALTH_REGEN] = getGlobalFloat(L, "rateHealthRegen", 1.0);
        //float[RATE_HEALTH_REGEN_SPEED] = getGlobalFloat(L, "rateHealthRegenSpeed", 1.0);
        //float[RATE_MANA_REGEN] = getGlobalFloat(L, "rateManaRegen", 1.0);
        //float[RATE_MANA_REGEN_SPEED] = getGlobalFloat(L, "rateManaRegenSpeed", 1.0);
        //float[RATE_SOUL_REGEN] = getGlobalFloat(L, "rateSoulRegen", 1.0);
        //float[RATE_SOUL_REGEN_SPEED] = getGlobalFloat(L, "rateSoulRegenSpeed", 1.0);
        //float[RATE_SPELL_COOLDOWN] = getGlobalFloat(L, "rateSpellCooldown", 1.0);
        //float[RATE_ATTACK_SPEED] = getGlobalFloat(L, "rateAttackSpeed", 1.0);
        //float[RATE_OFFLINE_TRAINING_SPEED] = getGlobalFloat(L, "rateOfflineTrainingSpeed", 1.0);
        //float[RATE_EXERCISE_TRAINING_SPEED] = getGlobalFloat(L, "rateExerciseTrainingSpeed", 1.0);

        //float[RATE_MONSTER_HEALTH] = getGlobalFloat(L, "rateMonsterHealth", 1.0);
        //float[RATE_MONSTER_ATTACK] = getGlobalFloat(L, "rateMonsterAttack", 1.0);
        //float[RATE_MONSTER_DEFENSE] = getGlobalFloat(L, "rateMonsterDefense", 1.0);
        //float[RATE_BOSS_HEALTH] = getGlobalFloat(L, "rateBossHealth", 1.0);
        //float[RATE_BOSS_ATTACK] = getGlobalFloat(L, "rateBossAttack", 1.0);
        //float[RATE_BOSS_DEFENSE] = getGlobalFloat(L, "rateBossDefense", 1.0);
        //integer[BOSS_DEFAULT_TIME_TO_FIGHT_AGAIN] = getGlobalNumber(L, "bossDefaultTimeToFightAgain", 20 * 60 * 60);
        //integer[BOSS_DEFAULT_TIME_TO_DEFEAT] = getGlobalNumber(L, "bossDefaultTimeToDefeat", 20 * 60);

        //float[RATE_NPC_HEALTH] = getGlobalFloat(L, "rateNpcHealth", 1.0);
        //float[RATE_NPC_ATTACK] = getGlobalFloat(L, "rateNpcAttack", 1.0);
        //float[RATE_NPC_DEFENSE] = getGlobalFloat(L, "rateNpcDefense", 1.0);

        //bool[PREY_ENABLED] = getGlobalBoolean(L, "preySystemEnabled", true);
        //bool[PREY_FREE_THIRD_SLOT] = getGlobalBoolean(L, "preyFreeThirdSlot", false);
        //integer[PREY_REROLL_PRICE_LEVEL] = getGlobalNumber(L, "preyRerollPricePerLevel", 200);
        //integer[PREY_SELECTION_LIST_PRICE] = getGlobalNumber(L, "preySelectListPrice", 5);
        //integer[PREY_BONUS_TIME] = getGlobalNumber(L, "preyBonusTime", 7200);
        //integer[PREY_BONUS_REROLL_PRICE] = getGlobalNumber(L, "preyBonusRerollPrice", 1);
        //integer[PREY_FREE_REROLL_TIME] = getGlobalNumber(L, "preyFreeRerollTime", 72000);

        //bool[TASK_HUNTING_ENABLED] = getGlobalBoolean(L, "taskHuntingSystemEnabled", true);
        //bool[TASK_HUNTING_FREE_THIRD_SLOT] = getGlobalBoolean(L, "taskHuntingFreeThirdSlot", false);
        //integer[TASK_HUNTING_LIMIT_EXHAUST] = getGlobalNumber(L, "taskHuntingLimitedTasksExhaust", 72000);
        //integer[TASK_HUNTING_REROLL_PRICE_LEVEL] = getGlobalNumber(L, "taskHuntingRerollPricePerLevel", 200);
        //integer[TASK_HUNTING_SELECTION_LIST_PRICE] = getGlobalNumber(L, "taskHuntingSelectListPrice", 5);
        //integer[TASK_HUNTING_BONUS_REROLL_PRICE] = getGlobalNumber(L, "taskHuntingBonusRerollPrice", 1);
        //integer[TASK_HUNTING_FREE_REROLL_TIME] = getGlobalNumber(L, "taskHuntingFreeRerollTime", 72000);

        //integer[BESTIARY_KILL_MULTIPLIER] = getGlobalNumber(L, "bestiaryKillMultiplier", 1);
        //integer[BOSSTIARY_KILL_MULTIPLIER] = getGlobalNumber(L, "bosstiaryKillMultiplier", 1);
        //bool[BOOSTED_BOSS_SLOT] = getGlobalBoolean(L, "boostedBossSlot", true);
        //integer[BOOSTED_BOSS_LOOT_BONUS] = getGlobalNumber(L, "boostedBossLootBonus", 250);
        //integer[BOOSTED_BOSS_KILL_BONUS] = getGlobalNumber(L, "boostedBossKillBonus", 3);

        //integer[FAMILIAR_TIME] = getGlobalNumber(L, "familiarTime", 30);

        //bool[TOGGLE_GOLD_POUCH_ALLOW_ANYTHING] = getGlobalBoolean(L, "toggleGoldPouchAllowAnything", false);
        //bool[TOGGLE_GOLD_POUCH_QUICKLOOT_ONLY] = getGlobalBoolean(L, "toggleGoldPouchQuickLootOnly", false);
        //bool[TOGGLE_SERVER_IS_RETRO] = getGlobalBoolean(L, "toggleServerIsRetroPVP", false);
        //bool[TOGGLE_TRAVELS_FREE] = getGlobalBoolean(L, "toggleTravelsFree", false);
        //integer[BUY_AOL_COMMAND_FEE] = getGlobalNumber(L, "buyAolCommandFee", 0);
        //integer[BUY_BLESS_COMMAND_FEE] = getGlobalNumber(L, "buyBlessCommandFee", 0);
        //bool[TELEPORT_PLAYER_TO_VOCATION_ROOM] = getGlobalBoolean(L, "teleportPlayerToVocationRoom", true);

        //bool[TOGGLE_HAZARDSYSTEM] = getGlobalBoolean(L, "toogleHazardSystem", true);
        //integer[HAZARD_CRITICAL_INTERVAL] = getGlobalNumber(L, "hazardCriticalInterval", 2000);
        //integer[HAZARD_CRITICAL_CHANCE] = getGlobalNumber(L, "hazardCriticalChance", 750);
        //integer[HAZARD_CRITICAL_MULTIPLIER] = getGlobalNumber(L, "hazardCriticalMultiplier", 25);
        //integer[HAZARD_DAMAGE_MULTIPLIER] = getGlobalNumber(L, "hazardDamageMultiplier", 200);
        //integer[HAZARD_DODGE_MULTIPLIER] = getGlobalNumber(L, "hazardDodgeMultiplier", 85);
        //integer[HAZARD_PODS_DROP_MULTIPLIER] = getGlobalNumber(L, "hazardPodsDropMultiplier", 87);
        //integer[HAZARD_PODS_TIME_TO_DAMAGE] = getGlobalNumber(L, "hazardPodsTimeToDamage", 2000);
        //integer[HAZARD_PODS_TIME_TO_SPAWN] = getGlobalNumber(L, "hazardPodsTimeToSpawn", 4000);
        //integer[HAZARD_EXP_BONUS_MULTIPLIER] = getGlobalNumber(L, "hazardExpBonusMultiplier", 2);
        //integer[HAZARD_LOOT_BONUS_MULTIPLIER] = getGlobalNumber(L, "hazardLootBonusMultiplier", 2);
        //integer[HAZARD_PODS_DAMAGE] = getGlobalNumber(L, "hazardPodsDamage", 5);
        //integer[HAZARD_SPAWN_PLUNDER_MULTIPLIER] = getGlobalNumber(L, "hazardSpawnPlunderMultiplier", 25);
        //integer[LOW_LEVEL_BONUS_EXP] = getGlobalNumber(L, "lowLevelBonusExp", 50);

        //bool[LOYALTY_ENABLED] = getGlobalBoolean(L, "loyaltyEnabled", true);
        //integer[LOYALTY_POINTS_PER_CREATION_DAY] = getGlobalNumber(L, "loyaltyPointsPerCreationDay", 1);
        //integer[LOYALTY_POINTS_PER_PREMIUM_DAY_SPENT] = getGlobalNumber(L, "loyaltyPointsPerPremiumDaySpent", 0);
        //integer[LOYALTY_POINTS_PER_PREMIUM_DAY_PURCHASED] = getGlobalNumber(L, "loyaltyPointsPerPremiumDayPurchased", 0);
        //float[LOYALTY_BONUS_PERCENTAGE_MULTIPLIER] = getGlobalFloat(L, "loyaltyBonusPercentageMultiplier", 1.0);

        //bool[TOGGLE_WHEELSYSTEM] = getGlobalBoolean(L, "wheelSystemEnabled", true);
        //integer[WHEEL_POINTS_PER_LEVEL] = getGlobalNumber(L, "wheelPointsPerLevel", 1);

        //bool[PARTY_AUTO_SHARE_EXPERIENCE] = getGlobalBoolean(L, "partyAutoShareExperience", true);
        //bool[PARTY_SHARE_LOOT_BOOSTS] = getGlobalBoolean(L, "partyShareLootBoosts", true);
        //float[PARTY_SHARE_LOOT_BOOSTS_DIMINISHING_FACTOR] = getGlobalFloat(L, "partyShareLootBoostsDimishingFactor", 0.7f);
        //integer[TIBIADROME_CONCOCTION_COOLDOWN] = getGlobalNumber(L, "tibiadromeConcoctionCooldown", 24 * 60 * 60);
        //integer[TIBIADROME_CONCOCTION_DURATION] = getGlobalNumber(L, "tibiadromeConcoctionDuration", 1 * 60 * 60);
        //string[TIBIADROME_CONCOCTION_TICK_TYPE] = getGlobalString(L, "tibiadromeConcoctionTickType", "online");

        //string[M_CONST] = getGlobalString(L, "memoryConst", "1<<16");
        //integer[T_CONST] = getGlobalNumber(L, "temporaryConst", 2);
        //integer[PARALLELISM] = getGlobalNumber(L, "parallelism", 2);

        //// Vip System
        //bool[VIP_SYSTEM_ENABLED] = getGlobalBoolean(L, "vipSystemEnabled", false);
        //integer[VIP_BONUS_EXP] = getGlobalNumber(L, "vipBonusExp", 0);
        //integer[VIP_BONUS_LOOT] = getGlobalNumber(L, "vipBonusLoot", 0);
        //integer[VIP_BONUS_SKILL] = getGlobalNumber(L, "vipBonusSkill", 0);
        //bool[VIP_AUTOLOOT_VIP_ONLY] = getGlobalBoolean(L, "vipAutoLootVipOnly", false);
        //bool[VIP_STAY_ONLINE] = getGlobalBoolean(L, "vipStayOnline", false);
        //integer[VIP_FAMILIAR_TIME_COOLDOWN_REDUCTION] = getGlobalNumber(L, "vipFamiliarTimeCooldownReduction", 0);

        //bool[REWARD_CHEST_COLLECT_ENABLED] = getGlobalBoolean(L, "rewardChestCollectEnabled", true);
        //integer[REWARD_CHEST_MAX_COLLECT_ITEMS] = getGlobalNumber(L, "rewardChestMaxCollectItems", 200);

        //// PVP System
        //float[PVP_RATE_DAMAGE_TAKEN_PER_LEVEL] = getGlobalFloat(L, "pvpRateDamageTakenPerLevel", 0.0);
        //float[PVP_RATE_DAMAGE_REDUCTION_PER_LEVEL] = getGlobalFloat(L, "pvpRateDamageReductionPerLevel", 0.0);
        //integer[PVP_MAX_LEVEL_DIFFERENCE] = getGlobalNumber(L, "pvpMaxLevelDifference", 0);

        //bool[TOGGLE_MOUNT_IN_PZ] = getGlobalBoolean(L, "toggleMountInProtectionZone", false);

        //bool[TOGGLE_HOUSE_TRANSFER_ON_SERVER_RESTART] = getGlobalBoolean(L, "togglehouseTransferOnRestart", false);

        //bool[TOGGLE_RECEIVE_REWARD] = getGlobalBoolean(L, "toggleReceiveReward", false);

        loaded = true;
        Lua.Close(L);
        return true;
    }

    //public bool Reload()
    //{
    //    //bool result = Load();
    //    //if (TransformToSHA1(GetString(StringConfig.ServerMotd)) != Game.Instance.GetMotdHash())
    //    //{
    //    //    Game.Instance.IncrementMotdNum();
    //    //}
    //    //return result;
    //}

    public string GetString(StringConfigType what)
    {
        if (what >= StringConfigType.LAST_STRING_CONFIG)
        {
            _logger.Warning($"[ConfigManager.GetString] - Accessing invalid index: {what}");
            return string.Empty;
        }
        return stringConfig[(int)what];
    }

    public int GetNumber(IntegerConfigType what)
    {
        if (what >= IntegerConfigType.LAST_INTEGER_CONFIG)
        {
            _logger.Warning($"[ConfigManager.GetNumber] - Accessing invalid index: {what}");
            return 0;
        }
        return integerConfig[(int)what];
    }

    public short GetShortNumber(IntegerConfigType what)
    {
        if (what >= IntegerConfigType.LAST_INTEGER_CONFIG)
        {
            _logger.Warning($"[ConfigManager.GetShortNumber] - Accessing invalid index: {what}");
            return 0;
        }
        return (short)integerConfig[(int)what];
    }

    public ushort GetUShortNumber(IntegerConfigType what) => (ushort)integerConfig[(int)what];

    public bool GetBoolean(BooleanConfigType what)
    {
        if (what >= BooleanConfigType.LAST_BOOLEAN_CONFIG)
        {
            _logger.Warning($"[ConfigManager.GetBoolean] - Accessing invalid index: {what}");
            return false;
        }
        return booleanConfig[(int)what];
    }

    public float GetFloat(FloatingConfigType what)
    {
        if (what >= FloatingConfigType.LAST_FLOATING_CONFIG)
        {
            _logger.Warning($"[ConfigManager.GetFloat] - Accessing invalid index: {what}");
            return 0;
        }
        return floatingConfig[(int)what];
    }

    public string SetConfigFileLua(string what)
    {
        return configFileLua = what;
    }

    public string GetConfigFileLua()
    {
        return configFileLua;
    }

    private string configFileLua = "";

    private ConfigManager()
    {
        // Implementar lógica do construtor aqui
    }

    private static readonly string DummyStr = string.Empty;

    //private string TransformToSHA1(string input)
    //{
    //    using (SHA1 sha1 = SHA1.Create())
    //    {
    //        byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
    //        StringBuilder sb = new StringBuilder();
    //        foreach (byte b in hashBytes)
    //        {
    //            sb.Append(b.ToString("x2"));
    //        }
    //        return sb.ToString();
    //    }
    //}

    public string GetGlobalString(LuaState L, string identifier, string defaultValue)
    {
        Lua.GetGlobal(L, identifier);
        if (Lua.IsString(L, -1))
        {
            return defaultValue;
        }

        ulong len = 0;
        var str = Lua.ToLString(L, -1, ref len);
        Lua.Pop(L, 1);
        return str;
    }

    public int GetGlobalNumber(LuaState L, string identifier, int defaultValue = 0)
    {
        Lua.GetGlobal(L, identifier);
        if (Lua.IsNumber(L, -1))
        {
            return defaultValue;
        }

        int val = (int)Lua.ToNumber(L, -1);
        Lua.Pop(L, 1);
        return val;
    }

    public bool GetGlobalBoolean(LuaState L, string identifier, bool defaultValue)
    {
        Lua.GetGlobal(L, identifier);
        if (Lua.IsBoolean(L, -1))
        {
            if (Lua.IsString(L, -1))
            {
                return defaultValue;
            }

            ulong len = 0;
            var str = Lua.ToLString(L, -1, ref len);
            Lua.Pop(L, 1);
            return BooleanString(str);
        }

        var val = Lua.ToBoolean(L, -1);
        Lua.Pop(L, 1);
        return val;
    }

    public float GetGlobalFloat(LuaState L, string identifier, float defaultValue = 0.0f)
    {
        Lua.GetGlobal(L, identifier);
        if (Lua.IsNumber(L, -1))
        {
            return defaultValue;
        }

        float val = (float)Lua.ToNumber(L, -1);
        Lua.Pop(L, 1);
        return val;
    }

    private bool BooleanString(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return false;
        }

        var ch = str.ToLower();
        return ch != "f" && ch != "n" && ch != "0";
    }
}
