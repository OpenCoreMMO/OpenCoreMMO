using System;
using System.Collections.Generic;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Core.Houses;

/// <summary>
/// Represents a house in the game world
/// </summary>
public class House
{
    public uint Id { get; set; }
    public string Name { get; set; }
    public uint Owner { get; set; }
    public List<uint> Guests { get; set; } = new();
    public List<uint> SubOwners { get; set; } = new();
    public Coordinate Entry { get; set; }
    public uint TownId { get; set; }
    public uint Price { get; set; }
    public uint Rent { get; set; }
    public uint Size { get; set; }
    public uint GuildId { get; set; }
    public long PaidUntil { get; set; }
    public List<Coordinate> Tiles { get; set; } = new();
    public List<Coordinate> Doors { get; set; } = new();
    
    /// <summary>
    /// Check if a player can enter the house
    /// </summary>
    public bool CanEnter(uint playerId)
    {
        if (Owner == 0) return true; // Unowned house
        if (Owner == playerId) return true; // Owner
        if (SubOwners.Contains(playerId)) return true; // Sub-owner
        if (Guests.Contains(playerId)) return true; // Guest
        
        return false;
    }
    
    /// <summary>
    /// Check if a player can edit the house
    /// </summary>
    public bool CanEdit(uint playerId)
    {
        if (Owner == playerId) return true; // Owner
        if (SubOwners.Contains(playerId)) return true; // Sub-owner
        
        return false;
    }
    
    /// <summary>
    /// Check if the house is paid and not expired
    /// </summary>
    public bool IsPaid()
    {
        if (Owner == 0) return true; // Unowned houses don't need payment
        return PaidUntil > DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
    
    /// <summary>
    /// Add a guest to the house
    /// </summary>
    public bool AddGuest(uint playerId)
    {
        if (Guests.Contains(playerId)) return false;
        Guests.Add(playerId);
        return true;
    }
    
    /// <summary>
    /// Remove a guest from the house
    /// </summary>
    public bool RemoveGuest(uint playerId)
    {
        return Guests.Remove(playerId);
    }
    
    /// <summary>
    /// Add a sub-owner to the house
    /// </summary>
    public bool AddSubOwner(uint playerId)
    {
        if (SubOwners.Contains(playerId)) return false;
        SubOwners.Add(playerId);
        return true;
    }
    
    /// <summary>
    /// Remove a sub-owner from the house
    /// </summary>
    public bool RemoveSubOwner(uint playerId)
    {
        return SubOwners.Remove(playerId);
    }
    
    /// <summary>
    /// Transfer ownership to another player
    /// </summary>
    public void TransferOwnership(uint newOwnerId)
    {
        Owner = newOwnerId;
        // Reset payment when transferring ownership
        PaidUntil = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds();
    }
    
    /// <summary>
    /// Release the house (remove ownership)
    /// </summary>
    public void Release()
    {
        Owner = 0;
        Guests.Clear();
        SubOwners.Clear();
        PaidUntil = 0;
        GuildId = 0;
    }
}
