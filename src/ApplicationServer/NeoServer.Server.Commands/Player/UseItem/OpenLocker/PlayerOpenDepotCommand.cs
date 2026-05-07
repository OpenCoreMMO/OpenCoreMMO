using System;
using System.Linq;
using NeoServer.Data.Interfaces;
using NeoServer.Data.Parsers;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Locker;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Common.Contracts.Commands;

namespace NeoServer.Server.Commands.Player.UseItem.OpenLocker;

public class PlayerOpenDepotCommand(
    IPlayerUseService playerUseService,
    IPlayerDepotItemRepository playerDepotItemRepository,
    IItemFactory itemFactory,
    LockerManager lockerManager) : ICommand
{
    public void Execute(IPlayer player, IContainer depotChest, UseItemPacket useItemPacket)
    {
        var playerDepot = LoadDepotChest(player, depotChest);

        playerDepot.SetNewLocation(useItemPacket.Location);

        playerUseService.Use(player, playerDepot, useItemPacket.Index);
    }

    private IContainer LoadDepotChest(IPlayer player, IItem container)
    {
        var locker = lockerManager.Get(player.Id);

        if (locker is null)
            //locker should always exist when player tried to open the depot chest
            //this is an unexpected error
            throw new Exception($"Locker does not exist for player {player.Id}");

        if (locker.Items.FirstOrDefault()?.ServerId != 2594)
            //First container should always be the depot chest
            //this is an unexpected error
            throw new Exception($"Depot chest is not the first container in the Locker for player {player.Id}");

        if (lockerManager.IsDepotLoaded(player.Id)) return (IContainer)locker.Items[0];

        var chest = (IContainer)locker.Items[0];

        var depotRecords = playerDepotItemRepository.GetByPlayerId(player.Id).ToList();

        var depotItemModels = depotRecords.ToList();

        ItemEntityParser.BuildContainer(chest, depotItemModels, container.Location, itemFactory);

        lockerManager.SetDepotAsLoaded(player.Id);

        return chest;
    }
}