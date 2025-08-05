using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Core.Houses;
using Serilog;

namespace NeoServer.Data.DataStores;

/// <summary>
/// Data store implementation for houses
/// </summary>
public class HouseStore : IHouseStore
{
    private readonly NeoContext _context;
    private readonly ILogger _logger;

    public HouseStore(NeoContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> SaveHouseAsync(House house)
    {
        try
        {
            var entity = await _context.Houses.FirstOrDefaultAsync(h => h.Id == house.Id);
            
            if (entity == null)
            {
                entity = new HouseEntity();
                _context.Houses.Add(entity);
            }

            MapHouseToEntity(house, entity);
            await _context.SaveChangesAsync();
            
            _logger.Debug("[HouseStore] Saved house {HouseId} to database", house.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[HouseStore] Error saving house {HouseId}", house.Id);
            return false;
        }
    }

    public async Task<List<House>> GetAllHousesAsync()
    {
        try
        {
            var entities = await _context.Houses.Include(h => h.HouseLists).ToListAsync();
            return entities.Select(MapEntityToHouse).ToList();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[HouseStore] Error getting all houses");
            return new List<House>();
        }
    }

    public async Task<House> GetHouseByIdAsync(uint houseId)
    {
        try
        {
            var entity = await _context.Houses
                .Include(h => h.HouseLists)
                .FirstOrDefaultAsync(h => h.Id == houseId);
            
            return entity != null ? MapEntityToHouse(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[HouseStore] Error getting house {HouseId}", houseId);
            return null;
        }
    }

    public async Task<List<House>> GetHousesByOwnerAsync(uint ownerId)
    {
        try
        {
            var entities = await _context.Houses
                .Include(h => h.HouseLists)
                .Where(h => h.Owner == ownerId)
                .ToListAsync();
            
            return entities.Select(MapEntityToHouse).ToList();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[HouseStore] Error getting houses for owner {OwnerId}", ownerId);
            return new List<House>();
        }
    }

    public async Task<bool> UpdateHouseOwnershipAsync(uint houseId, uint ownerId, long paidUntil)
    {
        try
        {
            var entity = await _context.Houses.FirstOrDefaultAsync(h => h.Id == houseId);
            if (entity == null) return false;

            entity.Owner = (int)ownerId;
            entity.Paid = (int)paidUntil;
            
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[HouseStore] Error updating house ownership {HouseId}", houseId);
            return false;
        }
    }

    public async Task<bool> DeleteHouseAsync(uint houseId)
    {
        try
        {
            var entity = await _context.Houses.FirstOrDefaultAsync(h => h.Id == houseId);
            if (entity == null) return false;

            _context.Houses.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[HouseStore] Error deleting house {HouseId}", houseId);
            return false;
        }
    }

    // Synchronous versions
    public bool SaveHouse(House house)
    {
        return SaveHouseAsync(house).GetAwaiter().GetResult();
    }

    public List<House> GetAllHouses()
    {
        return GetAllHousesAsync().GetAwaiter().GetResult();
    }

    public House GetHouseById(uint houseId)
    {
        return GetHouseByIdAsync(houseId).GetAwaiter().GetResult();
    }

    public List<House> GetHousesByOwner(uint ownerId)
    {
        return GetHousesByOwnerAsync(ownerId).GetAwaiter().GetResult();
    }

    public bool UpdateHouseOwnership(uint houseId, uint ownerId, long paidUntil)
    {
        return UpdateHouseOwnershipAsync(houseId, ownerId, paidUntil).GetAwaiter().GetResult();
    }

    public bool DeleteHouse(uint houseId)
    {
        return DeleteHouseAsync(houseId).GetAwaiter().GetResult();
    }

    // IDataStore implementation
    public House Get(uint key)
    {
        return GetHouseById(key);
    }

    public IEnumerable<House> GetAll()
    {
        return GetAllHouses();
    }

    private void MapHouseToEntity(House house, HouseEntity entity)
    {
        entity.Id = (int)house.Id;
        entity.Name = house.Name;
        entity.Owner = (int)house.Owner;
        entity.Paid = (int)house.PaidUntil;
        entity.Rent = (int)house.Rent;
        entity.TownId = (int)house.TownId;
        entity.Size = (int)house.Size;
        entity.Warnings = 0; // Default
        
        // Save guests and sub-owners as comma-separated strings in HouseLists
        SaveHouseList(entity, "guests", string.Join(",", house.Guests));
        SaveHouseList(entity, "subowners", string.Join(",", house.SubOwners));
        SaveHouseList(entity, "doors", string.Join(";", house.Doors.Select(d => $"{d.X},{d.Y},{d.Z}")));
        SaveHouseList(entity, "tiles", string.Join(";", house.Tiles.Select(t => $"{t.X},{t.Y},{t.Z}")));
        SaveHouseList(entity, "entry", $"{house.Entry.X},{house.Entry.Y},{house.Entry.Z}");
    }

    private void SaveHouseList(HouseEntity entity, string listType, string data)
    {
        entity.HouseLists ??= new List<HouseListEntity>();
        
        var existingList = entity.HouseLists.FirstOrDefault(hl => hl.ListId.ToString() == listType);
        if (existingList != null)
        {
            existingList.List = data;
        }
        else
        {
            entity.HouseLists.Add(new HouseListEntity
            {
                HouseId = entity.Id,
                ListId = listType.GetHashCode(), // Simple hash for list identification
                List = data
            });
        }
    }

    private House MapEntityToHouse(HouseEntity entity)
    {
        var house = new House
        {
            Id = (uint)entity.Id,
            Name = entity.Name,
            Owner = (uint)entity.Owner,
            PaidUntil = entity.Paid,
            Rent = (uint)entity.Rent,
            TownId = (uint)entity.TownId,
            Size = (uint)entity.Size,
            Price = (uint)(entity.Rent * 10), // Default price calculation
            Guests = new List<uint>(),
            SubOwners = new List<uint>(),
            Doors = new List<Coordinate>(),
            Tiles = new List<Coordinate>()
        };

        // Load house lists
        if (entity.HouseLists != null)
        {
            foreach (var houseList in entity.HouseLists)
            {
                switch (houseList.ListId.ToString())
                {
                    case "guests":
                        if (!string.IsNullOrEmpty(houseList.List))
                        {
                            house.Guests = houseList.List.Split(',')
                                .Where(s => uint.TryParse(s, out _))
                                .Select(uint.Parse)
                                .ToList();
                        }
                        break;
                    case "subowners":
                        if (!string.IsNullOrEmpty(houseList.List))
                        {
                            house.SubOwners = houseList.List.Split(',')
                                .Where(s => uint.TryParse(s, out _))
                                .Select(uint.Parse)
                                .ToList();
                        }
                        break;
                    case "doors":
                        if (!string.IsNullOrEmpty(houseList.List))
                        {
                            house.Doors = ParseCoordinates(houseList.List);
                        }
                        break;
                    case "tiles":
                        if (!string.IsNullOrEmpty(houseList.List))
                        {
                            house.Tiles = ParseCoordinates(houseList.List);
                        }
                        break;
                    case "entry":
                        if (!string.IsNullOrEmpty(houseList.List))
                        {
                            var coords = ParseCoordinates(houseList.List);
                            if (coords.Count > 0)
                                house.Entry = coords[0];
                        }
                        break;
                }
            }
        }

        return house;
    }

    private List<Coordinate> ParseCoordinates(string data)
    {
        if (string.IsNullOrEmpty(data))
            return new List<Coordinate>();

        return data.Split(';')
            .Select(coord =>
            {
                var parts = coord.Split(',');
                if (parts.Length == 3 &&
                    int.TryParse(parts[0], out var x) &&
                    int.TryParse(parts[1], out var y) &&
                    sbyte.TryParse(parts[2], out var z))
                {
                    return new Coordinate(x, y, z);
                }
                return new Coordinate(0, 0, 0);
            })
            .Where(c => c.X != 0 || c.Y != 0 || c.Z != 0)
            .ToList();
    }
}
