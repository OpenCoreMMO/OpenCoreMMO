using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;
using NeoServer.Data.Extensions;

namespace NeoServer.Data.Configurations.ForSqLite;

public class ForSqLitePlayerDepotItemEntityConfiguration : IEntityTypeConfiguration<PlayerDepotItemEntity>
{
    public void Configure(EntityTypeBuilder<PlayerDepotItemEntity> entity)
    {
        entity.ToTable("PlayerDepotItem");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.PlayerId)
            .IsRequired();

        entity.Property(e => e.Id)
            .HasAnnotation("Sqlite:Autoincrement", true)
            .ValueGeneratedOnAdd();

        entity.Property(e => e.Amount)
            .IsRequired()
            .HasDefaultValueSql("1");

        entity.Property(e => e.ParentId)
            .HasDefaultValueSql("0");

        entity.Property(e => e.ServerId)
            .IsRequired()
            .HasDefaultValueSql("0");

        entity.HasOne(d => d.Player)
            .WithMany(p => p.PlayerDepotItems)
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

        entity.Property(e => e.Attributes)
            .HasColumnType("TEXT")
            .HasConversion(
                v => JsonExtensions.SerializeAttributes(v),
                v => JsonExtensions.DeserializeAttributes<ItemAttribute>(v)
            );
    }
}