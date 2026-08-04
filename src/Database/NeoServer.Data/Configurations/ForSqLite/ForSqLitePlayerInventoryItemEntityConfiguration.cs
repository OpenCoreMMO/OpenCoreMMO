using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;
using NeoServer.Data.Extensions;
using NeoServer.Data.Seeds;

namespace NeoServer.Data.Configurations.ForSqLite;

public class ForSqLitePlayerInventoryItemEntityConfiguration : IEntityTypeConfiguration<PlayerInventoryItemEntity>
{
    public void Configure(EntityTypeBuilder<PlayerInventoryItemEntity> entity)
    {
        entity.ToTable("PlayerInventoryItem");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id)
            .HasAnnotation("Sqlite:Autoincrement", true)
            .ValueGeneratedOnAdd();

        entity.Property(e => e.Amount)
            .IsRequired()
            .HasDefaultValueSql("1");

        entity.Property(e => e.Charges)
            .HasColumnType("int");

        entity.Property(e => e.DecayTo)
            .HasColumnType("int");

        entity.Property(e => e.DecayDuration)
            .HasColumnType("int");

        entity.Property(e => e.DecayElapsed)
            .HasColumnType("int");

        entity.Property(e => e.SlotId)
            .IsRequired();

        entity.Property(e => e.PlayerId)
            .IsRequired();

        entity.Property(e => e.ServerId)
            .IsRequired()
            .HasDefaultValueSql("0");

        entity.Property(e => e.Attributes)
            .HasColumnType("TEXT")
            .HasConversion(
                v => JsonExtensions.SerializeAllAttributes(v),
                v => JsonExtensions.DeserializeAllAttributes(v)
            );

        entity.HasOne(d => d.Player)
            .WithMany(p => p.PlayerInventoryItems)
            .HasForeignKey(d => d.PlayerId)
            .HasConstraintName("player_inventory_items_ibfk_1");

        PlayerInventorySeed.Seed(entity);
    }
}