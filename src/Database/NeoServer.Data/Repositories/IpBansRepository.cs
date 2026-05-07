using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using Serilog;

namespace NeoServer.Data.Repositories;

public class IpBansRepository : BaseRepository<IpBanEntity>, IIpBansRepository
{
    public IpBansRepository(DbContextOptions<NeoContext> contextOptions, ILogger logger) : base(contextOptions,
        logger)
    {
    }

    public IpBanEntity ExistBan(string Ip)
    {
        using var context = NewDbContext;

        return context.IpBans
            .Where(x => x.Ip.Equals(Ip) && x.ExpiresAt.Date >= DateTime.UtcNow.Date)
            .SingleOrDefault();
    }
}