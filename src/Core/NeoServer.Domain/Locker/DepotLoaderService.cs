using System;
using System.Linq;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Repositories;

namespace NeoServer.Domain.Locker;

/// <summary>
///     Loads a player's depot chest contents from the database into the locker container.
/// </summary>
public class DepotLoaderService
{
    private readonly IPlayerDepotRepository _depotRepository;
    private readonly LockerManager _lockerManager;

    public DepotLoaderService(
        IPlayerDepotRepository depotRepository,
        LockerManager lockerManager)
    {
        _depotRepository = depotRepository;
        _lockerManager = lockerManager;
    }

    /// <summary>
    ///     Loads the depot chest contents for the given player.
    ///     Returns the chest container with items populated from the database.
    /// </summary>
    public IContainer LoadDepotChest(IPlayer player, IItem container)
    {
        var locker = _lockerManager.Get(player.Id);

        if (locker is null)
            throw new Exception($"Locker does not exist for player {player.Id}");

        if (locker.Items.FirstOrDefault()?.ServerId != 2594)
            throw new Exception($"Depot chest is not the first container in the Locker for player {player.Id}");

        if (_lockerManager.IsDepotLoaded(player.Id))
            return (IContainer)locker.Items[0];

        var chest = (IContainer)locker.Items[0];

        _depotRepository.LoadDepotChest(chest, container.Location, player.Id).GetAwaiter().GetResult();

        _lockerManager.SetDepotAsLoaded(player.Id);

        return chest;
    }
}
