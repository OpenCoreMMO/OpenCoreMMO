using System.Linq;
using System.Threading.Tasks;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Repositories;
using NeoServer.Loaders.Interfaces;
using Serilog;

namespace NeoServer.Loaders.Houses;

public class HouseLoader : ICustomLoader
{
    private readonly IHouseRepository _houseRepository;
    private readonly IHouseStore _houseStore;
    private readonly HouseAccessListLoader _accessListLoader;
    private readonly ILogger _logger;

    public HouseLoader(
        IHouseRepository houseRepository,
        IHouseStore houseStore,
        HouseAccessListLoader accessListLoader,
        ILogger logger)
    {
        _houseRepository = houseRepository;
        _houseStore = houseStore;
        _accessListLoader = accessListLoader;
        _logger = logger;
    }

    public async Task Load()
    {
        var houses = (await _houseRepository.GetAll()).ToArray();
        var accessListData = await _houseRepository.GetAllAccessListText();

        foreach (var house in houses)
        {
            _houseStore.AddOrUpdate(house.Id, house);

            if (accessListData.TryGetValue(house.Id, out var lists))
                await _accessListLoader.Load(house, lists);
        }

        _logger.Information("{Count} houses loaded", houses.Length);
    }
}
