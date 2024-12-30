using System;
using System.IO;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Services;
using NeoServer.Scripts.Lua.EventRegister;
using NeoServer.Scripts.Lua.Functions;
using NeoServer.Scripts.Lua.Functions.Libs;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers.Extensions;
using NeoServer.Server.Services;
using NLua;
using Serilog;

namespace NeoServer.Scripts.Lua;

public class LuaGlobalRegister
{
    private readonly ICoinTransaction _coinTransaction;
    private readonly ICreatureFactory _creatureFactory;
    private readonly IDecayableItemManager _decayableItemManager;
    private readonly IGameServer _gameServer;
    private readonly IItemFactory _itemFactory;
    private readonly IItemService _itemService;
    private readonly ILogger _logger;
    private readonly NLua.Lua _lua;
    private readonly ServerConfiguration _serverConfiguration;

    public LuaGlobalRegister(IGameServer gameServer, IItemFactory itemFactory, ICreatureFactory creatureFactory,
        NLua.Lua lua, ServerConfiguration serverConfiguration, ILogger logger, ICoinTransaction coinTransaction,
        IDecayableItemManager decayableItemManager, IItemService itemService)
    {
        _gameServer = gameServer;
        _itemFactory = itemFactory;
        _creatureFactory = creatureFactory;
        _lua = lua;
        _serverConfiguration = serverConfiguration;
        _logger = logger;
        _coinTransaction = coinTransaction;
        _decayableItemManager = decayableItemManager;
        _itemService = itemService;
    }

    public void Register()
    {
        _logger.Step("Loading lua scripts...", "{Lua} scripts loaded", () =>
        {
            _lua.LoadCLRPackage();

            _lua["GameServer"] = _gameServer;
            _lua["sendNotification"] = NotificationSenderService.Send;
            _lua["sendOperationFail"] = (IPlayer player, string message) => OperationFailService.Send(player, message);
            _lua["Scheduler"] = _gameServer.Scheduler;
            _lua["Map"] = _gameServer.Map;
            _lua["ItemFactory"] = _itemFactory;
            _lua["CreatureFactory"] = _creatureFactory;
            _lua["dofile"] = new Action<string>(DoFile);
            _lua["loadfile"] = LoadFile;
            _lua["Logger"] = _logger;
            _lua["coinTransaction"] = _coinTransaction;
            _lua["Random"] = GameRandom.Random;
            _lua["DecayableManager"] = _decayableItemManager;
            _lua["register"] = LuaEventManager.Register;
            _lua["ItemService"] = _itemService;

            _lua.AddQuestFunctions();
            _lua.AddPlayerFunctions();
            _lua.AddItemFunctions();
            _lua.AddTileFunctions();
            _lua.AddLibs();

            _lua["make_array"] = (string typeName, LuaTable x) =>
            {
                if (typeName == "ushort")
                {
                    var values = new ushort[x.Values.Count];
                    var i = 0;
                    foreach (var key in x.Values) values[i++] = Convert.ToUInt16(key);

                    return values;
                }

                throw new Exception("Type not found");
            };

            ExecuteMainFiles();
            QuestFunctions.RegisterQuests(_lua);

            LuaBind.Setup();
            return new object[] { "LUA" };
        });
    }

    private void ExecuteMainFiles()
    {
        var dataPath = _serverConfiguration.Data;

        foreach (var file in Directory.GetFiles(dataPath, "main.lua", new EnumerationOptions
                 {
                     AttributesToSkip = FileAttributes.Temporary,
                     IgnoreInaccessible = true,
                     RecurseSubdirectories = true
                 }))
            _lua.DoFile(file);
    }

    private void DoFile(string luaPath)
    {
        _lua.DoFile(Path.Combine(_serverConfiguration.Data, luaPath));
    }

    private void LoadFile(string luaPath)
    {
        _lua.LoadFile(Path.Combine(_serverConfiguration.Data, luaPath));
    }
}