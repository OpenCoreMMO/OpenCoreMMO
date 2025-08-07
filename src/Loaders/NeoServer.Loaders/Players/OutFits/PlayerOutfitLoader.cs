using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Creatures.Player;
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
        _logger.Information("Starting PlayerOutfitLoader - Loading outfits from configuration");
        
        var path = $"{_serverConfiguration.Data}/player/outfits.json";
        
        _logger.Information("Looking for outfit file at: {Path}", path);

        if (!File.Exists(path))
        {
            _logger.Error("Outfit file not found at: {Path}", path);
            return;
        }

        _logger.Information("Reading outfit file: {Path}", path);
        var jsonContent = File.ReadAllText(path);
        var outfitsData = JsonConvert.DeserializeObject<IEnumerable<PlayerOutFitData>>(jsonContent).ToList();

        if (outfitsData == null || !outfitsData.Any())
        {
            _logger.Warning("No outfit data found or failed to deserialize from file: {Path}", path);
            return;
        }

        _logger.Information("Found {OutfitCount} outfits in configuration", outfitsData.Count);

        var femaleOutfits = outfitsData.Where(item => item.Type == Gender.Female).ToList();
        var maleOutfits = outfitsData.Where(item => item.Type == Gender.Male).ToList();

        _logger.Information("Adding {FemaleCount} female outfits and {MaleCount} male outfits", 
            femaleOutfits.Count, maleOutfits.Count);

        _playerOutFitStore.AddOrUpdate(Gender.Female, femaleOutfits);
        _playerOutFitStore.AddOrUpdate(Gender.Male, maleOutfits);
        
        _logger.Information("PlayerOutfitLoader completed successfully");
    }
}