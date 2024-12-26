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
using System.Threading.Tasks;
using Serilog;

namespace NeoServer.Loaders.Monsters;

public class MonsterLoader
{
    private readonly GameConfiguration _gameConfiguration;
    private readonly IItemTypeStore _itemTypeStore;
    private readonly ILogger _logger;
    private readonly IMonsterDataManager _monsterManager;
    private readonly ServerConfiguration _serverConfiguration;
    
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        DefaultBufferSize = 4096,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

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
            var monsters = GetMonsterDataListAsync().GetAwaiter().GetResult().ToList();
            _monsterManager.Load(monsters);
            return new object[] { monsters.Count };
        });
    }

    private async Task<IEnumerable<(string, IMonsterType)>> GetMonsterDataListAsync()
    {
        var basePath = $"{_serverConfiguration.Data}/monsters";
        await using var fileStream = new FileStream(Path.Combine(basePath, "monsters.json"), FileMode.Open, FileAccess.Read);
        var monstersPath = await JsonSerializer.DeserializeAsync<List<IDictionary<string, string>>>(fileStream, _jsonOptions);

        var tasks = monstersPath.Select(async x => (x["name"], await ConvertMonsterAsync(basePath, x)));

        return await Task.WhenAll(tasks);
    }

    private async Task<IMonsterType> ConvertMonsterAsync(string basePath, IDictionary<string, string> monsterFile)
    {
        await using var fileStream = new FileStream(Path.Combine(basePath, monsterFile["file"]), FileMode.Open, FileAccess.Read);

        var monster = await JsonSerializer.DeserializeAsync<MonsterData>(fileStream, _jsonOptions);

        return MonsterConverter.Convert(monster, _gameConfiguration, _monsterManager, _logger, _itemTypeStore);
    }
}