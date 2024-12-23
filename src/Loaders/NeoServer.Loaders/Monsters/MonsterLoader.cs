using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers.Extensions;
using System.Text.Json;
using Serilog;

namespace NeoServer.Loaders.Monsters;

public class MonsterLoader
{
    private readonly GameConfiguration _gameConfiguration;
    private readonly IItemTypeStore _itemTypeStore;
    private readonly ILogger _logger;
    private readonly IMonsterDataManager _monsterManager;
    private readonly ServerConfiguration _serverConfiguration;

    public MonsterLoader(IMonsterDataManager monsterManager, GameConfiguration gameConfiguration, ILogger logger,
        ServerConfiguration serverConfiguration, IItemTypeStore itemTypeStore)
    {
        _monsterManager = monsterManager;
        _gameConfiguration = gameConfiguration;
        _logger = logger;
        _serverConfiguration = serverConfiguration;
        _itemTypeStore = itemTypeStore;
    }

    public void Load()
    {
        _logger.Step("Loading monsters...", "{n} monsters loaded", () =>
        {
            var monsters = GetMonsterDataList().ToList();
            _monsterManager.Load(monsters);
            return new object[] { monsters.Count };
        });
    }

    private IEnumerable<(string, IMonsterType)> GetMonsterDataList()
    {
        var basePath = $"{_serverConfiguration.Data}/monsters";
        var jsonString = File.ReadAllText(Path.Combine(basePath, "monsters.json"));
        var monstersPath = JsonSerializer.Deserialize<List<IDictionary<string, string>>>(jsonString);

        return monstersPath.AsParallel().Select(x => (x["name"], ConvertMonster(basePath, x)));
    }

    private IMonsterType ConvertMonster(string basePath, IDictionary<string, string> monsterFile)
    {
        var json = File.ReadAllText(Path.Combine(basePath, monsterFile["file"]));

        var monster = JsonSerializer.Deserialize<MonsterData>(json, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        });
            
        return MonsterConverter.Convert(monster, _gameConfiguration, _monsterManager, _logger, _itemTypeStore);
    }
}