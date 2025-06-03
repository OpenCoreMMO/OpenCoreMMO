using NeoServer.Data.Interfaces;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Systems.Depot;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Player;

public class PlayerLoggedOutEventHandler : IEventHandler
{
    private readonly DepotManager _depotManager;
    private readonly IPlayerDepotItemRepository _playerDepotItemRepository;
    private readonly IPlayerRepository _playerRepository;

    public PlayerLoggedOutEventHandler(IPlayerRepository playerRepository,
        IPlayerDepotItemRepository playerDepotItemRepository,
        DepotManager depotManager)
    {
        _playerRepository = playerRepository;
        _playerDepotItemRepository = playerDepotItemRepository;
        _depotManager = depotManager;
    }

    public void Execute(IPlayer player)
    {
        SavePlayer(player);
    }

    private void SavePlayer(IPlayer player)
    {
        _playerRepository.SavePlayer(player);
        _playerRepository.UpdatePlayerOnlineStatus(player.Id, false).Wait();
        SaveDepot(player);
    }

    private void SaveDepot(IPlayer player)
    {
        if (!_depotManager.Get(player.Id, out var depot)) return;
        _playerDepotItemRepository.Save(player, depot).Wait();

        _depotManager.Unload(player.Id);
    }
}