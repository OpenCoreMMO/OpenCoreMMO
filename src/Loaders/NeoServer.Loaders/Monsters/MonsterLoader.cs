using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers.Extensions;
using Serilog;

namespace NeoServer.Loaders.Monsters;

public class MonsterLoader(
    IMonsterTypeStore monsterTypeStore,
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

            foreach (var monster in monsters)
            {
                monsterTypeStore.AddOrUpdate(monster.Name, monster);
            }

            return [monsters.Count];
        });
    }

    private async Task<IEnumerable<IMonsterType>> GetMonsterDataListAsync()
    {
        var basePath = $"{serverConfiguration.Data}/monsters";

        await using var fileStream =
            new FileStream(Path.Combine(basePath, "monsters.json"), FileMode.Open, FileAccess.Read);

        var monstersPath =
            await JsonSerializer.DeserializeAsync<List<MonstersFile>>(fileStream, _jsonOptions);

        var monsters = monstersPath;
        
        var tasks = new List<Task<IMonsterType>>();
        
        foreach (var monster in monsters)
        {
            tasks.Add(ConvertMonsterAsync(basePath, monster.File));
        }
    
        return await Task.WhenAll(tasks);
    }

    private async Task<IMonsterType> ConvertMonsterAsync(string basePath, string monsterFile)
    {
        await using var fileStream = new FileStream(Path.Combine(basePath, monsterFile), FileMode.Open, FileAccess.Read);

        var monster = await JsonSerializer.DeserializeAsync<MonsterData>(fileStream, _jsonOptions);

        return monsterConverter.Convert(monster);
    }
}

public record MonstersFile(string Name, string File);