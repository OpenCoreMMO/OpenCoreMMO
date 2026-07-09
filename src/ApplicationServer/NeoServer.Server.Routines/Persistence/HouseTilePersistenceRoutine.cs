using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Repositories;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Configurations;
using Serilog;

namespace NeoServer.Server.Routines.Persistence;

public class HouseTilePersistenceRoutine(
    IGameServer gameServer,
    IHouseStore houseStore,
    IHouseRepository houseRepository,
    ILogger logger,
    ServerConfiguration serverConfiguration)
{
    private readonly Stopwatch _stopwatch = new();
    private int _saveInterval;

    public void Start(CancellationToken token)
    {
        _saveInterval = (int)(serverConfiguration?.Save?.HouseTiles ?? 60);
        _saveInterval = (_saveInterval == 0 ? 60 : _saveInterval) * (int)TimeSpan.MillisecondsPerSecond;

        Task.Factory.StartNew(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(_saveInterval, token);
                gameServer.PersistenceDispatcher.AddEvent(async () => await SaveHouseTiles());
            }
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    public async Task SaveHouseTiles()
    {
        try
        {
            var houses = houseStore.All?.ToList();

            if (houses is null || houses.Count == 0) return;

            // Filter out houses with no linked tiles — prevents deleting persisted
            // house tile data when a house's tiles failed to link at boot.
            var linkedHouses = new List<House>(houses.Count);
            foreach (var house in houses)
            {
                if (house.TileCount > 0)
                {
                    linkedHouses.Add(house);
                    continue;
                }
                
                logger.Warning("House {HouseId} ({HouseName}) has no linked tiles", house.Id, house.Name);
            }

            if (linkedHouses.Count == 0)
            {
                logger.Information("No houses with linked tiles to save");
                return;
            }
            
            logger.Information("Saving tile data for {Count} houses...", linkedHouses.Count);
            _stopwatch.Restart();

            await houseRepository.SaveTilesAsync(linkedHouses);

            logger.Information("Tile data for {Count} houses saved in {Elapsed} ms", linkedHouses.Count, _stopwatch.ElapsedMilliseconds);
        }
        catch (Exception e)
        {
            logger.Error(e, "Error saving house tiles");
            throw;
        }
    }
}