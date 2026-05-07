using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types;
using Serilog;

namespace NeoServer.Data.Repositories.Player;

/// <summary>
///     Repository class for managing PlayerDepotItem entity.
/// </summary>
public class PlayerDepotItemRepository : BaseRepository<PlayerDepotItemEntity>,
    IPlayerDepotItemRepository
{
    #region constructors

    public PlayerDepotItemRepository(DbContextOptions<NeoContext> contextOptions, ILogger logger) : base(contextOptions,
        logger)
    {
    }

    #endregion

    #region public methods implementation

    public IEnumerable<PlayerDepotItemEntity> GetByPlayerId(uint id)
    {
        using var context = NewDbContext;
        return context.PlayerDepotItems
            .Where(c => c.PlayerId == id)
            .ToList();
    }

    private static void DeleteAll(uint playerId, NeoContext neoContext)
    {
        var items = neoContext.PlayerDepotItems.Where(x => x.PlayerId == playerId).ToList();
        neoContext.PlayerDepotItems.RemoveRange(items);
    }

    public void Save(IPlayer player, IContainer depotChest)
    {
        using var context = NewDbContext;

        DeleteAll(player.Id, context);

        if (depotChest is null) return;

        ContainerManager.Save<PlayerDepotItemEntity>(player, depotChest, context);
        context.SaveChanges();
    }

    #endregion
}