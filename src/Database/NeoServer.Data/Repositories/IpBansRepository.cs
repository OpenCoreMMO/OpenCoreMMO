using System;
using System.Linq;
using System.Threading.Tasks;
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

    public async Task<IpBanEntity> GetBan(string Ip)
    {
        await using var context = NewDbContext;

        return await context.IpBans
            .Where(x => x.Ip.Equals(Ip) && x.ExpiresAt.Date >= DateTime.UtcNow.Date)
            .SingleOrDefaultAsync();
    }
}