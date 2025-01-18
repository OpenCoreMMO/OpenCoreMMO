using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;
using NeoServer.Data.Seeds;

namespace NeoServer.Data.Configurations;

public class WorldEntityConfiguration : IEntityTypeConfiguration<WorldEntity>
{
    public void Configure(EntityTypeBuilder<WorldEntity> builder)
    {
        builder.HasKey(e => new { e.Id });

        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.Ip).IsRequired();
        builder.Property(e => e.Port).IsRequired();
        builder.Property(e => e.MaxCapacity).HasDefaultValue(100).IsRequired();
        builder.Property(w => w.RequiresPremium).IsRequired();
        builder.Property(w => w.TransferEnabled).IsRequired();
        builder.Property(w => w.AntiCheatEnabled).IsRequired();
        builder.Property(w => w.CreatedAt).IsRequired();

        builder.Property(w => w.Continent)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(w => w.PvpType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(w => w.Mode)
            .HasConversion<string>()
            .HasMaxLength(50);

        WorldModelSeed.Seed(builder);
    }
}