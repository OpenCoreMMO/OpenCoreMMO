using System.Linq;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using Serilog;

namespace NeoServer.Data.Repositories;

public class ReportBugRepository : BaseRepository<ReportBugEntity>, IReportBugRepository
{
    public ReportBugRepository(DbContextOptions<NeoContext> contextOptions, ILogger logger) : base(contextOptions,
        logger)
    {
    }

    public ReportBugEntity GetLatestReportPendingByPlayerId(uint playerId)
    {
        using var context = NewDbContext;
        return context.ReportBugs
            .Where(x => x.PlayerId == playerId && x.ClosedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();
    }
}