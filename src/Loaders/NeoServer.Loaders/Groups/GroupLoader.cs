using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Creatures.Group;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers.Extensions;
using Newtonsoft.Json;
using Serilog;

namespace NeoServer.Loaders.Groups;

public class GroupLoader
{
    public static GroupLoader Instance;

    private readonly ILogger _logger;
    private readonly ServerConfiguration _serverConfiguration;
    private readonly IGroupStore _groupStore;

    public GroupLoader
        (ILogger logger,
        ServerConfiguration serverConfiguration,
        IGroupStore groupStore)
    {
        _logger = logger;
        _serverConfiguration = serverConfiguration;
        _groupStore = groupStore;
        Instance = this;
    }

    public void Load()
    {
        _logger.Step("Loading groups...", "{n} groups loaded", () =>
        {
            _groupStore.Clear();
            var groups = GetGroups();
            foreach (var group in groups) _groupStore.Add(group.Id, group);

            return new object[] { groups.Count };
        });
    }

    public void Reload()
    {
        _logger.Step("Reloading groups...", "{n} groups reloaded", () =>
        {
            var groups = GetGroups();
            AddOrUpdateGroup(groups);
            return new object[] { groups.Count };
        });
    }

    private void AddOrUpdateGroup(List<Group> groups)
    {
        foreach (var group in groups)
        {
            if (_groupStore.TryGetValue(group.Id, out var existingGroup))
            {
                existingGroup = group;
                continue;
            }

            _groupStore.Add(group.Id, group);
        }
    }

    public List<Group> GetGroups()
    {
        var basePath = $"{_serverConfiguration.Data}";
        var jsonString = File.ReadAllText(Path.Combine(basePath, "groups.json"));
        var groups = JsonConvert.DeserializeObject<List<Group>>(jsonString, new JsonSerializerSettings
        {
            Converters =
            {
                new FlagConverter()
            }
        });
        return groups;
    }

    private class FlagConverter : JsonConverter<Dictionary<PlayerFlag, bool>>
    {
        public override Dictionary<PlayerFlag, bool> ReadJson(JsonReader reader, Type objectType,
            [AllowNull] Dictionary<PlayerFlag, bool> existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            return serializer.Deserialize<List<Dictionary<string, string>>>(reader)?.ToDictionary(
                x => ParsePlayerFlagName(x["name"]),
                x => int.Parse(x["value"], CultureInfo.InvariantCulture.NumberFormat) > 0);
        }

        private static PlayerFlag ParsePlayerFlagName(string flagName)
        {
            if (Enum.TryParse(typeof(PlayerFlag), flagName, true, out var result))
                return (PlayerFlag)result;

            throw new ArgumentOutOfRangeException(nameof(flagName), flagName, null);
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] Dictionary<PlayerFlag, bool> value,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}