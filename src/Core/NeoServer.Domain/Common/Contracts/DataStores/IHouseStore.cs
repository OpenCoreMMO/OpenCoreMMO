using System.Collections.Generic;
using System.Threading.Tasks;
using NeoServer.Domain.Core.Houses;

namespace NeoServer.Domain.Common.Contracts.DataStores;

/// <summary>
/// Interface for house data operations
/// </summary>
public interface IHouseStore : IDataStore<uint, House>
{
    Task<bool> SaveHouseAsync(House house);
    Task<List<House>> GetAllHousesAsync();
    Task<House> GetHouseByIdAsync(uint houseId);
    Task<List<House>> GetHousesByOwnerAsync(uint ownerId);
    Task<bool> UpdateHouseOwnershipAsync(uint houseId, uint ownerId, long paidUntil);
    Task<bool> DeleteHouseAsync(uint houseId);
    bool SaveHouse(House house);
    List<House> GetAllHouses();
    House GetHouseById(uint houseId);
    List<House> GetHousesByOwner(uint ownerId);
    bool UpdateHouseOwnership(uint houseId, uint ownerId, long paidUntil);
    bool DeleteHouse(uint houseId);
}
