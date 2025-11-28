using System.Linq;
using NeoServer.Data.Interfaces;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.Locker;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Commands;

namespace NeoServer.Server.Commands.Player;

public class PlayerLogOutCommand(
    IGameServer gameServer,
    IPlayerRepository playerRepository,
    IPlayerDepotItemRepository playerDepotItemRepository,
    IPlayerMailItemRepository playerMailItemRepository,
    LockerManager lockerManager,
    ITradeService tradeService,
    PlayerChannelService playerChannelService,
    IMap map) : ICommand
{
    public void Execute(IPlayer player, bool forced = false)
    {
        if (player.IsDead) return;
        if (!player.Logout(forced) && !forced) return;

        gameServer.CreatureManager.RemovePlayer(player);

        // Notify spectators
        foreach (var spectator in map.GetSpectators(player.Location)) spectator.OnSpectatorLoggedOut(player);

        // Exit all channels
        playerChannelService.ExitChannels(player);

        // Cancel player trade
        tradeService.Cancel(player);

        // Save player to database
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