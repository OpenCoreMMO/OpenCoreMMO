using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using Serilog;
using System.Linq;

namespace NeoServer.Data.Repositories;

public class ReportBugRepository : BaseRepository<ReportBugEntity>, IReportBugRepository
{
    public ReportBugRepository(DbContextOptions<NeoContext> contextOptions, ILogger logger) : base(contextOptions,
        logger)
    {
    }

    public async Task<ReportBugEntity> GetLatestReportPendingByPlayerIdAsync(uint playerId)
    {
        await using var context = NewDbContext;
        return await context.ReportBugs
            .Where(x => x.PlayerId == playerId && x.ClosedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }
}