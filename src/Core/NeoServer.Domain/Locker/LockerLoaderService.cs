using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Items.Containers.Container;

namespace NeoServer.Domain.Locker;

/// <summary>
///     Ensures a locker exists for the given player, creating and registering it if necessary.
///     This is the first phase of depot loading: creating the locker shell with its standard
///     sub-containers (mailbox and depot chest). The second phase (populating chest contents
///     from the database) is handled by <see cref="DepotLoaderService" />.
/// </summary>
public class LockerLoaderService(IItemFactory itemFactory, LockerManager lockerManager)
{
    /// <summary>
    ///     Returns the existing locker for the given player, or creates a new one
    ///     with the standard sub-containers (mailbox, depot chest) and registers it
    ///     with the <see cref="LockerManager" />.
    /// </summary>
    /// <param name="player">The player whose locker to ensure.</param>
    /// <param name="container">The locker item metadata used when creating a new locker.</param>
    /// <returns>The existing or newly created <see cref="Locker" />.</returns>
    public Locker EnsureLocker(IPlayer player, IItem container)
    {
        var locker = lockerManager.Get(player.Id);
        if (locker is not null) return locker;

        locker = (Locker)itemFactory.Create(container.Metadata, container.Location);

        var mailInbox = (Container)itemFactory.Create(2593, new Location(0));
        var chest = (Container)itemFactory.Create(2594, new Location(1));

        locker.AddItem(mailInbox);
        locker.AddItem(chest);

        lockerManager.Load(player.Id, locker);

        return locker;
    }
}
