using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;
using NeoServer.Data.Seeds;

namespace NeoServer.Data.Configurations.ForSqLite;

public class ForSqLiteWorldEntityConfiguration : IEntityTypeConfiguration<WorldEntity>
{
    public void Configure(EntityTypeBuilder<WorldEntity> builder)
    {
        builder.HasKey(e => new { e.Id });

        builder.Property(e => e.Id).HasAnnotation("Sqlite:Autoincrement", true);
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Ip).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Port).IsRequired();

        WorldModelSeed.Seed(builder);
    }
}