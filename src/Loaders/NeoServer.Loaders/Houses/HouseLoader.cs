using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Repositories;
using NeoServer.Loaders.Interfaces;
using NeoServer.Loaders.World;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers.Extensions;
using Serilog;

namespace NeoServer.Loaders.Houses;

public class HouseLoader(
    IHouseRepository houseRepository,
    IHouseStore houseStore,
    IHouseFactory houseFactory,
    HouseAccessListLoader accessListLoader,
    HouseXmlParser houseXmlParser,
    ServerConfiguration serverConfiguration,
    ILogger logger,
    WorldLoader worldLoader) : ICustomLoader
{
    public async Task Load()
    {
        var basePath = $"{serverConfiguration.Data}/world/";
        var xmlFilePath = Path.Combine(basePath,
            serverConfiguration.OTBM.Replace(".otbm", "-house.xml"));

        var xmlData = houseXmlParser.Parse(xmlFilePath);
        if (xmlData.Count == 0)
        {
            logger.Step("Loading houses...", "{n} houses loaded", () => [0]);
            return;
        }

        // Fetch DB house data (both queries in parallel)
        var getAllTask = houseRepository.GetAll();
        var getAccessListTask = houseRepository.GetAllAccessListText();
        var tilesDataTask = houseRepository.GetAllTileDataAsync();
        await Task.WhenAll(getAllTask, getAccessListTask);

        var dbHouses = (await getAllTask).ToArray();
        var accessListData = await getAccessListTask;
        var dbHouseMap = dbHouses.Length > 0
            ? dbHouses.ToDictionary(house => house.Id)
            : new Dictionary<uint, House>();

        int count;
        logger.Step("Loading houses...", "{n} houses loaded", () =>
        {
            var houses = new List<House>(xmlData.Count);
            foreach (var data in xmlData)
            {
                var house = houseFactory.Create(data.Id, data.Name, data.TownId, data.Rent,
                    0, string.Empty, 0, null, 0);

                if (data.EntryX != 0 || data.EntryY != 0 || data.EntryZ != 0)
                {
                    house.EntryPosition = new Location(data.EntryX, data.EntryY, data.EntryZ);
                }

                // Enrich with DB runtime data
                if (dbHouseMap.TryGetValue(house.Id, out var dbHouse))
                {
                    house.OwnerGuid = dbHouse.OwnerGuid;
                    house.OwnerName = dbHouse.OwnerGuid != 0 ? dbHouse.OwnerName : string.Empty;
                    house.OwnerAccountId = dbHouse.OwnerAccountId;
                    house.PaidUntil = dbHouse.PaidUntil;
                    house.PayRentWarnings = dbHouse.PayRentWarnings;

                    if (accessListData.TryGetValue(dbHouse.Id, out var lists))
                    {
                        accessListLoader.Load(house, lists).GetAwaiter().GetResult();
                    }
                }
                
                houseRepository.Save(house);
                houseStore.AddOrUpdate(house.Id, house);
                houses.Add(house);
            }
            
            // Link tiles collected during world loading to their houses
            LinkHouseTiles();

            // Load persisted house tile items from DB and place them on linked tiles
            var tileData = tilesDataTask.GetAwaiter().GetResult();
            if (tileData.Count > 0)
            {
                logger.Information("Loading {HouseCount} house tile data...", tileData.Count);

                AddItemsToTile(tileData);
            }

            count = houses.Count;
            return [count];
        });

       
    }

    private void AddItemsToTile(IReadOnlyDictionary<uint, List<(Location Location, List<IItem> Items)>> tileData)
    {
        foreach (var (houseId, tileItems) in tileData)
        {
            var house = houseStore.GetByHouseId(houseId);
            if (house is null)
            {
                logger.Warning("Orphan house tile data: house id {HouseId} not found in store", houseId);
                continue;
            }

            foreach (var (location, items) in tileItems)
            {
                foreach (var tile in house.Tiles)
                {
                    if (tile.Location != location) continue;

                    // Iterate in reverse so items are added bottom-first,
                    // restoring the original stack order. Items are serialized
                    // top-to-bottom (from AllItems reversed enumerator), so
                    // adding them in reverse preserves the original bottom-to-top layout.
                    for (var i = items.Count - 1; i >= 0; i--)
                    {
                        tile.AddItem(items[i]);
                    }

                    break;
                }
            }
        }
    }

    private void LinkHouseTiles()
    {
        foreach (var (houseId, tiles) in worldLoader.HouseTiles)
        {
            var house = houseStore.GetByHouseId(houseId);
            if (house is null)
            {
                logger.Warning("Orphan house tile at house id {HouseId}: house not found in store", houseId);
                continue;
            }

            foreach (var tile in tiles)
            {
                try
                {
                    house.LinkTile(tile);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Failed to link tile at {Coordinate} to house {HouseId}",
                        tile.Location, houseId);
                    continue;
                }

                // Link doors and beds
                foreach (var item in tile.AllItems)
                {
                    if (item is null) continue;

                    if (item.Metadata.Attributes.GetAttribute(ItemTypeAttribute.Type) == "door")
                    {
                        if (item.Attributes is not null && TryGetDoorId(item, out var doorId))
                        {
                            house.LinkDoor(doorId, item);
                        }
                    }

                    if (item.Metadata.HasFlag(ItemFlag.Bed))
                    {
                        house.LinkBed(item);
                    }
                }
            }
        }
    }

    private static bool TryGetDoorId(IItem item, out uint doorId)
    {
        doorId = 0;
        if (item.Attributes is null)
        {
            return false;
        }

        if (item.Attributes.TryGetAttribute<byte>(ItemAttribute.DoorId, out var doorIdByte))
        {
            doorId = doorIdByte;
            return true;
        }

        if (item.Attributes.TryGetAttribute(ItemAttribute.DoorId, out string doorIdStr) &&
            uint.TryParse(doorIdStr, out doorId))
        {
            return true;
        }

        return false;
    }
}
