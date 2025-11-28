using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using NeoServer.Data.Parsers;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Items.Items;
using NeoServer.Domain.Items.Items.Containers;
using NeoServer.Domain.Repositories;
using Serilog;

namespace NeoServer.Data.Repositories.Player;

/// <summary>
///     Repository class for managing PlayerDepotItem entity.
/// </summary>
public class PlayerMailItemRepository(DbContextOptions<NeoContext> contextOptions, ILogger logger)
    : BaseRepository<PlayerMailItemEntity>(contextOptions,
            logger),
        IPlayerMailItemRepository, IPlayerMailRepository
{
    #region constructors

    #endregion

    public async Task AddParcelToInbox(int playerId, Parcel parcel)
    {
        await using var context = NewDbContext;
        await ContainerManager.Save<PlayerMailItemEntity>(playerId, parcel, context, true);
        await context.SaveChangesAsync();
    }

    public async Task AddLetterToInbox(int playerId, Letter letter)
    {
        await using var context = NewDbContext;

        var itemModel = ItemEntityParser.ToPlayerItemEntity<PlayerMailItemEntity>(letter);
        if (itemModel is null) return;

        itemModel.PlayerId = playerId;
        await context.AddAsync(itemModel);
        await context.SaveChangesAsync();
    }

    public async Task<int> GetInboxItemCount(int playerId)
    {
        await using var context = NewDbContext;
        return await context.PlayerMailItems.CountAsync(x => x.PlayerId == playerId && x.ParentId == 0);
    }

    #region public methods implementation

    public async Task<IEnumerable<PlayerMailItemEntity>> GetByPlayerId(uint id)
    {
        await using var context = NewDbContext;
        return await context.PlayerMailItems
            .Where(c => c.PlayerId == id)
            .ToListAsync();
    }

    private static async Task DeleteAll(uint playerId, NeoContext neoContext)
    {
        var items = await neoContext.PlayerMailItems.Where(x => x.PlayerId == playerId).ToListAsync();
        neoContext.PlayerMailItems.RemoveRange(items);
    }

    public async Task Save(IPlayer player, IContainer mailInbox)
    {
        await using var context = NewDbContext;

        await DeleteAll(player.Id, context);

        if (mailInbox is null) return;

        await ContainerManager.Save<PlayerMailItemEntity>(player, mailInbox, context);
        await context.SaveChangesAsync();
    }

    #endregion
}