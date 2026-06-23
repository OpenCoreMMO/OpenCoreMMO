using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Houses.Services;

/// <summary>Orchestrates house ownership changes, rent collection, and player eviction.</summary>
public interface IHouseService
{
    /// <summary>Transfer house to a new owner. Evicts old occupants, wakes beds, moves items to old owner depot.</summary>
    void SetOwner(House house, uint guid, string name, int accountId, bool updatePaidUntil, DateTime now, uint rentPeriodSeconds);

    /// <summary>
    /// Collect rent from the owner's bank. Issues warnings or evicts on non-payment.
    /// Rent is bank-balance based; a coin-store was intentionally dropped in Phase 1.
    /// If a future phase needs coin-specific rent, re-introduce it at the service layer.
    /// </summary>
    HouseRentResult PayRent(House house, IPlayer owner, DateTime now, uint rentPeriodSeconds);

    /// <summary>Kick a player out of the house. Only subowners or higher can kick lower-level players.</summary>
    bool KickPlayer(House house, IPlayer caster, IPlayer target);
}
