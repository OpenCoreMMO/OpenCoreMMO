using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers.Extensions;
using Serilog;

namespace NeoServer.Loaders.Monsters;

public class MonsterLoader(
    IMonsterDataManager monsterManager,
    ILogger logger,
    ServerConfiguration serverConfiguration,
    MonsterConverter monsterConverter)
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultBufferSize = 4096,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public void Load()
    {
        logger.Step("Loading monsters...", "{n} monsters loaded", () =>
        {
            var monsters = GetMonsterDataListAsync().GetAwaiter().GetResult().ToList();
            monsterManager.Load(monsters);
            return [monsters.Count];
        });
    }

    private async Task<IEnumerable<(string, IMonsterType)>> GetMonsterDataListAsync()
    {
        var basePath = $"{serverConfiguration.Data}/monsters";
        
        await using var fileStream =
            new FileStream(Path.Combine(basePath, "monsters.json"), FileMode.Open, FileAccess.Read);
        
        var monstersPath =
            await JsonSerializer.DeserializeAsync<List<IDictionary<string, string>>>(fileStream, _jsonOptions);

        var tasks = monstersPath
            .OrderBy(x => x["name"])
            .Select(async x => (x["name"], await ConvertMonsterAsync(basePath, x)));

        return await Task.WhenAll(tasks);
    }

    private async Task<IMonsterType> ConvertMonsterAsync(string basePath, IDictionary<string, string> monsterFile)
    {
        await using var fileStream =
            new FileStream(Path.Combine(basePath, monsterFile["file"]), FileMode.Open, FileAccess.Read);

        var monster = await JsonSerializer.DeserializeAsync<MonsterData>(fileStream, _jsonOptions);

        return monsterConverter.Convert(monster);
    }
}