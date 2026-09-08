using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Houses;

namespace NeoServer.Domain.Repositories;

public interface IHouseRepository
{
    void Save(House house);
    Task SaveTilesAsync(House house);
    Task SaveTilesAsync(List<House> houses);
    void SaveAccessList(uint houseId, uint listId, string text);
    Task<IEnumerable<House>> GetAll();
    Task<House> GetById(uint id);
    Task<IReadOnlyDictionary<uint, List<(uint ListId, string ListText)>>> GetAllAccessListText();
    Task<IReadOnlyDictionary<uint, List<(Location Location, List<IItem> Items)>>> GetAllTileDataAsync();
}
