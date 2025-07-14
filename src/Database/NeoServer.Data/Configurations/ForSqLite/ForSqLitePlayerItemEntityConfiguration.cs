using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;
using NeoServer.Data.Extensions;
using NeoServer.Data.Seeds;

namespace NeoServer.Data.Configurations.ForSqLite;

public class ForSqLitePlayerItemEntityConfiguration : IEntityTypeConfiguration<PlayerItemEntity>
{
    public void Configure(EntityTypeBuilder<PlayerItemEntity> entity)
    {
        entity.ToTable("PlayerItem");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id)
            .HasAnnotation("Sqlite:Autoincrement", true)
            .ValueGeneratedOnAdd();

        entity.Property(e => e.Amount)
            .IsRequired()
            .HasDefaultValueSql("1");

        entity.Property(e => e.ParentId)
            .HasDefaultValueSql("0");

        entity.Property(e => e.PlayerId)
            .IsRequired();

        entity.Property(e => e.ServerId)
            .IsRequired();

        entity.Property(e => e.ContainerId)
            .IsRequired()
            .HasDefaultValueSql("0");

        entity.HasOne(d => d.Player)
            .WithMany(p => p.PlayerItems)
            .HasForeignKey(d => d.PlayerId)
            .HasConstraintName("player_items_ibfk_1");

        entity.Property(e => e.DecayTo)
            .HasDefaultValueSql("0");

        entity.Property(e => e.DecayDuration)
            .HasDefaultValueSql("0");

        entity.Property(e => e.DecayElapsed)
            .HasDefaultValueSql("0");

        entity.Property(e => e.Charges)
            .HasDefaultValueSql("0");

        entity.Property(e => e.Attributes)
            .HasColumnType("TEXT")
            .HasConversion(
                v => JsonExtensions.SerializeAttributes(v),
                v => JsonExtensions.DeserializeAttributes<ItemAttribute>(v)
            );

        PlayerItemSeed.Seed(entity);
    }
}