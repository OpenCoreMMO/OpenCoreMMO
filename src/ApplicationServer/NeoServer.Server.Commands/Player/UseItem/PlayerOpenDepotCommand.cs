using System.Linq;
using NeoServer.Data.Interfaces;
using NeoServer.Data.Parsers;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Depot;
using NeoServer.Networking.Packets.Incoming;

namespace NeoServer.Server.Commands.Player.UseItem;

public class PlayerOpenDepotCommand
{
    private readonly DepotManager _depotManager;
    private readonly IItemFactory _itemFactory;
    private readonly IPlayerDepotItemRepository _playerDepotItemRepository;
    private readonly IPlayerUseService _playerUseService;

    public PlayerOpenDepotCommand(IPlayerUseService playerUseService,
        IPlayerDepotItemRepository playerDepotItemRepository, IItemFactory itemFactory, DepotManager depotManager)
    {
        _playerUseService = playerUseService;
        _playerDepotItemRepository = playerDepotItemRepository;
        _itemFactory = itemFactory;
        _depotManager = depotManager;
    }

    public void Execute(IPlayer player, Depot depot, UseItemPacket useItemPacket)
    {
        var playerDepot = LoadDepot(player, depot);

        playerDepot.SetNewLocation(useItemPacket.Location);

        _playerUseService.Use(player, playerDepot, useItemPacket.Index);
    }

    private Depot LoadDepot(IPlayer player, IItem container)
    {
        var depot = _depotManager.Get(player.Id);
        if (depot is not null && depot.Location == container.Location) return depot;

        var depotRecordsTask = _playerDepotItemRepository.GetByPlayerId(player.Id);

        depot = (Depot)_itemFactory.Create(container.Metadata, container.Location);

        var depotRecords = depotRecordsTask.Result.ToList();

        var depotItemModels = depotRecords.ToList();

        ItemEntityParser.BuildContainer(depot, depotItemModels, container.Location, _itemFactory);

        _depotManager.Load(player.Id, depot);

        return depot;
    }
}