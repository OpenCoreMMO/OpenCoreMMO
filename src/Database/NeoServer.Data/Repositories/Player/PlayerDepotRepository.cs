using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using NeoServer.Data.Parsers;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Repositories;
using Serilog;

namespace NeoServer.Data.Repositories.Player;

/// <summary>
///     Repository class for managing PlayerDepotItem entity.
/// </summary>
public class PlayerDepotRepository : BaseRepository<PlayerDepotItemEntity>,
    //IPlayerDepotItemRepository, 
    IPlayerDepotRepository
{
    private readonly IItemFactory _itemFactory;

    #region constructors

    public PlayerDepotRepository(DbContextOptions<NeoContext> contextOptions, ILogger logger, IItemFactory itemFactory) : base(contextOptions,
        logger)
    {
        _itemFactory = itemFactory;
    }

    #endregion

    #region public methods implementation

    public async Task<IEnumerable<PlayerDepotItemEntity>> GetByPlayerId(uint id)
    {
        await using var context = NewDbContext;
        return await context.PlayerDepotItems
            .Where(c => c.PlayerId == id)
            .ToListAsync();
    }
    
    public async Task LoadDepotChest(IContainer chest, Location depotLocation, uint playerId)
    {
        var depotRecords = await GetByPlayerId(playerId);
        var depotItemModels = depotRecords.ToList();

        ItemEntityParser.BuildContainer(chest, depotItemModels, depotLocation, _itemFactory);
    }

    private static async Task DeleteAll(uint playerId, NeoContext neoContext)
    {
        var items = await neoContext.PlayerDepotItems.Where(x => x.PlayerId == playerId).ToListAsync();
        neoContext.PlayerDepotItems.RemoveRange(items);
    }

    public async Task Save(IPlayer player, IContainer depotChest)
    {
        await using var context = NewDbContext;

        await DeleteAll(player.Id, context);

        if (depotChest is null) return;

        await ContainerManager.Save<PlayerDepotItemEntity>(player, depotChest, context);
        await context.SaveChangesAsync();
    }

    #endregion
}