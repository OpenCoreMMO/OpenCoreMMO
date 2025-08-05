using System.Collections.Generic;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Core.Houses;

namespace NeoServer.Domain.Common.Contracts.Services;

/// <summary>
/// Interface for house business logic operations
/// </summary>
public interface IHouseService
{
    /// <summary>
    /// Get house by ID
    /// </summary>
    House GetHouseById(uint houseId);
    
    /// <summary>
    /// Get house by position
    /// </summary>
    House GetHouseByPosition(Location position);
    
    /// <summary>
    /// Get all houses
    /// </summary>
    List<House> GetAllHouses();
    
    /// <summary>
    /// Get houses owned by a player
    /// </summary>
    List<House> GetHousesOwnedByPlayer(uint playerId);
    
    /// <summary>
    /// Purchase a house for a player
    /// </summary>
    bool PurchaseHouse(uint houseId, uint playerId);
    
    /// <summary>
    /// Leave a house (remove ownership)
    /// </summary>
    bool LeaveHouse(uint houseId, uint playerId);
    
    /// <summary>
    /// Transfer house ownership to another player
    /// </summary>
    bool TransferHouse(uint houseId, uint fromPlayerId, uint toPlayerId);
    
    /// <summary>
    /// Add a guest to a house
    /// </summary>
    bool AddGuest(uint houseId, uint ownerId, uint guestId);
    
    /// <summary>
    /// Remove a guest from a house
    /// </summary>
    bool RemoveGuest(uint houseId, uint ownerId, uint guestId);
    
    /// <summary>
    /// Add a sub-owner to a house
    /// </summary>
    bool AddSubOwner(uint houseId, uint ownerId, uint subOwnerId);
    
    /// <summary>
    /// Remove a sub-owner from a house
    /// </summary>
    bool RemoveSubOwner(uint houseId, uint ownerId, uint subOwnerId);
    
    /// <summary>
    /// Check if a player can enter a house
    /// </summary>
    bool CanEnterHouse(uint houseId, uint playerId);
    
    /// <summary>
    /// Check if a player can edit a house
    /// </summary>
    bool CanEditHouse(uint houseId, uint playerId);
    
    /// <summary>
    /// Check if a house payment is due
    /// </summary>
    bool IsHousePaymentDue(uint houseId);
    
    /// <summary>
    /// Pay house rent
    /// </summary>
    bool PayHouseRent(uint houseId, uint playerId, uint days = 30);
    
    /// <summary>
    /// Get houses that need to be cleaned (payment expired)
    /// </summary>
    List<House> GetHousesForCleaning();
    
    /// <summary>
    /// Clean expired houses
    /// </summary>
    void CleanExpiredHouses();
}
