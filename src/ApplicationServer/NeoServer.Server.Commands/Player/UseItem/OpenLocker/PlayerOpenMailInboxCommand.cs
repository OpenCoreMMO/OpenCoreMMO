using System;
using System.Linq;
using NeoServer.Data.Interfaces;
using NeoServer.Data.Parsers;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Locker;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Common.Contracts.Commands;

namespace NeoServer.Server.Commands.Player.UseItem.OpenLocker;

public class PlayerOpenMailInboxCommand(IPlayerUseService playerUseService,
    IPlayerMailItemRepository playerMailItemRepository, IItemFactory itemFactory, LockerManager lockerManager): ICommand
{
    public void Execute(IPlayer player, IContainer depotChest, UseItemPacket useItemPacket)
    {
        var playerDepot = LoadMailInbox(player, depotChest);

        playerDepot.SetNewLocation(useItemPacket.Location);

        playerUseService.Use(player, playerDepot, useItemPacket.Index);
    }

    private IContainer LoadMailInbox(IPlayer player, IItem container)
    {
        var locker = lockerManager.Get(player.Id);
        
        if (locker is null)
        {
            //locker should always exist when player tried to open the mailbox
            //this is an unexpected error
            throw new Exception($"Locker does not exist for player {player.Id}");
        }

        if (locker.Items.Count == 2 && locker.Items[1].Metadata.Attributes.GetAttribute(ItemTypeAttribute.Type) != "mailbox")
        {
            //Second container should always be the mailbox
            //this is an unexpected error
            throw new Exception($"Mail box is not the second container in the Locker for player {player.Id}");
        }

        if (lockerManager.IsMailboxLoaded(player.Id))
        {
            return (IContainer) locker.Items[1];
        }

        var mailbox = (IContainer) locker.Items[1];
        
        var mailRecordTask = playerMailItemRepository.GetByPlayerId(player.Id);
        
        var mailRecords = mailRecordTask.Result.ToList();

        var mailItemModels = mailRecords.ToList();

        ItemEntityParser.BuildContainer(mailbox, mailItemModels, container.Location, itemFactory);
        
        lockerManager.SetMailboxAsLoaded(player.Id);
        
        return mailbox;
    }
}