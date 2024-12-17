using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Factory;

namespace NeoServer.Data.Providers.PostgreSQL;

public static class DbContextFactoryExtensions
{
    public static DbContextOptions<NeoContext> UsePostgreSQL(this DbContextFactory factory, string name)
    {
        var builder = new DbContextOptionsBuilder<NeoContext>();
        builder.UseNpgsql(name);

        return builder.Options;
    }
}