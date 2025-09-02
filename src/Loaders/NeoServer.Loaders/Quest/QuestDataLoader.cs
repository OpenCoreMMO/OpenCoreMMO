using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Loaders.Helpers;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers.Extensions;
using Serilog;

namespace NeoServer.Loaders.Quest;

public class QuestDataLoader
{
    private readonly ILogger _logger;
    private readonly IQuestDataStore _questDataStore;
    private readonly ServerConfiguration _serverConfiguration;

    public QuestDataLoader(ILogger logger,
        ServerConfiguration serverConfiguration, IQuestDataStore questDataStore)
    {
        _logger = logger;
        _serverConfiguration = serverConfiguration;
        _questDataStore = questDataStore;
    }

    public void Load()
    {
        _logger.Step("Loading quests...", "{n} quests loaded", () =>
        {
            _questDataStore.Clear();
            var actions = GetQuests();
            actions.ForEach(x => _questDataStore.AddOrUpdate(x.Id, x));

            return new object[] { actions.Count };
        });
    }

    private List<Domain.Quest.Quest> GetQuests()
    {
        var basePath = $"{_serverConfiguration.Data}";
        var jsonString = File.ReadAllText(Path.Combine(basePath, "quests.json"));
        var quests = JsonSerializer.Deserialize<List<QuestModel>>(jsonString, JsonSettings.Options);

        return quests?.Select(x => new Domain.Quest.Quest
        {
            Name = x.Name,
            StartId = x.StartId,
            StartValue = x.StartValue,
            Missions = x.Missions?.Select(m => new Domain.Quest.Mission
            {
                Id = m.Id,
                Name = m.Name,
                StartValue = m.StartValue,
                EndValue = m.EndValue,
                IgnoreEndValue = m.IgnoreEndValue,
                Description = m.Description,
                States = m.States?.Select(s => new Domain.Quest.MissionState
                {
                    Id = s.Id,
                    Description = s.Description
                }).ToList()
            }).ToList()
        }).ToList();
    }
}