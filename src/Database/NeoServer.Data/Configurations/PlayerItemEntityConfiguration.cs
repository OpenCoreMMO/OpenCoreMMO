using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;
using NeoServer.Data.Seeds;

namespace NeoServer.Data.Configurations;

public class PlayerItemEntityConfiguration : IEntityTypeConfiguration<PlayerItemEntity>
{
    public void Configure(EntityTypeBuilder<PlayerItemEntity> entity)
    {
        entity.ToTable("PlayerItem");

        entity.HasKey(x => x.Id);

        entity.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        entity.Property(e => e.PlayerId)
            .IsRequired()
            .HasColumnType("int");

        entity.Property(e => e.ServerId)
            .IsRequired()
            .HasColumnType("int");

        entity.Property(e => e.Amount)
            .HasColumnType("smallint")
            .HasDefaultValueSql("1");

        entity.Property(e => e.ParentId)
            .HasColumnType("int")
            .HasDefaultValueSql("0");

        entity.Property(e => e.ContainerId)
            .IsRequired()
            .HasColumnType("smallint")
            .HasDefaultValueSql("0");

        entity.HasOne(d => d.Player)
            .WithMany(p => p.PlayerItems)
            .HasForeignKey(d => d.PlayerId)
            .HasConstraintName("player_items_ibfk_1");

        entity.Property(e => e.DecayTo)
            .HasColumnType("int");

        entity.Property(e => e.DecayDuration)
            .HasColumnType("int");

        entity.Property(e => e.DecayElapsed)
            .HasColumnType("int");

        entity.Property(e => e.Charges)
            .HasColumnType("int");

        PlayerItemSeed.Seed(entity);
    }
}