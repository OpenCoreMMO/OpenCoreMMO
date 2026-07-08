using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Repositories;
using NeoServer.Loaders.Interfaces;
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
    ILogger logger) : ICustomLoader
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

                houseStore.AddOrUpdate(house.Id, house);
                houses.Add(house);
            }

            count = houses.Count;
            return [count];
        });
    }
}
