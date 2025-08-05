using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Core.Houses;
using NeoServer.Domain.World.Models.Tiles;
using Serilog;

namespace NeoServer.Domain.Services;

/// <summary>
/// Service for managing house operations
/// </summary>
public class HouseService : IHouseService
{
    private readonly ILogger _logger;
    private readonly IHouseStore _houseStore;
    private readonly IMap _map;

    public HouseService(ILogger logger, IHouseStore houseStore, IMap map)
    {
        _logger = logger;
        _houseStore = houseStore;
        _map = map;
    }

    public House GetHouseById(uint houseId)
    {
        return _houseStore.GetHouseById(houseId);
    }

    public House GetHouseByPosition(Location position)
    {
        var tile = _map[position];
        if (tile is HouseTile houseTile)
            return houseTile.House;

        return null;
    }

    public List<House> GetAllHouses()
    {
        return _houseStore.GetAllHouses();
    }

    public List<House> GetHousesOwnedByPlayer(uint playerId)
    {
        return _houseStore.GetHousesByOwner(playerId);
    }

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

        // Set the owner and payment
        house.Owner = playerId;
        house.PaidUntil = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds();

        // Save to database
        var success = _houseStore.SaveHouse(house);
        if (success)
        {
            _logger.Information("[HouseService] Player {PlayerId} purchased house {HouseId} ({HouseName})", playerId, houseId, house.Name);
        }

        return success;
    }

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

        // Release the house
        house.Release();

        // Save to database
        var success = _houseStore.SaveHouse(house);
        if (success)
        {
            _logger.Information("[HouseService] Player {PlayerId} left house {HouseId} ({HouseName})", playerId, houseId, house.Name);
        }

        return success;
    }

    public bool TransferHouse(uint houseId, uint fromPlayerId, uint toPlayerId)
    {
        var house = GetHouseById(houseId);
        if (house == null || house.Owner != fromPlayerId)
        {
            _logger.Warning("[HouseService] Invalid house transfer from {FromPlayerId} to {ToPlayerId} for house {HouseId}", fromPlayerId, toPlayerId, houseId);
            return false;
        }

        house.TransferOwnership(toPlayerId);

        var success = _houseStore.SaveHouse(house);
        if (success)
        {
            _logger.Information("[HouseService] House {HouseId} transferred from player {FromPlayerId} to player {ToPlayerId}", houseId, fromPlayerId, toPlayerId);
        }

        return success;
    }

    public bool AddGuest(uint houseId, uint ownerId, uint guestId)
    {
        var house = GetHouseById(houseId);
        if (house == null || !house.CanEdit(ownerId))
        {
            return false;
        }

        var success = house.AddGuest(guestId);
        if (success)
        {
            _houseStore.SaveHouse(house);
            _logger.Debug("[HouseService] Added guest {GuestId} to house {HouseId}", guestId, houseId);
        }

        return success;
    }

    public bool RemoveGuest(uint houseId, uint ownerId, uint guestId)
    {
        var house = GetHouseById(houseId);
        if (house == null || !house.CanEdit(ownerId))
        {
            return false;
        }

        var success = house.RemoveGuest(guestId);
        if (success)
        {
            _houseStore.SaveHouse(house);
            _logger.Debug("[HouseService] Removed guest {GuestId} from house {HouseId}", guestId, houseId);
        }

        return success;
    }

    public bool AddSubOwner(uint houseId, uint ownerId, uint subOwnerId)
    {
        var house = GetHouseById(houseId);
        if (house == null || house.Owner != ownerId)
        {
            return false;
        }

        var success = house.AddSubOwner(subOwnerId);
        if (success)
        {
            _houseStore.SaveHouse(house);
            _logger.Debug("[HouseService] Added sub-owner {SubOwnerId} to house {HouseId}", subOwnerId, houseId);
        }

        return success;
    }

    public bool RemoveSubOwner(uint houseId, uint ownerId, uint subOwnerId)
    {
        var house = GetHouseById(houseId);
        if (house == null || house.Owner != ownerId)
        {
            return false;
        }

        var success = house.RemoveSubOwner(subOwnerId);
        if (success)
        {
            _houseStore.SaveHouse(house);
            _logger.Debug("[HouseService] Removed sub-owner {SubOwnerId} from house {HouseId}", subOwnerId, houseId);
        }

        return success;
    }

    public bool CanEnterHouse(uint houseId, uint playerId)
    {
        var house = GetHouseById(houseId);
        return house?.CanEnter(playerId) ?? false;
    }

    public bool CanEditHouse(uint houseId, uint playerId)
    {
        var house = GetHouseById(houseId);
        return house?.CanEdit(playerId) ?? false;
    }

    public bool IsHousePaymentDue(uint houseId)
    {
        var house = GetHouseById(houseId);
        return house != null && !house.IsPaid();
    }

    public bool PayHouseRent(uint houseId, uint playerId, uint days = 30)
    {
        var house = GetHouseById(houseId);
        if (house == null || house.Owner != playerId)
        {
            return false;
        }

        // Extend payment by the specified days
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var newPaidUntil = Math.Max(house.PaidUntil, currentTime) + (days * 24 * 60 * 60);
        house.PaidUntil = newPaidUntil;

        var success = _houseStore.SaveHouse(house);
        if (success)
        {
            _logger.Information("[HouseService] Player {PlayerId} paid rent for house {HouseId} for {Days} days", playerId, houseId, days);
        }

        return success;
    }

    public List<House> GetHousesForCleaning()
    {
        return GetAllHouses().Where(h => h.Owner != 0 && !h.IsPaid()).ToList();
    }

    public void CleanExpiredHouses()
    {
        var expiredHouses = GetHousesForCleaning();
        foreach (var house in expiredHouses)
        {
            _logger.Information("[HouseService] Cleaning expired house {HouseId} ({HouseName}) owned by player {PlayerId}", house.Id, house.Name, house.Owner);
            house.Release();
            _houseStore.SaveHouse(house);
        }
    }
}
