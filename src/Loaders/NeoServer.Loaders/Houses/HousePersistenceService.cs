using System;
using System.IO;
using System.Text.Json;
using Serilog;
using System.Collections.Generic;
using NeoServer.Domain.Core.Houses;

namespace NeoServer.Loaders.Houses;

/// <summary>
/// Service responsible for persisting house ownership changes
/// </summary>
public class HousePersistenceService
{
    private readonly ILogger _logger;
    private readonly HouseRepository _houseRepository;

    public HousePersistenceService(ILogger logger, string connectionString = null)
    {
        _logger = logger;
        
        var actualConnectionString = connectionString ?? GetConnectionStringFromConfig();
        if (string.IsNullOrEmpty(actualConnectionString))
        {
            throw new InvalidOperationException("Database connection string not found. Cannot proceed without database connection for house persistence.");
        }
        
        _houseRepository = new HouseRepository(actualConnectionString);
    }

    private string GetConnectionStringFromConfig()
    {
        try
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            if (File.Exists(configPath))
            {
                var configJson = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<JsonElement>(configJson);
                
                if (config.TryGetProperty("database", out var database) &&
                    database.TryGetProperty("connections", out var connections) &&
                    connections.TryGetProperty("POSTGRESQL", out var postgresConnection))
                {
                    return postgresConnection.GetString();
                }
            }
        }
        catch (Exception ex)
        {
            if (_logger != null)
                _logger.Warning(ex, "[HousePersistence] Could not read database connection from config");
            else
                Console.WriteLine($"[HousePersistence] Could not read database connection from config: {ex.Message}");
        }
        return null;
    }

    /// <summary>
    /// Synchronize houses to database
    /// </summary>
    public void SynchronizeHousesToDatabase(List<House> houses)
    {
        foreach (var house in houses)
        {
            _houseRepository.UpsertHouseAsync(house).Wait();
        }
    }

    /// <summary>
    /// Load house ownership data from database and apply to house objects
    /// </summary>
    public void LoadHouseOwnershipFromDatabase(List<House> houses)
    {
        foreach (var house in houses)
        {
            try
            {
                var houseFromDb = _houseRepository.GetHouseAsync(house.Id).Result;
                if (houseFromDb != null)
                {
                    house.Owner = (uint)houseFromDb.OwnerId;
                    house.PaidUntil = houseFromDb.PaidUntil;
                    Console.WriteLine($"[HousePersistenceService] Loaded house {house.Id} ownership: Owner={house.Owner}, PaidUntil={house.PaidUntil}");
                }
                else
                {
                    Console.WriteLine($"[HousePersistenceService] No ownership data found for house {house.Id}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HousePersistenceService] Error loading ownership for house {house.Id}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Save house ownership data to PostgreSQL database
    /// </summary>
    public void SaveHouseOwnership(uint houseId, uint ownerId, long paidUntil)
    {
        try
        {
            // Save directly to PostgreSQL database only
            SaveToDatabase(houseId, ownerId, paidUntil);
        }
        catch (Exception ex)
        {
            if (_logger != null)
                _logger.Error(ex, "[HousePersistence] Error saving house ownership for house {HouseId}", houseId);
            else
                Console.WriteLine($"[HousePersistence] Error saving house ownership for house {houseId}: {ex.Message}");
        }
    }

    /// <summary>
    /// Save to PostgreSQL database
    /// </summary>
    private void SaveToDatabase(uint houseId, uint ownerId, long paidUntil)
    {
        try
        {
            bool success = _houseRepository.UpdateHouseOwnership(houseId, ownerId, paidUntil);
            
            if (success)
            {
                if (_logger != null)
                    _logger.Information("[HousePersistence] Successfully saved house {HouseId} to PostgreSQL database for player {OwnerId}", houseId, ownerId);
                else
                    Console.WriteLine($"[HousePersistence] Successfully saved house {houseId} to PostgreSQL database for player {ownerId}");
            }
            else
            {
                if (_logger != null)
                    _logger.Warning("[HousePersistence] Failed to save house {HouseId} to PostgreSQL database", houseId);
                else
                    Console.WriteLine($"[HousePersistence] Failed to save house {houseId} to PostgreSQL database");
            }
        }
        catch (Exception ex)
        {
            if (_logger != null)
                _logger.Warning(ex, "[HousePersistence] Failed to save to database for house {HouseId}", houseId);
            else
                Console.WriteLine($"[HousePersistence] Failed to save to database for house {houseId}: {ex.Message}");
            
            throw; // Re-throw para que o erro seja tratado no método principal
        }
    }
}
