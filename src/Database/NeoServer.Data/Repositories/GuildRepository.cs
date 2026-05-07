using System.Collections.Generic;
using System.Linq;
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

    public new IEnumerable<GuildEntity> GetAll()
    {
        using var context = NewDbContext;
        return context.Guilds.Include(x => x.Members).ThenInclude(x => x.Rank).ToList();
    }

    public GuildEntity GetByName(string name)
    {
        using var context = NewDbContext;
        return context.Guilds.FirstOrDefault(x => x.Name == name);
    }

    public GuildEntity GetById(int id)
    {
        using var context = NewDbContext;
        return context.Guilds
            .Include(x => x.Members)
            .ThenInclude(x => x.Rank)
            .FirstOrDefault(x => x.Id == id);
    }
}