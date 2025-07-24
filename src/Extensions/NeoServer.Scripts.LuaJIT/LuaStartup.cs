using LuaNET;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Configurations;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class LuaStartup(
    ILogger logger,
    ILuaEnvironment luaEnviroment,
    IConfigManager configManager,
    IScripts scripts,
    IActionFunctions actionFunctions,
    IConfigFunctions configFunctions,
    IContainerFunctions containerFunctions,
    ICreatureFunctions creatureFunctions,
    ICreatureEventFunctions creatureEventFunctions,
    IDBFunctions dbFunctions,
    IEnumFunctions enumFunctions,
    IGameFunctions gameFunctions,
    IGlobalFunctions globalFunctions,
    IGlobalEventFunctions globalEventFunctions,
    IGroupFunctions groupFunctions,
    IItemFunctions itemFunctions,
    IItemTypeFunctions itemTypeFunctions,
    ILoggerFunctions loggerFunctions,
    IMonsterFunctions monsterFunctions,
    IMonsterTypeFunctions monsterTypeFunctions,
    IMoveEventFunctions moveEventFunctions,
    INpcFunctions npcFunctions,
    INpcTypeFunctions npcTypeFunctions,
    IPlayerFunctions playerFunctions,
    IPositionFunctions positionFunctions,
    IResultFunctions resultFunctions,
    ITalkActionFunctions talkActionFunctions,
    ITeleportFunctions teleportFunctions,
    ITileFunctions tileFunctions,
    ServerConfiguration serverConfiguration,
    IConditionFunctions conditionFunctions,
    IBankFunctions bankFunctions,
    ISpellFunctions spellFunctions,
    ICombatFunctions combatFunctions,
    IVariantFunctions variantFunctions,
    IEventCallbackFunctions eventCallbackFunctions
) : ILuaStartup
{
    #region Public Methods

    public void Start()
    {
        var currentDir = AppContext.BaseDirectory;

        if (!string.IsNullOrEmpty(ArgManager.GetInstance().ExePath))
            currentDir = ArgManager.GetInstance().ExePath;

        ModulesLoadHelper(luaEnviroment.InitState(), "luaEnvironment");

        var luaState = luaEnviroment.GetLuaState();

        if (luaState.IsNull)
            logger.Error("Invalid lua state, cannot load lua Functions");

        Lua.OpenLibs(luaState);

        actionFunctions.Init(luaState);
        conditionFunctions.Init(luaState);
        configFunctions.Init(luaState);
        creatureFunctions.Init(luaState);
        creatureEventFunctions.Init(luaState);
        dbFunctions.Init(luaState);
        enumFunctions.Init(luaState);
        gameFunctions.Init(luaState);
        globalFunctions.Init(luaState);
        globalEventFunctions.Init(luaState);
        itemFunctions.Init(luaState);
        itemTypeFunctions.Init(luaState);
        loggerFunctions.Init(luaState);
        positionFunctions.Init(luaState);
        resultFunctions.Init(luaState);
        talkActionFunctions.Init(luaState);
        tileFunctions.Init(luaState);

        containerFunctions.Init(luaState);
        monsterFunctions.Init(luaState);
        monsterTypeFunctions.Init(luaState);
        moveEventFunctions.Init(luaState);
        npcFunctions.Init(luaState);
        npcTypeFunctions.Init(luaState);
        playerFunctions.Init(luaState);
        teleportFunctions.Init(luaState);
        groupFunctions.Init(luaState);
        spellFunctions.Init(luaState);
        combatFunctions.Init(luaState);
        bankFunctions.Init(luaState);
        variantFunctions.Init(luaState);
        eventCallbackFunctions.Init(luaState);

        ModulesLoadHelper(configManager.Load($"{currentDir}/config.lua"), "config.lua");

        ModulesLoadHelper(luaEnviroment.LoadFile($"{serverConfiguration.Data}/core.lua", "core.lua"),
            "/Data/core.lua");

        ModulesLoadHelper(scripts.LoadScripts($"{serverConfiguration.Data}/scripts/libs", true, false),
            "/Data/scripts/libs");
        ModulesLoadHelper(scripts.LoadScripts($"{serverConfiguration.Data}/scripts", false, false), "/Data/scripts");
        ModulesLoadHelper(luaEnviroment.LoadFile($"{serverConfiguration.Data}/npclib/load.lua", "load.lua"),
            "/Data/npclib");

        ModulesLoadHelper(scripts.LoadScripts($"{serverConfiguration.Data}/npcs", false, false), "/Data/npcs");
    }

    #endregion

    #region Private Methods

    private void ModulesLoadHelper(bool loaded, string moduleName)
    {
        logger.Information("Loaded {ModuleName}", moduleName);
        if (!loaded)
            logger.Error("Cannot load: {ModuleName}", moduleName);
    }

    #endregion
}