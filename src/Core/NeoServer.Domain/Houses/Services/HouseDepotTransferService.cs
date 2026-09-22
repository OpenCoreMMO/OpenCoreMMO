using System.Collections.Generic;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Locker;
using NeoServer.Domain.Repositories;
using Serilog;

namespace NeoServer.Domain.Houses.Services;

/// <summary>
///     Moves transferable house items into a new backpack in the previous owner's shared depot.
///     When a backpack has only one slot left and more items remain, the next backpack is created
///     inside it so packing can continue.
/// </summary>
public class HouseDepotTransferService(
    LockerManager lockerManager,
    IPlayerDepotRepository playerDepotRepository,
    IItemFactory itemFactory,
    ILogger logger) : IHouseDepotTransfer
{
    public void TransferToOwnerDepot(House house, uint ownerId)
    {
        if (house is null || ownerId == 0)
            return;

        var transferItems = CollectTransferItems(house);
        if (transferItems.Count == 0)
            return;

        var depotChest = ResolveDepotChest(ownerId);
        if (depotChest is null)
            return;

        var backpack = CreateBackpack();
        if (backpack is null || backpack.Capacity == 0)
        {
            logger.Warning(
                "Could not create a backpack to store items from house {HouseId} for player {PlayerId}",
                house.Id, ownerId);
            return;
        }

        if (!TryPlaceInDepot(depotChest, backpack))
        {
            logger.Warning(
                "Depot of player {PlayerId} has no free slot for items from house {HouseId}",
                ownerId, house.Id);
            return;
        }

        Pack(backpack, transferItems);

        playerDepotRepository.Save(ownerId, depotChest).GetAwaiter().GetResult();

        logger.Debug(
            "Transferred {ItemCount} items from house {HouseId} to the depot of player {PlayerId}",
            transferItems.Count, house.Id, ownerId);
    }

    private void Pack(IContainer backpack, List<TransferItem> transferItems)
    {
        var current = backpack;

        for (var index = 0; index < transferItems.Count; index++)
        {
            var transferItem = transferItems[index];
            var moreItemsRemain = index < transferItems.Count - 1;

            if (!EnsureRoomForAnotherItem(ref current, moreItemsRemain))
            {
                logger.Warning(
                    "Stopped packing house items into the depot backpack at item {ServerId}",
                    transferItem.Item.ServerId);
                return;
            }

            Detach(transferItem);

            if (current.AddItem(transferItem.Item).Succeeded)
            {
                continue;
            }

            Restore(transferItem);
            logger.Warning(
                "Could not pack item {ServerId} into the depot backpack",
                transferItem.Item.ServerId);
            return;
        }
    }

    /// <summary>
    ///     Keeps one free slot for the next backpack when more items remain.
    ///     A backpack needs at least two slots to both store an item and hold the next backpack.
    /// </summary>
    private bool EnsureRoomForAnotherItem(ref IContainer current, bool moreItemsRemain)
    {
        var freeSlots = current.Capacity - current.SlotsUsed;
        var reserveSlotForNestedBackpack = moreItemsRemain && current.Capacity > 1;

        if (reserveSlotForNestedBackpack)
        {
            if (freeSlots > 1)
                return true;

            return OpenNestedBackpack(ref current);
        }

        if (freeSlots >= 1)
            return true;

        return OpenNestedBackpack(ref current);
    }

    private bool OpenNestedBackpack(ref IContainer current)
    {
        var nested = CreateBackpack();
        if (nested is null)
            return false;

        if (!current.AddItem(nested).Succeeded)
            return false;

        current = nested;
        return true;
    }

    private IContainer ResolveDepotChest(uint ownerId)
    {
        if (lockerManager.Get(ownerId, out var locker))
        {
            if (locker.Items.Count == 0 ||
                locker.Items[0] is not IContainer chest ||
                chest.ServerId != GameConstants.DEPOT_CHEST_SERVER_ID)
            {
                logger.Warning("Player {PlayerId} locker has no depot chest", ownerId);
                return null;
            }

            if (!lockerManager.IsDepotLoaded(ownerId))
            {
                playerDepotRepository.LoadDepotChest(chest, chest.Location, ownerId).GetAwaiter().GetResult();
                lockerManager.SetDepotAsLoaded(ownerId);
            }

            return chest;
        }

        if (itemFactory.Create(GameConstants.DEPOT_CHEST_SERVER_ID, Location.Zero) is not IContainer offlineChest)
        {
            logger.Warning("Could not create a depot chest for player {PlayerId}", ownerId);
            return null;
        }

        playerDepotRepository.LoadDepotChest(offlineChest, offlineChest.Location, ownerId).GetAwaiter().GetResult();
        return offlineChest;
    }

    private static bool TryPlaceInDepot(IContainer depotChest, IContainer backpack)
    {
        if (depotChest.AddItem(backpack).Succeeded)
            return true;

        var pending = new Queue<IContainer>();
        pending.Enqueue(depotChest);

        while (pending.Count > 0)
        {
            var container = pending.Dequeue();
            var children = new List<IItem>(container.Items);

            foreach (var child in children)
            {
                if (child is not IContainer childContainer)
                {
                    continue;
                }

                if (ReferenceEquals(childContainer, backpack))
                {
                    continue;
                }

                if (!childContainer.IsFull && childContainer.AddItem(backpack).Succeeded)
                    return true;

                pending.Enqueue(childContainer);
            }
        }

        return false;
    }

    private IContainer CreateBackpack()
    {
        return itemFactory.Create(GameConstants.BACKPACK_SERVER_ID, Location.Zero) as IContainer;
    }

    private static List<TransferItem> CollectTransferItems(House house)
    {
        var transferItems = new List<TransferItem>();

        foreach (var tile in house.Tiles)
        {
            var tileItems = tile.AllItems;
            if (tileItems is null)
            {
                continue;
            }

            foreach (var item in tileItems)
            {
                if (item is null)
                {
                    continue;
                }

                if (item.IsPickupable)
                {
                    transferItems.Add(new TransferItem(item, tile, null));
                    continue;
                }

                if (item is not IContainer container)
                {
                    continue;
                }

                var children = new List<IItem>(container.Items);
                foreach (var child in children)
                {
                    if (child is null)
                    {
                        continue;
                    }

                    transferItems.Add(new TransferItem(child, tile, container));
                }
            }
        }

        return transferItems;
    }

    private static void Detach(TransferItem transferItem)
    {
        if (transferItem.SourceContainer is not null)
        {
            var amount = transferItem.Item.Amount == 0 ? (byte)1 : transferItem.Item.Amount;
            transferItem.SourceContainer.RemoveItem(transferItem.Item, amount);
            return;
        }

        transferItem.Tile.RemoveItem(transferItem.Item);
    }

    private static void Restore(TransferItem transferItem)
    {
        if (transferItem.SourceContainer is not null)
        {
            transferItem.SourceContainer.AddItem(transferItem.Item);
            return;
        }

        transferItem.Tile.AddItem(transferItem.Item);
    }

    private readonly record struct TransferItem(IItem Item, IDynamicTile Tile, IContainer SourceContainer);
}
