using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Configurations;

public class ForSqLiteWorldRecordEntityConfiguration : IEntityTypeConfiguration<WorldRecordEntity>
{
    public void Configure(EntityTypeBuilder<WorldRecordEntity> builder)
    {
        builder.ToTable("WorldRecord");

        builder.HasKey(e => new { e.Id });

        builder.Property(e => e.Id).HasAnnotation("Sqlite:Autoincrement", true);
        builder.Property(e => e.WordId).IsRequired();
        builder.Property(e => e.Record).IsRequired();
        builder.Property(e => e.CreatedAt);

        builder.HasOne(e => e.World)
            .WithMany(p => p.WorldRecords)
            .HasForeignKey(x => x.World);
    }
}