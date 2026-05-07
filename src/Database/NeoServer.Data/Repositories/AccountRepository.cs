using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using Serilog;

namespace NeoServer.Data.Repositories;

public class AccountRepository(DbContextOptions<NeoContext> contextOptions, ILogger logger)
    : BaseRepository<AccountEntity>(contextOptions,
        logger), IAccountRepository
{
    #region public methods implementation

    #region gets

    public AccountEntity GetAccount(string name, string password)
    {
        using var context = NewDbContext;

        return context.Accounts
            .Where(x => (x.EmailAddress.Equals(name) || x.AccountName.Equals(name)) && x.Password.Equals(password))
            .Include(x => x.Players)
            .ThenInclude(x => x.World)
            .SingleOrDefault();
    }

    public PlayerEntity GetPlayer(string accountName, string password, string charName,
        bool includeDeathList = false, bool includeKillsLastMonth = false)
    {
        using var context = NewDbContext;

        var query = context.Players.Where(x => x.Account.EmailAddress.Equals(accountName) &&
                                               x.Account.Password.Equals(password) &&
                                               x.Name.Equals(charName))
            .Include(x => x.PlayerItems)
            .Include(x => x.PlayerInventoryItems)
            .Include(x => x.World)
            .Include(x => x.Account)
            .ThenInclude(x => x.VipList)
            .ThenInclude(x => x.Player)
            .Include(x => x.GuildMember)
            .ThenInclude(x => x.Guild)
            .Include(x => x.GuildMember)
            .ThenInclude(x => x.Rank)
            .Include(x => x.PlayerStorages);

        if (includeDeathList)
            query.Include(x => x.Deaths)
                .ThenInclude(x => x.Killers);

        var result = query.AsNoTracking().SingleOrDefault();

        if (result is null) return null;

        if (includeKillsLastMonth)
        {
            var lastMonth = DateTime.UtcNow.AddMonths(-1).ToUniversalTime();
            result.KillsLastMonth = context.PlayerDeathKillers
                .Include(x => x.PlayerDeath)
                .Where(x => x.PlayerId == result.Id && x.PlayerDeath.DeathDateTime >= lastMonth)
                .Select(x => x.PlayerDeath)
                .AsNoTracking()
                .ToList();
        }

        return result;
    }

    public IList<PlayerEntity> GetOnlinePlayers(string accountName)
    {
        using var context = NewDbContext;

        return context.Players
            .Include(x => x.Account)
            .Where(x => x.Account.EmailAddress.Equals(accountName) && x.Online)
            .AsNoTracking()
            .ToList();
    }

    #endregion

    #region inserts

    public void AddPlayerToVipList(int accountId, int playerId)
    {
        using var context = NewDbContext;

        context.AccountsVipList.Add(new AccountVipListEntity
        {
            AccountId = accountId,
            PlayerId = playerId
        });

        CommitChanges(context);
    }

    #endregion

    #region updates

    public int Ban(uint accountId, string reason, uint bannedByAccountId)
    {
        using var context = NewDbContext;

        return context.Accounts
            .Where(x => x.Id == accountId)
            .ExecuteUpdate(x
                => x.SetProperty(y => y.BannedBy, bannedByAccountId)
                    .SetProperty(y => y.BanishmentReason, reason)
                    .SetProperty(y => y.BanishedAt, DateTime.UtcNow));
    }

    #endregion

    #region deletes

    public void RemoveFromVipList(int accountId, int playerId)
    {
        using var context = NewDbContext;

        var item = context.AccountsVipList.SingleOrDefault(x =>
            x.PlayerId == playerId && x.AccountId == accountId);

        if (item is null) return;

        context.AccountsVipList.Remove(item);
        CommitChanges(context);
    }

    #endregion

    #endregion
}