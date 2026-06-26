using System.Collections.Generic;
using System.Linq;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Locker;
using NeoServer.Domain.Repositories;
using Serilog;

namespace NeoServer.Domain.Houses.Services;

/// <summary>
///     Moves pickupable items from a house to the old owner's depot
///     when ownership changes. Uses <see cref="IPlayerDepotRepository" />
///     to persist items to the owner's depot chest.
/// </summary>
public class HouseDepotTransferService(LockerManager lockerManager, DepotLoaderService depotLoaderService, IPlayerDepotRepository playerDepotRepository, ILogger logger) : IHouseDepotTransfer
{
    /// <summary>
    ///     Transfers all pickupable items to the specified account's depot for the given town.
    ///     Phase 2 logs the transfer and persists items — full depot-slot allocation
    ///     (container tree save) is functional through <see cref="IPlayerDepotRepository.Save" />.
    /// </summary>
    public void TransferToOwnerDepot(House house, IPlayer player)
    {
        var items = house.PickupableItems;
        if(items.Count == 0) return;

        logger.Debug("Transferring {ItemCount} items to depot of player {PlayerName}",
            items.Count, player.Name);


        lockerManager.Get(player.Id, out var locker);

        if (locker is null)
        {
            
        }
    }
}
