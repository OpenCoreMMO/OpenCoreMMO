using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Creatures.Players;
using NeoServer.Loaders.Interfaces;
using NeoServer.Server.Configurations;
using Newtonsoft.Json;
using Serilog;

namespace NeoServer.Loaders.Players.OutFits;

public class PlayerOutfitLoader : IStartupLoader
{
    private readonly ILogger _logger;
    private readonly IPlayerOutFitStore _playerOutFitStore;
    private readonly ServerConfiguration _serverConfiguration;

    public PlayerOutfitLoader(ServerConfiguration serverConfiguration, ILogger logger,
        IPlayerOutFitStore playerOutFitStore)
    {
        _serverConfiguration = serverConfiguration;
        _logger = logger;
        _playerOutFitStore = playerOutFitStore;
    }

    public void Load()
    {
        var path = $"{_serverConfiguration.Data}/player/outfits.json";

        if (!File.Exists(path))
        {
            _logger.Error("{Path} file not found", path);
            return;
        }

        var jsonContent = File.ReadAllText(path);
        var outfitsData = JsonConvert.DeserializeObject<IEnumerable<PlayerOutFitData>>(jsonContent).ToList();

        _playerOutFitStore.AddOrUpdate(Gender.Female, outfitsData.Where(item => item.Type == Gender.Female));
        _playerOutFitStore.AddOrUpdate(Gender.Male, outfitsData.Where(item => item.Type == Gender.Male));
    }
}