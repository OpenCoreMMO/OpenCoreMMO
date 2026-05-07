using System.Linq;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using Serilog;

namespace NeoServer.Data.Repositories;

public class WorldRecordRepository : BaseRepository<WorldRecordEntity>, IWorldRecordRepository
{
    #region constructors

    public WorldRecordRepository(DbContextOptions<NeoContext> contextOptions, ILogger logger) : base(contextOptions,
        logger)
    {
    }

    #endregion

    public WorldRecordEntity GetLastFromWord(int worldId)
    {
        using var context = NewDbContext;
        return context.WorldRecords
            .OrderBy(c => c.CreatedAt)
            .LastOrDefault(c => c.WorldId == worldId);
    }
}