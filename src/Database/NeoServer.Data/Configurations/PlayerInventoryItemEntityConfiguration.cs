using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;
using NeoServer.Data.Seeds;

namespace NeoServer.Data.Configurations;

public class PlayerInventoryItemEntityConfiguration : IEntityTypeConfiguration<PlayerInventoryItemEntity>
{
    public void Configure(EntityTypeBuilder<PlayerInventoryItemEntity> entity)
    {
        entity.HasKey(e => e.Id);

        entity.ToTable("PlayerInventoryItem");

        entity.Property(e => e.PlayerId)
            .IsRequired()
            .HasColumnType("int");

        entity.Property(e => e.ServerId)
            .IsRequired()
            .HasColumnType("int");

        entity.Property(e => e.SlotId)
            .IsRequired()
            .HasColumnType("int");

        entity.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        entity.Property(e => e.Amount)
            .HasColumnType("smallint")
            .HasDefaultValueSql("1");

        entity.HasOne(d => d.Player)
            .WithMany(p => p.PlayerInventoryItems)
            .HasForeignKey(d => d.PlayerId)
            .HasConstraintName("player_inventory_items_ibfk_1");

        PlayerInventorySeed.Seed(entity);
    }
}