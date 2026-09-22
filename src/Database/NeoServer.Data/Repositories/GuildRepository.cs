using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Data.Interfaces;
using Serilog;

namespace NeoServer.Data.Repositories;

public class GuildRepository : BaseRepository<GuildEntity>, IGuildRepository
{
    #region constructors

    public GuildRepository(DbContextOptions<NeoContext> contextOptions, ILogger logger) : base(contextOptions,
        logger)
    {
    }

    #endregion

    public async Task<IEnumerable<GuildEntity>> GetAll()
    {
        await using var context = NewDbContext;
        return await context.Guilds.Include(x => x.Members).ThenInclude(x => x.Rank).ToListAsync();
    }

    public async Task<GuildEntity> GetByName(string name)
    {
        await using var context = NewDbContext;
        return await context.Guilds
            .Include(x => x.Ranks)
            .FirstOrDefaultAsync(x => x.Name == name);
    }

    public async Task<GuildEntity> GetById(int id)
    {
        await using var context = NewDbContext;
        return await context.Guilds
            .Include(x => x.Members)
            .ThenInclude(x => x.Rank)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}