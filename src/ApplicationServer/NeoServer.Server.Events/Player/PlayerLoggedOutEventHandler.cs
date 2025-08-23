using System.Linq;
using NeoServer.Data.Interfaces;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Locker;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Player;

public class PlayerLoggedOutEventHandler(
    IPlayerRepository playerRepository,
    IPlayerDepotItemRepository playerDepotItemRepository,
    IPlayerMailItemRepository playerMailItemRepository,
    LockerManager lockerManager)
    : IEventHandler
{
    public void Execute(IPlayer player)
    {
        SavePlayer(player);
    }

    private void SavePlayer(IPlayer player)
    {
        playerRepository.SavePlayer(player);
        playerRepository.UpdatePlayerOnlineStatus(player.Id, false).Wait();
        
        SaveDepot(player);
        SaveMailInbox(player);
        
        lockerManager.Unload(player.Id);
    }

    private void SaveDepot(IPlayer player)
    {
        if (!lockerManager.Get(player.Id, out var locker)) return;
        
        var depotChest = locker.Items.FirstOrDefault() as IContainer;
        playerDepotItemRepository.Save(player, depotChest).Wait();
    }
    private void SaveMailInbox(IPlayer player)
    {
        if (!lockerManager.Get(player.Id, out var locker)) return;
        
        var mailInbox = locker.Items.ElementAtOrDefault(1) as IContainer;
        playerMailItemRepository.Save(player, mailInbox).Wait();
    }
}