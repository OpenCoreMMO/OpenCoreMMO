using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Houses.Events;
using NeoServer.Domain.Repositories;

namespace NeoServer.Domain.Houses.Services;

/// <summary>Orchestrates house ownership changes, rent, and eviction.
/// Calls the pure House aggregate for domain logic, then performs
/// game-world side effects (teleport, bed wake, depot transfer)
/// and persists via IHouseRepository.</summary>
public class HouseService(
    IHouseRepository houseRepository,
    IHouseEviction eviction,
    IHouseBedWaker bedWaker,
    IHouseDepotTransfer depotTransfer) : IHouseService
{
    /// <summary>Transfer house to a new owner. Evicts old occupants, wakes beds, moves items to old owner depot.</summary>
    public void SetOwner(House house, uint guid, string name, int accountId, bool updatePaidUntil, DateTime now, uint rentPeriodSeconds)
    {
        var oldOwnerGuid = house.OwnerGuid;
        var oldOwnerAccountId = house.OwnerAccountId;

        house.SetNewOwner(guid, name, accountId, updatePaidUntil, now, rentPeriodSeconds);

        if (oldOwnerGuid != 0 && guid != oldOwnerGuid)
        {
            var entryPosition = house.EntryPosition.GetValueOrDefault();

            foreach (var tile in house.Tiles)
            {
                if (tile.Players is not null)
                {
                    foreach (var player in tile.Players)
                    {
                        if (player.Id != guid)
                            eviction.TeleportToExit(player, entryPosition);
                    }
                }
            }

            bedWaker.WakeAll(house.Beds);

            var pickupableItems = new List<IItem>();
            foreach (var tile in house.Tiles)
            {
                if (tile.AllItems is null) continue;
                foreach (var item in tile.AllItems)
                {
                    if (item is not null && item.IsPickupable)
                    {
                        pickupableItems.Add(item);
                    }
                }
            }

            if (pickupableItems.Count > 0)
            {
                depotTransfer.TransferToOwnerDepot(oldOwnerAccountId, house.TownId, pickupableItems);
            }
        }

        houseRepository.Save(house);
        EventAggregator.Invoke(new HouseOwnerChangedEvent(house, oldOwnerGuid, guid));
    }

    /// <summary>Collect rent from owner's bank. Raises warning or eviction events based on result.</summary>
    public HouseRentResult PayRent(House house, IPlayer owner, ICoinTypeStore coinTypeStore, DateTime now, uint rentPeriodSeconds)
    {
        var result = house.PayRent(owner, coinTypeStore, now, rentPeriodSeconds);

        houseRepository.Save(house);

        if (result == HouseRentResult.Warned)
            EventAggregator.Invoke(new HouseRentWarningEvent(house, owner, house.PayRentWarnings));

        if (result == HouseRentResult.Evicted)
            EventAggregator.Invoke(new HouseEvictedEvent(house));

        return result;
    }

    /// <summary>Kick a player from the house. Teleports them to the house exit on success.</summary>
    public bool KickPlayer(House house, IPlayer caster, IPlayer target)
    {
        if (!house.CanKick(caster, target))
            return false;

        eviction.TeleportToExit(target, house.EntryPosition.GetValueOrDefault());
        houseRepository.Save(house);
        return true;
    }
}
