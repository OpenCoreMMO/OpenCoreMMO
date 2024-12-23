using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Services;
using NeoServer.Loaders.Interfaces;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers.Extensions;
using Serilog;

namespace NeoServer.Loaders.TileRule;

public class TileRuleLoader : IStartupLoader
{
    private readonly ILogger _logger;
    private readonly IMap _map;
    private readonly ServerConfiguration _serverConfiguration;

    public TileRuleLoader(ILogger logger, ServerConfiguration serverConfiguration, IMap map)
    {
        _logger = logger;
        _serverConfiguration = serverConfiguration;
        _map = map;
    }

    public void Load()
    {
        _logger.Step("Loading tile rules...", "{n} tile rules loaded", () =>
        {
            var count = LoadTileRules();

            return new object[] { count };
        });
    }

    private int LoadTileRules()
    {
        var file = $"{_serverConfiguration.Data}/tiles.json";

        var jsonContent = File.ReadAllText(file);
        try
        {
            var tilesData = JsonSerializer.Deserialize<List<TileJsonData>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
                
            if (tilesData is null) return 0;

            foreach (var tileRule in tilesData)
            foreach (var location in tileRule.Locations)
            {
                tileRule.MaxLevel = tileRule.MaxLevel == 0 ? ushort.MaxValue : tileRule.MaxLevel;

                var tileLocation = new Location(location[0], location[1], (byte)location[2]);

                if (_map[tileLocation] is not IDynamicTile dynamicTile) continue;

                dynamicTile.CanEnter = creature => CanEnter(creature, tileRule);
            }

            return tilesData.Count;
        }
        catch (Exception e)
        { 
            Console.WriteLine(e);
        }

        return 0;
    }

    private static bool CanEnter(ICreature creature, TileJsonData tileRule)
    {
        if (creature is not IPlayer player) return true;

        if (player.Level >= tileRule.MinLevel &&
            player.Level <= tileRule.MaxLevel &&
            (!tileRule.RequiresPremium || (tileRule.RequiresPremium && player.PremiumTime > 0))) return true;

        if (string.IsNullOrWhiteSpace(tileRule.Message)) return false;

        NotificationSenderService.Send(player, tileRule.Message);
        return false;
    }
}