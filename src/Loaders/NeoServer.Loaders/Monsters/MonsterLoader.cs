using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Loaders.Helpers;
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
    public void Load()
    {
        logger.Step("Loading monsters...", "{n} monsters loaded", () =>
        {
            var monsters = GetMonsterDataListAsync().GetAwaiter().GetResult();

            foreach (var monster in monsters)
            {
                monsterTypeStore.AddOrUpdate(monster.Name, monster);
            }

            return [monsters.Length];
        });
    }

    private async Task<IMonsterType[]> GetMonsterDataListAsync()
    {
        var basePath = $"{serverConfiguration.Data}/monsters";

        await using var fileStream =
            new FileStream(Path.Combine(basePath, "monsters.json"), FileMode.Open, FileAccess.Read);

        var monstersPath =
            await JsonSerializer.DeserializeAsync<List<MonstersFile>>(fileStream, JsonSettings.Options);

        var tasks = new List<Task<IMonsterType>>();
        
        foreach (var monster in monstersPath)
        {
            tasks.Add(ConvertMonsterAsync(basePath, monster.File));
        }
    
        return await Task.WhenAll(tasks);
    }

    private Task<IMonsterType> ConvertMonsterAsync(string basePath, string monsterFile)
    {
        return Task.Run(() =>
        {
            using var fileStream = File.OpenRead(Path.Combine(basePath, monsterFile));
            var monster = JsonSerializer.Deserialize<MonsterData>(fileStream, JsonSettings.Options);
            return monsterConverter.Convert(monster);
        });
    }
}

public record MonstersFile(string Name, string File);