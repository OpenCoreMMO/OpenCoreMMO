using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using Serilog;
using System;
using System.Threading.Tasks;
using NeoServer.Domain.Core.Houses;

namespace NeoServer.Loaders.Houses;

/// <summary>
/// Repository for house database operations
/// </summary>
public class HouseRepository
{
    private readonly string _connectionString;

    public HouseRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Insert or update house in the database (upsert operation)
    /// </summary>
    public async Task<bool> UpsertHouseAsync(House house)
    {
        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<NeoContext>();
            optionsBuilder.UseNpgsql(_connectionString);

            // Create a simple null logger for NeoContext
            var logger = new LoggerConfiguration().CreateLogger();

            await using var context = new NeoContext(optionsBuilder.Options, logger);
            
            var existingHouse = await context.Houses.FirstOrDefaultAsync(h => h.Id == house.Id);
            if (existingHouse == null)
            {
                // Insert new house
                var houseEntity = new HouseEntity
                {
                    Id = (int)house.Id,
                    Name = house.Name,
                    OwnerId = (int)house.Owner,
                    PaidUntil = house.PaidUntil,
                    Warnings = 0,
                    LastWarning = 0,
                    Rent = (int)house.Rent,
                    TownId = (int)house.TownId,
                    Size = (int)house.Size,
                    Beds = 0, // Default value
                    Doors = house.Doors.Count,
                    Tiles = house.Tiles.Count,
                    GuildId = (int)house.GuildId,
                    IsGuildHall = house.GuildId > 0,
                    BuyPrice = (int)house.Price,
                    SellPrice = (int)(house.Price * 0.9), // 90% of buy price
                    EntryX = house.Entry.X,
                    EntryY = house.Entry.Y,
                    EntryZ = house.Entry.Z,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                context.Houses.Add(houseEntity);
                Console.WriteLine($"[HouseRepository] Inserting new house {house.Id}: {house.Name}");
            }
            else
            {
                // Update existing house basic info (not ownership)
                existingHouse.Name = house.Name;
                existingHouse.Rent = (int)house.Rent;
                existingHouse.TownId = (int)house.TownId;
                existingHouse.Size = (int)house.Size;
                existingHouse.Doors = house.Doors.Count;
                existingHouse.Tiles = house.Tiles.Count;
                existingHouse.BuyPrice = (int)house.Price;
                existingHouse.SellPrice = (int)(house.Price * 0.9);
                existingHouse.EntryX = house.Entry.X;
                existingHouse.EntryY = house.Entry.Y;
                existingHouse.EntryZ = house.Entry.Z;
                existingHouse.UpdatedAt = DateTime.UtcNow;
                
                Console.WriteLine($"[HouseRepository] Updating existing house {house.Id}: {house.Name}");
            }
            
            await context.SaveChangesAsync();
            
            Console.WriteLine($"[HouseRepository] Successfully saved house {house.Id} to database");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HouseRepository] Error saving house {house.Id}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Synchronous version for upsert
    /// </summary>
    public bool UpsertHouse(House house)
    {
        try
        {
            return UpsertHouseAsync(house).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HouseRepository] Sync error saving house {house.Id}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Update house ownership in the database
    /// </summary>
    public async Task<bool> UpdateHouseOwnershipAsync(uint houseId, uint ownerId, long paidUntil)
    {
        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<NeoContext>();
            optionsBuilder.UseNpgsql(_connectionString);

            // Create a simple null logger for NeoContext
            var logger = new LoggerConfiguration().CreateLogger();

            await using var context = new NeoContext(optionsBuilder.Options, logger);
            
            var house = await context.Houses.FirstOrDefaultAsync(h => h.Id == houseId);
            if (house == null)
            {
                Console.WriteLine($"[HouseRepository] House {houseId} not found in database");
                return false;
            }

            house.OwnerId = (int)ownerId;
            house.PaidUntil = paidUntil;
            house.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            
            Console.WriteLine($"[HouseRepository] Successfully updated house {houseId} ownership to player {ownerId}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HouseRepository] Error updating house {houseId}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Synchronous version for compatibility
    /// </summary>
    public bool UpdateHouseOwnership(uint houseId, uint ownerId, long paidUntil)
    {
        try
        {
            return UpdateHouseOwnershipAsync(houseId, ownerId, paidUntil).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HouseRepository] Sync error updating house {houseId}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get a house by ID from database
    /// </summary>
    public async Task<HouseEntity?> GetHouseAsync(uint houseId)
    {
        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<NeoContext>();
            optionsBuilder.UseNpgsql(_connectionString);

            // Create a simple null logger for NeoContext
            var logger = new LoggerConfiguration().CreateLogger();

            await using var context = new NeoContext(optionsBuilder.Options, logger);
            var house = await context.Houses.FirstOrDefaultAsync(h => h.Id == (int)houseId);
            return house;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HouseRepository] Error getting house {houseId}: {ex.Message}");
            return null;
        }
    }
}
