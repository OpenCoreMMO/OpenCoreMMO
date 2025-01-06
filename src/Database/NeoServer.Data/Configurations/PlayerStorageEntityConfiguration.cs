using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;

namespace NeoServer.Data.Configurations;

public class PlayerStorageEntityConfiguration : IEntityTypeConfiguration<PlayerStorageEntity>
{
    public void Configure(EntityTypeBuilder<PlayerStorageEntity> builder)
    {
        builder.HasKey(e => new { e.PlayerId, e.Key });

        builder.Property(e => e.PlayerId).IsRequired();
        builder.Property(e => e.Key).IsRequired();
        builder.Property(e => e.Value).IsRequired();

        builder.HasOne(e => e.Player)
            .WithMany(p => p.PlayerStorages)
            .HasForeignKey(x => x.PlayerId);
    }
}