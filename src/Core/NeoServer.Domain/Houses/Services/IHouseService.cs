using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;

namespace NeoServer.Domain.Houses.Services;

/// <summary>Orchestrates house ownership changes, rent collection, and player eviction.</summary>
public interface IHouseService
{
    /// <summary>Transfer house to a new owner. Evicts old occupants, wakes beds, moves items to old owner depot.</summary>
    void SetOwner(House house, uint guid, string name, int accountId, bool updatePaidUntil, DateTime now, uint rentPeriodSeconds);

    /// <summary>Collect rent from the owner's bank. Issues warnings or evicts on non-payment.</summary>
    HouseRentResult PayRent(House house, IPlayer owner, ICoinTypeStore coinTypeStore, DateTime now, uint rentPeriodSeconds);

    /// <summary>Kick a player out of the house. Only subowners or higher can kick lower-level players.</summary>
    bool KickPlayer(House house, IPlayer caster, IPlayer target);
}
