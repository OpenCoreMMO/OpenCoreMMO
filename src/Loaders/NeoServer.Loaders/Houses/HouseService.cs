using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Domain.Core.Houses;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.World.Models.Tiles;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location.Structs;
using Serilog;

namespace NeoServer.Loaders.Houses;

/// <summary>
/// Service for managing house operations like buying, selling, etc.
/// </summary>
public class HouseService
{
    private readonly ILogger _logger;
    private readonly HousePersistenceService _persistenceService;
    private readonly IMap _map;
    private static readonly Dictionary<uint, House> _houses = new();

    public HouseService(ILogger logger, IMap map)
    {
        _logger = logger;
        _map = map;
        _persistenceService = new HousePersistenceService(logger);
    }

    /// <summary>
    /// Register houses from the startup loader
    /// </summary>
    public static void RegisterHouses(IEnumerable<House> houses)
    {
        Console.WriteLine($"[HouseService] RegisterHouses called with {houses?.Count() ?? 0} houses. Current count: {_houses.Count}");
        
        _houses.Clear();
        Console.WriteLine($"[HouseService] Cleared existing houses. New count: {_houses.Count}");

        foreach (var house in houses)
        {
            _houses[house.Id] = house;
            Console.WriteLine($"[HouseService] Registered house {house.Id}: {house.Name}");
        }
        
        Console.WriteLine($"[HouseService] Final house count: {_houses.Count}");
    }

    /// <summary>
    /// Get all houses (static method for access from other modules)
    /// </summary>
    public static List<House> GetAllHousesStatic() => _houses.Values.ToList();

    /// <summary>
    /// Get house by ID
    /// </summary>
    public House GetHouseById(uint houseId)
    {
        _houses.TryGetValue(houseId, out var house);
        return house;
    }

    /// <summary>
    /// Get house by position (checking if the position is a house tile)
    /// </summary>
    public House GetHouseByPosition(Location position)
    {
        var tile = _map[position];

        if (tile is HouseTile houseTile)
            return houseTile.House;

        return null;
    }

    /// <summary>
    /// Purchase a house for a player
    /// </summary>
    public bool PurchaseHouse(uint houseId, uint playerId)
    {
        var house = GetHouseById(houseId);
        if (house == null)
        {
            _logger.Warning("[HouseService] Attempted to purchase non-existent house {HouseId}", houseId);
            return false;
        }

        if (house.Owner != 0)
        {
            _logger.Warning("[HouseService] Attempted to purchase already owned house {HouseId} (owner: {OwnerId})", houseId, house.Owner);
            return false;
        }

        // Set the owner
        house.Owner = playerId;
        
        // Set paid until 30 days from now
        var paidUntil = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds();
        house.PaidUntil = paidUntil;

        // Persist the change
        _persistenceService.SaveHouseOwnership(houseId, playerId, paidUntil);

        _logger.Information("[HouseService] Player {PlayerId} purchased house {HouseId} ({HouseName})", playerId, houseId, house.Name);
        return true;
    }

    /// <summary>
    /// Leave a house (remove ownership)
    /// </summary>
    public bool LeaveHouse(uint houseId, uint playerId)
    {
        var house = GetHouseById(houseId);
        if (house == null)
        {
            _logger.Warning("[HouseService] Attempted to leave non-existent house {HouseId}", houseId);
            return false;
        }

        if (house.Owner != playerId)
        {
            _logger.Warning("[HouseService] Player {PlayerId} attempted to leave house {HouseId} but is not the owner (actual owner: {OwnerId})", playerId, houseId, house.Owner);
            return false;
        }

        // Remove ownership
        house.Owner = 0;
        house.PaidUntil = 0;

        // Persist the change
        _persistenceService.SaveHouseOwnership(houseId, 0, 0);

        _logger.Information("[HouseService] Player {PlayerId} left house {HouseId} ({HouseName})", playerId, houseId, house.Name);
        return true;
    }

    /// <summary>
    /// Get all houses owned by a player
    /// </summary>
    public List<House> GetHousesOwnedByPlayer(uint playerId)
        => _houses.Values.Where(h => h.Owner == playerId).ToList();

    /// <summary>
    /// Get all houses
    /// </summary>
    public List<House> GetAllHouses()
        => _houses.Values.ToList();

    /// <summary>
    /// Static method to save house ownership without requiring service instantiation
    /// </summary>
    public static void SaveHouseOwnershipStatic(uint houseId, uint ownerId, long paidUntil)
    {
        try
        {
            // Create a persistence service with null logger (will use Console.WriteLine for critical errors)
            var persistenceService = new HousePersistenceService(null); // Will auto-detect connection string
            persistenceService.SaveHouseOwnership(houseId, ownerId, paidUntil);
            
            Console.WriteLine($"[HouseService] Static save: House {houseId} ownership saved for player {ownerId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HouseService] Error in static save for house {houseId}: {ex.Message}");
        }
    }
}