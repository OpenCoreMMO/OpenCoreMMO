using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServer.Domain.Houses;

namespace NeoServer.Domain.Repositories;

public interface IHouseRepository
{
    void Save(House house);
    void SaveAccessList(uint houseId, uint listId, string text);
    Task<IEnumerable<House>> GetAll();
    Task<House> GetById(uint id);
    Task<IReadOnlyDictionary<uint, List<(uint ListId, string ListText)>>> GetAllAccessListText();
}
