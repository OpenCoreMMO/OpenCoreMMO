using System.Collections.Generic;
using System.Linq;
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
    public void AddParcelToInbox(int playerId, Parcel parcel)
    {
        using var context = NewDbContext;
        ContainerManager.Save<PlayerMailItemEntity>(playerId, parcel, context, true);
        context.SaveChanges();
    }

    public void AddLetterToInbox(int playerId, Letter letter)
    {
        using var context = NewDbContext;

        var itemModel = ItemEntityParser.ToPlayerItemEntity<PlayerMailItemEntity>(letter);
        if (itemModel is null) return;

        itemModel.PlayerId = playerId;
        context.Add(itemModel);
        context.SaveChanges();
    }

    public int GetInboxItemCount(int playerId)
    {
        using var context = NewDbContext;
        return context.PlayerMailItems.Count(x => x.PlayerId == playerId && x.ParentId == 0);
    }

    #region public methods implementation

    public IEnumerable<PlayerMailItemEntity> GetByPlayerId(uint id)
    {
        using var context = NewDbContext;
        return context.PlayerMailItems
            .Where(c => c.PlayerId == id)
            .ToList();
    }

    private static void DeleteAll(uint playerId, NeoContext neoContext)
    {
        var items = neoContext.PlayerMailItems.Where(x => x.PlayerId == playerId).ToList();
        neoContext.PlayerMailItems.RemoveRange(items);
    }

    public void Save(IPlayer player, IContainer mailInbox)
    {
        using var context = NewDbContext;

        DeleteAll(player.Id, context);

        if (mailInbox is null) return;

        ContainerManager.Save<PlayerMailItemEntity>(player, mailInbox, context);
        context.SaveChanges();
    }

    #endregion
}