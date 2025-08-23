using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Items.Containers.Container;
using NeoServer.Domain.Locker;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Common.Contracts.Commands;

namespace NeoServer.Server.Commands.Player.UseItem.OpenLocker;

public class PlayerOpenLockerCommand : ICommand
{
    private readonly LockerManager _lockerManager;
    private readonly IItemFactory _itemFactory;
    private readonly IPlayerUseService _playerUseService;

    public PlayerOpenLockerCommand(IPlayerUseService playerUseService, IItemFactory itemFactory,
        LockerManager lockerManager)
    {
        _playerUseService = playerUseService;
        _itemFactory = itemFactory;
        _lockerManager = lockerManager;
    }

    public void Execute(IPlayer player, Locker locker, UseItemPacket useItemPacket)
    {
        var playerDepot = LoadLocker(player, locker);

        playerDepot.SetNewLocation(useItemPacket.Location);

        _playerUseService.Use(player, playerDepot, useItemPacket.Index);
    }

    private Locker LoadLocker(IPlayer player, IItem container)
    {
        var locker = _lockerManager.Get(player.Id);
        if (locker is not null) return locker;

        locker = (Locker)_itemFactory.Create(container.Metadata, container.Location);

        var chest = (Container)_itemFactory.Create(2594, new Location(0));
        var mailInbox = (Container)_itemFactory.Create(2593, new Location(1));

        locker.AddItem(mailInbox);
        locker.AddItem(chest);

        _lockerManager.Load(player.Id, locker);

        return locker;
    }
}