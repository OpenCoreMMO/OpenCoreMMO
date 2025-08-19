using System.Linq;
using NeoServer.Data.Interfaces;
using NeoServer.Data.Parsers;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Depot;
using NeoServer.Domain.Items.Items.Containers.Container;
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

    public void Execute(IPlayer player, Locker locker, UseItemPacket useItemPacket)
    {
        var playerDepot = LoadDepot(player, locker);

        playerDepot.SetNewLocation(useItemPacket.Location);

        _playerUseService.Use(player, playerDepot, useItemPacket.Index);
    }

    private Locker LoadDepot(IPlayer player, IItem container)
    {
        var locker = _depotManager.Get(player.Id);
        if (locker is not null) return locker;

        var depotRecordsTask = _playerDepotItemRepository.GetByPlayerId(player.Id);

        locker = (Locker)_itemFactory.Create(container.Metadata, container.Location);
        var chest = (Container) _itemFactory.Create(2594, container.Location);

        locker.AddItem(chest);

        var depotRecords = depotRecordsTask.Result.ToList();

        var depotItemModels = depotRecords.ToList();

        ItemEntityParser.BuildContainer(chest, depotItemModels, container.Location, _itemFactory);

        _depotManager.Load(player.Id, locker);

        return locker;
    }
}